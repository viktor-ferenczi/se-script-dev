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
    // Adds a Publish button to the right of a vanilla button of the main menu and the in-game menu,
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
            AddButtonNextTo(__instance, "LoadWorld");
        }

        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("CreateInGameMenu")]
        private static void CreateInGameMenuPostfix(MyGuiScreenMainMenu __instance)
        {
            AddButtonNextTo(__instance, "Options");
        }

        // The horizontal gap is the same as the vertical one between the menu buttons
        private static void AddButtonNextTo(MyGuiScreenMainMenu screen, string buttonName)
        {
            var neighbor = screen
                .Controls.OfType<MyGuiControlButton>()
                .FirstOrDefault(button => button.Name == buttonName);
            if (neighbor == null)
                return;

            var gap = Math.Abs(MyGuiConstants.MENU_BUTTONS_POSITION_DELTA.Y) - neighbor.Size.Y;
            screen.Controls.Add(
                new MyGuiControlButton(
                    neighbor.Position + new Vector2(neighbor.Size.X + gap, 0f),
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
