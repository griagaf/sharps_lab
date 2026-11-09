using System.Text.Json;
using Lab6.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Lab6.Teacher.Queue;

public sealed class RabbitLessonPublisher : ILessonPublisher, IAsyncDisposable
{
    // С отслеживанием подтверждений BasicPublishAsync завершается только после ack брокера.
    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true
    );

    // Persistent: сообщение переживёт перезапуск брокера, пока История его не забрала.
    private static readonly BasicProperties MessageProperties = new BasicProperties
    {
        Persistent = true,
        ContentType = "application/json",
    };

    private readonly ConnectionFactory _factory;
    private readonly TimeSpan _publishTimeout;
    private IConnection? _connection;

    public RabbitLessonPublisher(IOptions<TeacherOptions> options)
    {
        TeacherOptions teacher = options.Value;

        _factory = new ConnectionFactory
        {
            Uri = teacher.BrokerUrl!,
            ClientProvidedName = $"teacher-{teacher.Subject}",
        };

        _publishTimeout = teacher.PublishTimeout;
    }

    /// <summary>Подключается к брокеру и объявляет очередь. Вызывается при старте, до первой пары.</summary>
    /// <exception cref="BrokerUnreachableException">Брокер недоступен или не пустил с этим логином и паролем.</exception>
    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _connection = await _factory.CreateConnectionAsync(cancellationToken);

        // Те же параметры объявляет История: кто стартует первым, тот и создаёт очередь.
        await using IChannel channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            HistoryQueues.Lessons, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(LessonHeld lesson, CancellationToken cancellationToken)
    {
        IConnection connection = _connection
            ?? throw new InvalidOperationException("Публикация до подключения к брокеру: не вызван ConnectAsync");

        byte[] body = JsonSerializer.SerializeToUtf8Bytes(lesson, ContractJson.Options);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_publishTimeout);

        try
        {
            await using IChannel channel = await connection.CreateChannelAsync(ChannelOptions, timeout.Token);

            // mandatory: если очереди нет, брокер вернёт сообщение, а не выбросит его молча.
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: HistoryQueues.Lessons,
                mandatory: true,
                basicProperties: MessageProperties,
                body: body,
                cancellationToken: timeout.Token);
        }
        catch (Exception e) when (IsBrokerFailure(e, cancellationToken))
        {
            throw new BrokerException($"Не удалось записать в очередь пару {lesson.Lesson}: {Describe(e)}", e);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    // IOException — обрыв сокета. Отмена, о которой никто не просил, — это истёкший PublishTimeout.
    private static bool IsBrokerFailure(Exception e, CancellationToken cancellationToken)
    {
        return e is RabbitMQClientException or IOException
            || (e is OperationCanceledException && !cancellationToken.IsCancellationRequested);
    }

    private string Describe(Exception e)
    {
        return e switch
        {
            OperationCanceledException => $"брокер не ответил за {_publishTimeout.TotalSeconds} с",
            PublishException => "брокер не принял сообщение",
            // Сама причина (например, брокер недоступен) лежит во вложенном исключении.
            _ => e.GetBaseException().Message,
        };
    }
}
