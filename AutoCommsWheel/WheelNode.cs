using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoCommsWheel
{
    public class WheelNode
    {
        /// <summary>
        /// the "name" key in the json, represents the slice name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The "content" key in the json, represents a part of the content to be put together for the final message. 
        /// Can be empty in which case name takes over
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// The "subsection" key in the json. we represent this by storing Wheel nodes in here.
        /// If a wheel node doesn't have more subsections, subsections stays null
        /// </summary>
        [JsonPropertyName("subsection")]
        public List<WheelNode>? Subsections { get; set; }

        /// <summary>
        /// The "invoke" key in the json. we represent this as text for now and later add the subsections.
        /// </summary>
        [JsonPropertyName("invoke")]
        public string? Invoke { get; set; }

        /// <summary>
        /// The "id" key in the json. This key serves to replace the name / content of the node with a code generated value through per-game related data like player names for example.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The sentence builder will always take Text. Text gives content but if no content is in a node it gives name instead. 
        /// this lets us not have to write content when it's the same as name
        /// </summary>
        [JsonIgnore]
        public string Text => Content ?? Name;

        public WheelNode() { }

        /// <summary>
        /// Method to deep clone nodes so the node and all it's subnodes so the cloned node is not linked to other nodes called in code anymore.
        /// </summary>
        /// <param name="wheelNode"></param>
        /// <returns></returns>
        public static WheelNode CloneNode(WheelNode wheelNode)
        {
            return new WheelNode
            {
                Content = wheelNode.Content,
                Id = wheelNode.Id,
                Invoke = wheelNode.Invoke,
                Name = wheelNode.Name,
                Subsections = wheelNode.Subsections?.Select(CloneNode).ToList()
            };
        }
    }
}
