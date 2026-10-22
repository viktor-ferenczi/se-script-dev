using System;
using System.Linq;
using System.Text;
using ClientPlugin.GUI;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using VRage.Game;
using VRageMath;

namespace ClientPlugin.Patches
{
    // Adds a Publish button beside Load Game in the main menu and above Save in the in-game menu,
    // without moving any of the vanilla buttons
    // ReSharper disable once UnusedType.Global
    [HarmonyPatch(typeof(MyGuiScreenMainMenu))]
    public static class MainMenuPatch
    {
        // Publishing used to be reached through the Load Game screen
        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("CreateMainMenu")]
        private static void CreateMainMenuPostfix(MyGuiScreenMainMenu __instance)
        {
            AddPublishButton(__instance, "LoadWorld", above: false);
        }

        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("CreateInGameMenu")]
        private static void CreateInGameMenuPostfix(MyGuiScreenMainMenu __instance)
        {
            AddPublishButton(__instance, "SaveWorld", above: true);
        }

        // The horizontal gap is the same as the vertical one between the menu buttons
        private static void AddPublishButton(
            MyGuiScreenMainMenu screen,
            string buttonName,
            bool above
        )
        {
            var neighbor = screen
                .Controls.OfType<MyGuiControlButton>()
                .FirstOrDefault(button => button.Name == buttonName);
            if (neighbor == null)
                return;

            var gap = Math.Abs(MyGuiConstants.MENU_BUTTONS_POSITION_DELTA.Y) - neighbor.Size.Y;
            // The column beside the pause menu is used by In Game World Loading.
            var offset = above
                ? -MyGuiConstants.MENU_BUTTONS_POSITION_DELTA
                : new Vector2(neighbor.Size.X + gap, 0f);
            screen.Controls.Add(
                new MyGuiControlButton(
                    neighbor.Position + offset,
                    MyGuiControlButtonStyleEnum.StripeLeft,
                    originAlign: neighbor.OriginAlign,
                    text: new StringBuilder("Publish"),
                    toolTip: "Publish local scripts and mods to the Workshop (ScriptDev)",
                    onButtonClick: _ => MyGuiSandbox.AddScreen(new PublishDialog())
                )
                {
                    Name = "ScriptDevPublish",
                    BorderEnabled = false,
                    BorderSize = 0,
                    BorderHighlightEnabled = false,
                    BorderColor = Vector4.Zero,
                }
            );
        }
    }
}
