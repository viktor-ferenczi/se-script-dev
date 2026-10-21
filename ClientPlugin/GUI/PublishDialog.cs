using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientPlugin.Publish;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Graphics.GUI;
using VRage.GameServices;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.GUI
{
    public class PublishDialog : MyGuiScreenBase
    {
        private const string Caption = "Publish to Workshop";

        public override string GetFriendlyName() => "ScriptDevPublishDialog";

        private static readonly string[] ColumnNames =
        {
            "Name",
            "Workshop ID",
            "Visibility",
            "Local change",
            "Published",
            "Status",
        };

        // Vanilla always publishes new items as public, while publishing privately first
        // allows checking the item before anybody sees it. Remembered for the session.
        private static MyPublishedFileVisibility newItemVisibility =
            MyPublishedFileVisibility.Private;

        private List<LocalItem> items = new List<LocalItem>();

        private MyGuiControlTable table;
        private MyGuiControlButton publishButton;
        private MyGuiControlButton openButton;

        public PublishDialog()
            : base(
                new Vector2(0.5f, 0.5f),
                MyGuiConstants.SCREEN_BACKGROUND_COLOR,
                new Vector2(0.9f, 0.8f),
                false,
                null,
                MySandboxGame.Config.UIBkOpacity,
                MySandboxGame.Config.UIOpacity
            )
        {
            EnabledBackgroundFade = true;
            m_closeOnEsc = true;
            m_drawEvenWithoutFocus = true;
            CanHideOthers = true;
            CanBeHidden = true;
            CloseButtonEnabled = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            AddCaption(Caption);

            table = new MyGuiControlTable
            {
                Name = "PublishTable",
                Position = new Vector2(-0.42f, -0.31f),
                Size = new Vector2(0.84f, 0.6f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                ColumnsCount = ColumnNames.Length,
                VisibleRowsCount = 15,
            };
            table.SetCustomColumnWidths(new[] { 0.31f, 0.14f, 0.11f, 0.16f, 0.16f, 0.12f });
            for (var i = 0; i < ColumnNames.Length; i++)
                table.SetColumnName(i, new StringBuilder(ColumnNames[i]));
            table.ItemSelected += (_, __) => UpdateButtons();
            Controls.Add(table);

            Controls.Add(
                new MyGuiControlLabel(
                    new Vector2(-0.42f, 0.27f),
                    text: "Visibility of new items:",
                    originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
                )
            );
            var visibilityCombobox = new MyGuiControlCombobox(
                new Vector2(-0.22f, 0.27f),
                new Vector2(0.15f, 0.04f),
                toolTip: "Updates keep the visibility the item has on the Workshop",
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER
            )
            {
                Name = "VisibilityCombobox",
            };
            foreach (
                MyPublishedFileVisibility visibility in Enum.GetValues(
                    typeof(MyPublishedFileVisibility)
                )
            )
                visibilityCombobox.AddItem((long)visibility, visibility.ToString(), sort: false);
            visibilityCombobox.SelectItemByKey((long)newItemVisibility);
            visibilityCombobox.ItemSelected += () =>
                newItemVisibility = (MyPublishedFileVisibility)visibilityCombobox.GetSelectedKey();
            Controls.Add(visibilityCombobox);

            var y = 0.34f;
            publishButton = AddButton(
                new Vector2(-0.3f, y),
                "PublishButton",
                "Publish",
                _ => PublishSelected(),
                "Publish the selected item, or update it if it is already published"
            );
            openButton = AddButton(
                new Vector2(-0.1f, y),
                "OpenButton",
                "Open in Workshop",
                _ => OpenSelected(),
                "Open the Workshop page of the selected item"
            );
            AddButton(
                new Vector2(0.1f, y),
                "RefreshButton",
                "Refresh",
                _ => Refresh(),
                "Reload the local items and their Workshop details"
            );
            AddButton(new Vector2(0.3f, y), "CloseButton", "Close", _ => CloseScreen(), null);

            Refresh();
        }

        private MyGuiControlButton AddButton(
            Vector2 position,
            string name,
            string text,
            Action<MyGuiControlButton> onClick,
            string toolTip
        )
        {
            var button = new MyGuiControlButton(
                position,
                text: new StringBuilder(text),
                onButtonClick: onClick,
                toolTip: toolTip
            )
            {
                Name = name,
            };
            Controls.Add(button);
            return button;
        }

        private LocalItem SelectedItem => table.SelectedRow?.UserData as LocalItem;

        private void UpdateButtons()
        {
            var item = SelectedItem;
            publishButton.Enabled = item != null;
            openButton.Enabled = item?.WorkshopItem != null;
        }

        private void Refresh()
        {
            items = LocalItem.ListAll();
            FillTable();
            FetchWorkshopItems();
        }

        private void FillTable()
        {
            var selectedName = SelectedItem?.Name;

            table.Clear();
            foreach (var item in items)
            {
                var row = new MyGuiControlTable.Row(item);
                AddCell(row, item.Name, item.Path);
                AddCell(row, string.Join(", ", item.WorkshopIds.Select(id => id.Id)));
                AddCell(row, item.WorkshopItem?.Visibility.ToString() ?? "");
                AddCell(row, FormatTime(item.LastChange));
                AddCell(row, FormatTime(item.LastPublish));
                AddCell(row, FormatStatus(item));
                table.Add(row);

                if (item.Name == selectedName)
                    table.SelectedRow = row;
            }

            UpdateButtons();
        }

        private static void AddCell(MyGuiControlTable.Row row, string text, string toolTip = null)
        {
            row.AddCell(
                new MyGuiControlTable.Cell(text, toolTip: toolTip) { IsAutoScaleEnabled = true }
            );
        }

        private static string FormatTime(DateTime? utc)
        {
            return utc.HasValue && utc.Value != DateTime.MinValue
                ? utc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : "";
        }

        private static string FormatStatus(LocalItem item)
        {
            if (!item.IsPublished)
                return "Not published";

            if (!item.LastPublish.HasValue)
                return "Unknown";

            return item.LastChange > item.LastPublish.Value ? "Changed" : "Up to date";
        }

        // The publish times recorded locally only exist for items published from this machine,
        // while the Workshop knows them all, along with the visibility and the tags
        private void FetchWorkshopItems()
        {
            var fetchedFor = items;
            var workshopIds = fetchedFor.SelectMany(item => item.WorkshopIds).ToList();
            if (workshopIds.Count == 0)
                return;

            MyWorkshop.GetItemsAsync(
                workshopIds,
                (success, workshopItems) =>
                {
                    if (!success)
                        return;

                    MySandboxGame.Static.Invoke(
                        () => OnWorkshopItems(fetchedFor, workshopItems),
                        "ScriptDev"
                    );
                }
            );
        }

        private void OnWorkshopItems(List<LocalItem> fetchedFor, List<MyWorkshopItem> workshopItems)
        {
            if (fetchedFor != items)
                return;

            foreach (var item in items)
            {
                item.WorkshopItem = item
                    .WorkshopIds.Select(id =>
                        workshopItems.FirstOrDefault(wi =>
                            wi.Id == id.Id && wi.ServiceName == id.ServiceName
                        )
                    )
                    .FirstOrDefault(wi => wi != null);

                if (item.WorkshopItem != null)
                    item.LastPublish = item.WorkshopItem.TimeUpdated;
            }

            FillTable();
        }

        private void PublishSelected()
        {
            var item = SelectedItem;
            if (item != null)
                Publisher.Publish(item, newItemVisibility, Refresh);
        }

        private void OpenSelected()
        {
            var workshopItem = SelectedItem?.WorkshopItem;
            if (workshopItem != null)
                MyGuiSandbox.OpenUrlWithFallback(
                    workshopItem.GetItemUrl(),
                    workshopItem.ServiceName + " Workshop"
                );
        }
    }
}
