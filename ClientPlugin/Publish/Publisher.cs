using System;
using System.Linq;
using Sandbox.Engine.Networking;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.GameServices;

namespace ClientPlugin.Publish
{
    // Follows the vanilla publish flow of the script browser (MyBlueprintUtils.PublishScript),
    // except that new items get the given visibility instead of always being public
    public static class Publisher
    {
        public static void Publish(
            LocalItem item,
            MyPublishedFileVisibility visibility,
            Action onDone
        )
        {
            MyGuiSandbox.AddScreen(
                MyGuiSandbox.CreateMessageBox(
                    MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.YES_NO,
                    messageCaption: MyTexts.Get(MyCommonTexts.LoadScreenButtonPublish),
                    messageText: MyTexts.Get(
                        MySpaceTexts.ProgrammableBlock_PublishScriptDialogText
                    ),
                    callback: result =>
                    {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                            SelectTags(item, visibility, onDone);
                    }
                )
            );
        }

        private static void SelectTags(
            LocalItem item,
            MyPublishedFileVisibility visibility,
            Action onDone
        )
        {
            var tags = item.WorkshopItem?.Tags.ToArray();
            MyGuiSandbox.AddScreen(
                new MyGuiScreenWorkshopTags(
                    "ingameScript",
                    MyWorkshop.ScriptCategories,
                    tags,
                    (result, outTags, serviceNames) =>
                    {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                            Upload(item, visibility, outTags, serviceNames, onDone);
                    }
                )
            );
        }

        private static void Upload(
            LocalItem item,
            MyPublishedFileVisibility visibility,
            string[] tags,
            string[] serviceNames,
            Action onDone
        )
        {
            var workshopIds = MyWorkshop.FilterWorkshopIds(item.WorkshopIds, serviceNames);
            MyWorkshop.PublishIngameScriptAsync(
                item.Path,
                item.Title,
                "",
                workshopIds,
                tags,
                visibility,
                (success, result, resultServiceName, publishedFiles) =>
                {
                    if (publishedFiles != null && publishedFiles.Length != 0)
                        MyWorkshop.GenerateModInfo(item.Path, publishedFiles, Sync.MyId);

                    MyWorkshop.ReportPublish(publishedFiles, result, resultServiceName, onDone);
                }
            );
        }
    }
}
