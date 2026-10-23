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
        public static WheelDefinition? CurrentWheel;

        public static bool Load(string fileName)
        {
            string path = Path.Combine(Helpers.Wheels, fileName);

            WheelDefinition? tempWheel = Helpers.LoadWheelFile(path);

            // if the import had a problem it'll return null up to here 
            if (tempWheel == null) return false;

            // Otherwise the import worked and we change the current wheel
            CurrentWheel = tempWheel;

            return true;
        }

        public static void ClearWheel()
        {
            CurrentWheel = null;
        }
    }
}
