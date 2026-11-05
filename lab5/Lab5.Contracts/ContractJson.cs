using System.Text.Json;

namespace Lab5.Contracts;

/// <summary>
/// Как сервисы читают JSON друг друга. Все поля контрактов обязательны:
/// иначе {} молча разобрался бы в значения по умолчанию, например Asked = false — «не спросили».
/// </summary>
public static class ContractJson
{
    /// <summary>Для клиентов: передаётся в ReadFromJsonAsync.</summary>
    public static JsonSerializerOptions Options { get; } = Configure(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    /// <summary>Для серверов: настраивает уже существующие параметры ASP.NET.</summary>
    public static JsonSerializerOptions Configure(JsonSerializerOptions options)
    {
        options.RespectRequiredConstructorParameters = true;
        return options;
    }
}
