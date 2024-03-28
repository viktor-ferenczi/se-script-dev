using System;
using System.IO;
using Shared.Logging;

namespace ClientPlugin.Logic
{
    public class Script
    {
        private static IPluginLogger Log => Plugin.Instance.Log;

        public readonly string Path;
        public DateTime Modified { get; private set; }
        public string Code { get; private set; }

        public bool IsValid => Code != null;

        public Script(string path)
        {
            Path = path;
            CheckUpdate();
        }

        public void CheckUpdate()
        {
            if (!File.Exists(Path))
            {
                Modified = default;
                Code = null;
                return;
            }

            try
            {
                var current = File.GetLastWriteTimeUtc(Path);
                if (current == Modified)
                {
                    return;
                }

                Modified = current;
                Code = File.ReadAllText(Path);
                Log.Debug($"Loaded script from \"{Path}\", last modified {Modified:O}");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load script from \"{Path}\": {e}");
                Modified = default;
                Code = null;
            }
        }
    }
}