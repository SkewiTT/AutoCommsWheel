using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoCommsWheel
{
    public static class WheelManager
    {
        // The full wheel with all subwheels loaded after the wheel file is loaded and resolved.
        public static WheelDefinition? LoadedWheel;

        // list of nodes to show to form a subwheel based on where we are in the full wheel
        public static List<WheelNode> CurrentNodeList = new List<WheelNode>();

        // All the nodes through which we went when going through the graphical wheel
        public static List<WheelNode> NodePath = new List<WheelNode>();

        // Dictates if a wheel is open and ready to check for a selection on the wheel.
        public static bool IsWheelOpen = false;

        public static bool Load(string fileName)
        {
            string path = Path.Combine(Helpers.Wheels, fileName);

            WheelDefinition? tempWheel = Helpers.LoadWheelFile(path);

            // if the import had a problem it'll return null up to here 
            if (tempWheel == null) return false;

            // Otherwise the import worked and we change the current wheel
            LoadedWheel = tempWheel;

            return true;
        }

        public static void ClearLoadedWheel()
        {
            LoadedWheel = null;

            CurrentNodeList = new List<WheelNode>();
            NodePath.Clear();
            IsWheelOpen = false;
        }

        public static void ResetWheelState()
        {
            if (LoadedWheel == null) return;

            CurrentNodeList = LoadedWheel.Main;
            NodePath.Clear();
        }
    }
}
