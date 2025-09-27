using System.Text.Json.Serialization;

namespace Domain.Contract;

public class OkSituation
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }
}

public class ClientErrorSituation
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("errorMessage")]
    public required string ErrorMessage { get; set; }
}

public class ServerErrorSituation
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("errorMessage")]
    public required string ErrorMessage { get; set; }
}
