using System;
using System.Linq;
using Sandbox.Engine.Networking;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.GameServices;
using VRage.Utils;

namespace ClientPlugin.Publish
{
    // Follows the vanilla publish flows of the script browser (MyBlueprintUtils.PublishScript)
    // and the Mods screen (MyGuiScreenMods.OnPublishModClick), except that new items get the
    // given visibility instead of always being public
    public static class Publisher
    {
        public static void Publish(
            LocalItem item,
            MyPublishedFileVisibility visibility,
            Action onDone
        )
        {
            Action selectTags = () => SelectTags(item, visibility, onDone);
            switch (item.Kind)
            {
                case LocalItemKind.Script:
                    Confirm(
                        MyCommonTexts.LoadScreenButtonPublish,
                        MySpaceTexts.ProgrammableBlock_PublishScriptDialogText,
                        selectTags
                    );
                    break;

                case LocalItemKind.Mod:
                    Action checkOwner = () => CheckModOwner(item, selectTags);
                    if (item.IsPublished)
                        Confirm(
                            MyCommonTexts.MessageBoxCaptionDoYouWishToUpdateMod,
                            MyCommonTexts.MessageBoxTextDoYouWishToUpdateMod,
                            checkOwner
                        );
                    else
                        checkOwner();
                    break;
            }
        }

        private static void Confirm(MyStringId caption, MyStringId text, Action onYes)
        {
            MyGuiSandbox.AddScreen(
                MyGuiSandbox.CreateMessageBox(
                    MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.YES_NO,
                    messageCaption: MyTexts.Get(caption),
                    messageText: MyTexts.Get(text),
                    callback: result =>
                    {
                        if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                            onYes();
                    }
                )
            );
        }

        private static void CheckModOwner(LocalItem item, Action onOwner)
        {
            if (item.WorkshopItem != null && item.WorkshopItem.OwnerId != Sync.MyId)
            {
                MyGuiSandbox.AddScreen(
                    MyGuiSandbox.CreateMessageBox(
                        MyMessageBoxStyleEnum.Error,
                        MyMessageBoxButtonsType.OK,
                        MyTexts.Get(MyCommonTexts.MessageBoxTextPublishFailed_OwnerMismatchMod),
                        MyTexts.Get(MyCommonTexts.MessageBoxCaptionModPublishFailed)
                    )
                );
                return;
            }

            onOwner();
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
                    item.Kind == LocalItemKind.Script ? "ingameScript" : "mod",
                    item.Kind == LocalItemKind.Script
                        ? MyWorkshop.ScriptCategories
                        : MyWorkshop.ModCategories,
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
            Action<bool, MyGameServiceCallResult, string, MyWorkshopItem[]> onPublished = (
                success,
                result,
                resultServiceName,
                publishedFiles
            ) =>
            {
                if (publishedFiles != null && publishedFiles.Length != 0)
                    MyWorkshop.GenerateModInfo(item.Path, publishedFiles, Sync.MyId);

                MyWorkshop.ReportPublish(publishedFiles, result, resultServiceName, onDone);
            };

            if (item.Kind == LocalItemKind.Script)
                MyWorkshop.PublishIngameScriptAsync(
                    item.Path,
                    item.Title,
                    "",
                    workshopIds,
                    tags,
                    visibility,
                    onPublished
                );
            else
                MyWorkshop.PublishModAsync(
                    item.Path,
                    item.Title,
                    null,
                    workshopIds,
                    tags,
                    visibility,
                    onPublished
                );
        }
    }
}
