using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIT160Grader
{
    public partial class ValidationResponse
    {
        [JsonProperty("messages")]
        public Message[] Messages { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("lastLine")]
        public long LastLine { get; set; }

        [JsonProperty("lastColumn")]
        public long LastColumn { get; set; }

        [JsonProperty("firstColumn")]
        public long FirstColumn { get; set; }

        [JsonProperty("message")]
        public string MessageMessage { get; set; }

        [JsonProperty("extract")]
        public string Extract { get; set; }

        [JsonProperty("hiliteStart")]
        public long HiliteStart { get; set; }

        [JsonProperty("hiliteLength")]
        public long HiliteLength { get; set; }
    }
}
