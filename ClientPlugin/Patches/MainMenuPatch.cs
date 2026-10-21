using System.Linq;
using System.Text;
using ClientPlugin.GUI;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace ClientPlugin.Patches
{
    // Adds a Publish button right above the last button of the main menu and the in-game menu
    // ReSharper disable once UnusedType.Global
    [HarmonyPatch(typeof(MyGuiScreenMainMenu))]
    public static class MainMenuPatch
    {
        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("CreateMainMenu")]
        private static void CreateMainMenuPostfix(
            MyGuiScreenMainMenu __instance,
            ref Vector2 lastButtonPosition
        )
        {
            AddButton(__instance, lastButtonPosition);
        }

        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("CreateInGameMenu")]
        private static void CreateInGameMenuPostfix(
            MyGuiScreenMainMenu __instance,
            ref Vector2 lastButtonPosition
        )
        {
            AddButton(__instance, lastButtonPosition);
        }

        // The buttons above the last one move up to make room, because Pulsar already moves
        // the last button down for its own one and there is no space left below on 16:9 screens
        private static void AddButton(MyGuiScreenMainMenu screen, Vector2 last)
        {
            foreach (var button in screen.Controls.OfType<MyGuiControlButton>())
            {
                if (button.Position.X == last.X && button.Position.Y < last.Y)
                    button.Position -= MyGuiConstants.MENU_BUTTONS_POSITION_DELTA;
            }

            var position = last - MyGuiConstants.MENU_BUTTONS_POSITION_DELTA;
            screen.Controls.Add(
                new MyGuiControlButton(
                    position,
                    MyGuiControlButtonStyleEnum.StripeLeft,
                    originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM,
                    text: new StringBuilder("Publish"),
                    toolTip: "Publish local scripts to the Workshop (ScriptDev)",
                    onButtonClick: _ => MyGuiSandbox.AddScreen(new PublishDialog())
                )
                {
                    BorderEnabled = false,
                    BorderSize = 0,
                    BorderHighlightEnabled = false,
                    BorderColor = Vector4.Zero,
                }
            );
        }
    }
}
