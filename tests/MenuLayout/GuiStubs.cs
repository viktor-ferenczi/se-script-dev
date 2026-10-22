// Minimal GUI surface for running the production patch without starting the game.
// Dimensions come from the game's Default/StripeLeft styles over GUI_OPTIMAL_SIZE.
using System.Text;

namespace VRageMath
{
    public record struct Vector2(float X, float Y)
    {
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator *(int n, Vector2 v) => new(n * v.X, n * v.Y);
    }

    public struct Vector4
    {
        public static Vector4 Zero => default;
    }
}

namespace VRage
{
    public static class MyTexts
    {
        public static string GetString(string id) => "Charger une partie";
    }
}

namespace VRage.Game
{
    public static class MyCommonTexts
    {
        public static string ScreenMenuButtonLoadGame = "LoadGame";
    }

    public enum MyGuiControlButtonStyleEnum
    {
        Default,
        StripeLeft,
    }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type) { }

        public HarmonyPatch(string method) { }
    }

    public class HarmonyPostfix : Attribute { }
}

namespace ClientPlugin.GUI
{
    public class PublishDialog { }
}

namespace SpaceEngineers.Game.GUI
{
    public class MyGuiScreenMainMenu
    {
        public List<Sandbox.Graphics.GUI.MyGuiControlButton> Controls { get; } = new();
    }
}

namespace Sandbox.Graphics.GUI
{
    using VRage.Game;
    using VRageMath;

    public static class MyGuiConstants
    {
        public static Vector2 MENU_BUTTONS_POSITION_DELTA = new(0, 0.06f);
    }

    public static class MyGuiSandbox
    {
        public static void AddScreen(object screen) { }
    }

    public class MyGuiControlButton
    {
        public Vector2 Position;
        public Vector2 Size;
        public string Name = "";
        public string Text = "";
        public int OriginAlign;
        public bool BorderEnabled,
            BorderHighlightEnabled;
        public int BorderSize;
        public Vector4 BorderColor;

        public MyGuiControlButton(
            Vector2 position,
            MyGuiControlButtonStyleEnum style = default,
            int originAlign = 0,
            StringBuilder text = null,
            string toolTip = null,
            Action<MyGuiControlButton> onButtonClick = null
        )
        {
            Position = position;
            Text = text?.ToString();
            Size = new Vector2(
                (style == MyGuiControlButtonStyleEnum.StripeLeft ? 286f : 281f) / 1600,
                62f / 1200
            );
        }
    }
}
