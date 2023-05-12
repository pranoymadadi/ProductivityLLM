using Newtonsoft.Json;

namespace ProductivityLLM
{
    public class WorkItem
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? title { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? description { get; set; }

        [JsonProperty(PropertyName = "Resolved By", NullValueHandling = NullValueHandling.Ignore)]
        public string? resolvedBy { get; set; }

        [JsonProperty(PropertyName = "Assigned To", NullValueHandling = NullValueHandling.Ignore)]
        public string? assignedTo { get; set; }

        [JsonProperty(PropertyName = "Tags", NullValueHandling = NullValueHandling.Ignore)]
        public string? tags { get; set; }
    }
}
