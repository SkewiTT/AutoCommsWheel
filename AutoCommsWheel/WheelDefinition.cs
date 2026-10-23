using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoCommsWheel
{
    public class WheelDefinition
    {
        /// <summary>
        /// Holds the base Wheel options, corresponds to main in the json file
        /// </summary>
        [JsonPropertyName("main")]
        public List<WheelNode> Main { get; set; }
        
        /// <summary>
        /// Dictionary where key is the json key in json and value is the wheel, references wheel in the json file 
        /// </summary>
        [JsonPropertyName("wheels")]
        public Dictionary<string, List<WheelNode>>? Wheels { get; set; }
    }
}
