using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SmartFlowBackend.Domain.Entities;

namespace SmartFlowBackend.Domain.Contracts;

public class GetBalanceResponse
{
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    [JsonPropertyName("balance")]
    public required float Balance { get; set; }
}
