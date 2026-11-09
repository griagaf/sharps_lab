using System.Text.Json;
using Lab6.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Lab6.History;

public class LessonQueueConsumer : BackgroundService
{
    // Сколько неподтверждённых сообщений брокер отдаёт за раз.
    private const ushort PrefetchCount = 16;

    private readonly Uri _brokerUrl;
    private readonly IHistoryStore _store;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<LessonQueueConsumer> _logger;

    public LessonQueueConsumer(
        IOptions<HistoryOptions> options,
        IHistoryStore store,
        IHostApplicationLifetime lifetime,
        ILogger<LessonQueueConsumer> logger)
    {
        // Наличие адреса проверено при старте.
        _brokerUrl = options.Value.BrokerUrl!;
        _store = store;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // При обрыве связи клиент сам переподключается и восстанавливает подписку.
        var factory = new ConnectionFactory { Uri = _brokerUrl, ClientProvidedName = "history" };

        IConnection connection;

        try
        {
            connection = await factory.CreateConnectionAsync(stoppingToken);
        }
        catch (BrokerUnreachableException e)
        {
            // Без очереди История не получит ни одной пары, поэтому работать дальше бессмысленно.
            _logger.LogCritical(
                "Не удалось подключиться к RabbitMQ {Host}:{Port}: {Reason}",
                _brokerUrl.Host,
                _brokerUrl.Port,
                e.GetBaseException().Message);

            Environment.ExitCode = 1;
            _lifetime.StopApplication();
            return;
        }

        await using (connection)
        await using (IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
        {
            // Те же параметры объявляют и преподаватели: кто стартует первым, тот и создаёт очередь.
            await channel.QueueDeclareAsync(
                HistoryQueues.Lessons, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, delivery) => HandleAsync(channel, delivery);

            await channel.BasicConsumeAsync(HistoryQueues.Lessons, autoAck: false, consumer, stoppingToken);
            _logger.LogInformation("Читаю очередь {Queue}", HistoryQueues.Lessons);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Неподтверждённые сообщения брокер вернёт в очередь при закрытии канала.
                _logger.LogInformation("Перестаю читать очередь {Queue}: История останавливается", HistoryQueues.Lessons);
            }
        }
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery)
    {
        LessonHeld? message = Parse(delivery.Body);

        if (message is null)
        {
            // Битое сообщение не станет лучше от повторов, поэтому выбрасываем его, а не возвращаем в очередь.
            await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false);
            return;
        }

        _store.Save(message.Lesson, message.Asked);
        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);

        _logger.LogInformation(
            "Записана пара {Lesson}: {Asked}", message.Lesson, message.Asked ? "спросили" : "не спросили");
    }

    /// <returns>Сообщение или null, если оно битое: об этом пишется в лог.</returns>
    private LessonHeld? Parse(ReadOnlyMemory<byte> body)
    {
        LessonHeld? message;

        try
        {
            message = JsonSerializer.Deserialize<LessonHeld>(body.Span, ContractJson.Options);
        }
        catch (JsonException e)
        {
            _logger.LogWarning("Выброшено сообщение, которое не удалось разобрать: {Reason}", e.Message);
            return null;
        }

        if (message is null)
        {
            _logger.LogWarning("Выброшено пустое сообщение");
            return null;
        }

        string? error = LessonValidation.Validate(message.Lesson.Day, message.Lesson.Subject);

        if (error is not null)
        {
            _logger.LogWarning("Выброшено сообщение о паре {Lesson}: {Reason}", message.Lesson, error);
            return null;
        }

        return message;
    }
}
