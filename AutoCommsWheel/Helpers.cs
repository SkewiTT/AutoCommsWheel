using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using static AutoCommsWheel.ConfigWindow;

namespace AutoCommsWheel
{
    public static class Helpers
    {
        private const string AppFolderName = "AutoCommsWheel";
        public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);
        public static string Wheels => Path.Combine(Root, "Wheels");

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
                WheelDefinition definition = JsonSerializer.Deserialize<WheelDefinition>(File.ReadAllText(path), _jsonOptions);

                // Valid Json that isn't a usable wheel (like {}) counts as invalid
                if (definition?.Main == null || definition.Main.Count == 0) return null;

                return definition;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
