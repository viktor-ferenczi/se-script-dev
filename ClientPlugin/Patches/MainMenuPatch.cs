using System;
using System.Linq;
using System.Text;
using ClientPlugin.GUI;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using VRage;
using VRage.Game;
using VRageMath;

namespace ClientPlugin.Patches
{
    // ReSharper disable once UnusedType.Global
    [HarmonyPatch(typeof(MyGuiScreenMainMenu))]
    public static class MainMenuPatch
    {
        // Wait until the menu and other plugins' CreateInGameMenu postfixes have added their buttons.
        // ReSharper disable once UnusedMember.Local
        [HarmonyPostfix]
        [HarmonyPatch("RecreateControls")]
        private static void RecreateControlsPostfix(MyGuiScreenMainMenu __instance)
        {
            var buttons = __instance.Controls.OfType<MyGuiControlButton>().ToArray();
            var loadGameText = MyTexts.GetString(MyCommonTexts.ScreenMenuButtonLoadGame);
            var neighbor =
                buttons.FirstOrDefault(button =>
                    button.Name == "LoadWorld" || button.Text == loadGameText
                ) ?? buttons.FirstOrDefault(button => button.Name == "Options");
            if (neighbor == null)
                return;

            var gap = Math.Abs(MyGuiConstants.MENU_BUTTONS_POSITION_DELTA.Y) - neighbor.Size.Y;
            var publish = new MyGuiControlButton(
                neighbor.Position,
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
            };
            publish.Position += new Vector2((neighbor.Size.X + publish.Size.X) / 2f + gap, 0f);
            __instance.Controls.Add(publish);
        }
    }
}
