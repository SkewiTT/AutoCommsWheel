using System.IO;
using System.Text.Json;
using System.Windows;

namespace AutoCommsWheel
{
    public static class Helpers
    {
        private const string AppFolderName = "AutoCommsWheel";
        public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);
        public static string Wheels => Path.Combine(Root, "Wheels");

        private const int StackOverflowRangeMax = 12;

        /// <summary>
        /// Checks if a window is already open of said window type
        /// </summary>
        /// <typeparam name="T">The window type</typeparam>
        /// <param name="name">The name of the window</param>
        /// <returns>True if window is open false if not</returns>
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>
        /// Deserializes the Json into a wheel definition
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The wheel definition or null if it failed</returns>
        public static WheelDefinition? LoadWheelFile(string path)
        {
            try
            {
                WheelDefinition? definition = JsonSerializer.Deserialize<WheelDefinition>(File.ReadAllText(path), _jsonOptions);

                // Valid Json that isn't a usable wheel (like {}) counts as invalid
                if (definition?.Main == null || definition.Main.Count == 0) return null;

                ResolveWheel(definition);

                return definition;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static WheelDefinition? ResolveWheel(WheelDefinition wheel)
        {
            // For each of the base nodes in main run the cycle through the branches
            wheel.Main.ForEach(wn => CycleWheelNode(wn, wheel, 0));
            return wheel;
        }

        public static WheelNode? CycleWheelNode(WheelNode wheelNode, WheelDefinition wheel, int depth)
        {
            // If we do more cycles than the estimated stack overflow range, we return null.
            // No wheel will realistically go above 20 tabs and it would lead to a stack overflow.
            if (depth > StackOverflowRangeMax) throw new InvalidOperationException("Wheel is nested too deeply, a wheel may invoke itself");


            // In the case that there is both "invoke" and "subsection" on a wheel node we need to perform the invoke first and then apply the subsection onto each invoked node.
            if (wheelNode.Subsections != null && wheelNode.Invoke != null)
            {
                // Save the subsections currently on the node
                List<WheelNode> savedSubsecs = new List<WheelNode>(wheelNode.Subsections);

                // Remove all subsections from the node
                wheelNode.Subsections.Clear();

                // Add (invoke) the invoked nodes into the subsection we just emptied.
                // Use clone to separate from the list reference
                wheelNode.Subsections.AddRange(wheel.Wheels[wheelNode.Invoke].Select(WheelNode.CloneNode));

                wheelNode.Subsections.ForEach(subsection => 
                {
                    // Create a list for the subsection's subsection to add to.
                    subsection.Subsections ??= new List<WheelNode>();

                    // Add into each of those just added invoked nodes the subsections we saved earlier
                    // Use clone to separate from the list reference
                    subsection.Subsections.AddRange(savedSubsecs.Select(WheelNode.CloneNode));
                });

                // Gets rid of the invoke statement as we just invoked.
                wheelNode.Invoke = null;
            }

            // If there is only "invoke" and no subsec just invoke the sections into subsection.
            if (wheelNode.Invoke != null && wheelNode.Subsections == null)
            {
                // Add a list to the wheel node as it will be null, we have to add ?? to say if it isn't null don't do it
                wheelNode.Subsections ??= new List<WheelNode>();

                // Add (invoke) the nodes into subsection
                // Use clone to separate from the list reference
                wheelNode.Subsections.AddRange(wheel.Wheels[wheelNode.Invoke].Select(WheelNode.CloneNode));

                // Gets rid of the invoke statement as we just invoked.
                wheelNode.Invoke = null;
            }

            // If the wheel node only has subsections than just go deeper in the tree of nodes
            if (wheelNode.Subsections != null)
            {
                // Get all subsections and recursively run this method.
                wheelNode.Subsections.ForEach(wn => CycleWheelNode(wn, wheel, depth + 1));
                return wheelNode;
            }

            // Final return when reaching the end of a branch
            return wheelNode;
        }
    }
}
