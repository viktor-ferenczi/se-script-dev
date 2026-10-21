using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sandbox.Engine.Networking;
using Sandbox.Game.GUI;
using VRage.FileSystem;
using VRage.Game;
using VRage.GameServices;

namespace ClientPlugin.Publish
{
    public enum LocalItemKind
    {
        Script,
        Mod,
    }

    // A local folder which can be published to the Workshop
    public class LocalItem
    {
        private const string ModInfoFileName = "modinfo.sbmi";

        public readonly LocalItemKind Kind;

        // Path relative to the root folder of its kind, separated by forward slashes
        public readonly string Name;

        public readonly string Path;
        public readonly WorkshopId[] WorkshopIds;

        // UTC
        public readonly DateTime LastChange;

        // UTC, from the Workshop once known, before that from the local publish record
        public DateTime? LastPublish;

        // Details of the first Workshop item, once they arrived
        public MyWorkshopItem WorkshopItem;

        public bool IsPublished => WorkshopIds.Length != 0;
        public string Title => System.IO.Path.GetFileName(Path);

        private LocalItem(LocalItemKind kind, string root, string path)
        {
            Kind = kind;
            Name = path.Substring(root.Length).TrimStart('/', '\\').Replace('\\', '/');
            Path = path;
            WorkshopIds = MyWorkshop.GetWorkshopIdFromMod(path) ?? Array.Empty<WorkshopId>();
            LastChange = GetLastChange(path);

            var modInfoPath = System.IO.Path.Combine(path, ModInfoFileName);
            if (IsPublished && File.Exists(modInfoPath))
                LastPublish = File.GetLastWriteTimeUtc(modInfoPath);
        }

        public static List<LocalItem> ListAll()
        {
            var items = new List<LocalItem>();
            ListScripts(
                MyBlueprintUtils.SCRIPT_FOLDER_LOCAL,
                MyBlueprintUtils.SCRIPT_FOLDER_LOCAL,
                items
            );
            ListMods(items);
            return items;
        }

        // Same as the vanilla Mods screen: every non-empty folder in the Mods folder
        private static void ListMods(List<LocalItem> items)
        {
            var root = MyFileSystem.ModsPath;
            if (!Directory.Exists(root))
                return;

            foreach (
                var path in Directory
                    .EnumerateDirectories(root)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            )
            {
                if (Directory.EnumerateFileSystemEntries(path).Any())
                    items.Add(new LocalItem(LocalItemKind.Mod, root, path));
            }
        }

        // Scripts can be organized into subfolders, but a script's own folder is not searched further
        private static void ListScripts(string root, string folder, List<LocalItem> items)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (
                var path in Directory
                    .EnumerateDirectories(folder)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            )
            {
                if (
                    File.Exists(
                        System.IO.Path.Combine(
                            path,
                            MyBlueprintUtils.DEFAULT_SCRIPT_NAME + MyBlueprintUtils.SCRIPT_EXTENSION
                        )
                    )
                )
                    items.Add(new LocalItem(LocalItemKind.Script, root, path));
                else
                    ListScripts(root, path, items);
            }
        }

        // The publish writes modinfo.sbmi and metadata.mod, they must not count as local changes,
        // neither does the folder's own time, which changes when they are created
        private static DateTime GetLastChange(string path)
        {
            var lastChange = DateTime.MinValue;
            foreach (
                var filePath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            )
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                if (
                    fileName.Equals(ModInfoFileName, StringComparison.OrdinalIgnoreCase)
                    || fileName.Equals("metadata.mod", StringComparison.OrdinalIgnoreCase)
                )
                    continue;

                var lastWrite = File.GetLastWriteTimeUtc(filePath);
                if (lastWrite > lastChange)
                    lastChange = lastWrite;
            }

            return lastChange;
        }
    }
}
