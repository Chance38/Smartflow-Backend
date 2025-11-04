using System.Text.Json.Serialization;

namespace Application.Contract;

public class GetBalanceResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("balance")]
    public required float Balance { get; set; }
}
