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

        /// <summary>
        /// Checks if a json file's contents are correct
        /// </summary>
        /// <param name="path">Path to the json file</param>
        /// <returns>True if valid, false if unvalid</returns>
        public static bool IsJsonValid(string path)
        {
            try
            {
                using (JsonDocument.Parse(File.ReadAllText(path)))
                {
                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
