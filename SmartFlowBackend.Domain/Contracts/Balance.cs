using System.Text.Json.Serialization;

namespace SmartFlowBackend.Domain.Contracts;

public class GetBalanceResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("balance")]
    public required float Balance { get; set; }
}
