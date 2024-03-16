using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Shared.Logging;
using VRage.FileSystem;
using VRage.Game.Entity;

namespace ClientPlugin.Logic
{
    public static class Tracker
    {
        private static IPluginLogger Log => Plugin.Instance.Log;
        private static string IngameScriptsDir => Path.Combine(MyFileSystem.UserDataPath, "IngameScripts", "local");

        #region "Tracking PBs"

        private static readonly Dictionary<string, Script> Scripts = new Dictionary<string, Script>(64);
        private static readonly List<string> ScriptsToRemove = new List<string>(64);

        private static readonly Dictionary<IMyProgrammableBlock, DateTime> Updates = new Dictionary<IMyProgrammableBlock, DateTime>(64);
        private static readonly Dictionary<IMyProgrammableBlock, DateTime> UpdatesToModify = new Dictionary<IMyProgrammableBlock, DateTime>(64);

        public static void Configure()
        {
            MySession.OnLoading += Clear;
            MySession.OnUnloading += Clear;

            MyEntities.OnEntityAdd += OnEntityAdd;
            MyEntities.OnEntityRemove += OnEntityRemove;
        }

        private static void Clear()
        {
            Scripts.Clear();
            Updates.Clear();
        }

        private static void OnEntityAdd(MyEntity entity)
        {
            if (MyAPIGateway.Utilities.IsDedicated || MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
            {
                return;
            }

            if (entity is MyCubeGrid grid && grid.Physics != null)
            {
                foreach (var pb in grid.GetFatBlocks<MyProgrammableBlock>())
                {
                    Register(pb);
                }

                grid.OnFatBlockAdded += OnFatBlockAdded;
                grid.OnFatBlockRemoved += OnFatBlockRemoved;
            }
        }

        private static void OnEntityRemove(MyEntity entity)
        {
            if (MyAPIGateway.Utilities.IsDedicated || MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
            {
                return;
            }

            if (entity is MyCubeGrid grid && grid.Physics != null)
            {
                grid.OnFatBlockAdded -= OnFatBlockAdded;
                grid.OnFatBlockRemoved -= OnFatBlockRemoved;

                foreach (var pb in grid.GetFatBlocks<MyProgrammableBlock>())
                {
                    Unregister(pb);
                }
            }
        }

        private static void OnFatBlockAdded(MyCubeBlock block)
        {
            if (block.SlimBlock?.FatBlock is IMyProgrammableBlock pb)
            {
                Register(pb);
            }
        }

        private static void OnFatBlockRemoved(MyCubeBlock block)
        {
            if (block.SlimBlock?.FatBlock is IMyProgrammableBlock pb)
            {
                Unregister(pb);
            }
        }

        private static void Register(IMyProgrammableBlock pb)
        {

            Updates[pb] = default;
            Log.Debug($"Registered PB \"{pb.CustomName}\" [{pb.EntityId}]");
        }

        private static void Unregister(IMyProgrammableBlock pb)
        {

            Updates.Remove(pb);
            Log.Debug($"Unregistered PB \"{pb.CustomName}\" [{pb.EntityId}]");
        }

        #endregion

        #region "Updating code"

        public static void Update()
        {
            if (Plugin.Instance.Tick % 60 == 0)
            {
                UpdateScripts();
                UpdatePBs();
            }
        }

        private static void UpdateScripts()
        {
            foreach (var script in Scripts.Values)
            {
                script.CheckUpdate();

                if (!script.IsValid)
                {
                    ScriptsToRemove.Add(script.Path);
                }
            }

            foreach (var path in ScriptsToRemove)
            {
                Scripts.Remove(path);
            }

            ScriptsToRemove.Clear();
        }

        private static void UpdatePBs()
        {
            foreach (var (pb, pbUpdated) in Updates)
            {
                if (!pb.Closed && pb.IsFunctional && TryGetScript(pb, out var script) && pbUpdated != script.Modified)
                {
                    if (pb.ProgramData != script.Code)
                    {
                        pb.ProgramData = script.Code;
                    }

                    Log.Info($"Updated code in PB \"{pb.CustomName}\" [{pb.EntityId}] to {script.Modified:O}");
                    MyAPIGateway.Utilities.ShowNotification($"Updated code: {pb.CustomName}");

                    UpdatesToModify[pb] = script.Modified;
                }
            }

            foreach (var (pb, modified) in UpdatesToModify)
            {
                Updates[pb] = modified;
            }

            UpdatesToModify.Clear();
        }

        private static bool TryGetScript(IMyProgrammableBlock pb, out Script script)
        {
            var path = FormatScriptPath(pb.CustomName);
            if (path == null || !File.Exists(path))
            {
                script = null;
                return false;
            }

            if (!Scripts.TryGetValue(path, out script))
            {
                script = new Script(path);
                Scripts[path] = script;
            }

            return true;
        }

        private static readonly Regex RxScriptPathInBlockName = new Regex(@"\[(.*?)\]");

        private static string FormatScriptPath(string blockName)
        {
            var match = RxScriptPathInBlockName.Match(blockName);
            if (!match.Success)
            {
                return null;
            }

            var name = match.Groups[1].Value;
            if (name.Contains(".."))
            {
                return null;
            }

            var path = Path.Combine(IngameScriptsDir, name.Replace("/", "\\"), "Script.cs");
            return path;
        }

        #endregion
    }
}