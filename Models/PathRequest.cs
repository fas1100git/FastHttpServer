using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FastHttpServer.Models
{
    internal class PathRequest
    {
        [NotNull]
        [JsonPropertyName("from")]
        public required string From { get; set; }

        [NotNull]
        [JsonPropertyName("to")]
        public required string To { get; set; }
    }
}
