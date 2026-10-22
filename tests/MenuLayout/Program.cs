using System.Reflection;
using System.Text;
using ClientPlugin.Patches;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using VRage;
using VRage.Game;
using VRageMath;

var failures = 0;
foreach (var multiplayer in new[] { false, true })
foreach (var inGameExit in new[] { false, true })
foreach (var worldLoading in new[] { false, true })
{
    var menu = new MyGuiScreenMainMenu();
    // Translate the shared X origin; Y includes the 54px fullscreen margin and 0.043 offset.
    var origin = new Vector2(0.1f, 1f - 54f / 1080f + 0.043f);
    var rows = multiplayer
        ? new[] { "SaveWorld", "SaveAsWorld", "ShowPlayers", "Options", "ExitToMainMenu" }
        : new[] { "SaveWorld", "SaveAsWorld", "Options", "ExitToMainMenu" };
    for (var i = 0; i < rows.Length; i++)
        menu.Controls.Add(
            new MyGuiControlButton(
                origin - (rows.Length - i) * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA,
                VRage.Game.MyGuiControlButtonStyleEnum.StripeLeft
            )
            {
                Name = rows[i],
            }
        );
    // InGameWorldLoading b31a23d: Default buttons are center-bottom aligned.
    for (var i = 0; worldLoading && i < 3; i++)
        menu.Controls.Add(
            new MyGuiControlButton(
                new Vector2(0.3f, 0.873f - (inGameExit ? 0.06f : 0f) + i * 0.06f),
                text: new StringBuilder(
                    i == 1
                        ? MyTexts.GetString(MyCommonTexts.ScreenMenuButtonLoadGame)
                        : $"WorldLoading{i}"
                )
            )
            {
                Name = $"WorldLoading{i}",
            }
        );
    if (inGameExit)
        menu.Controls.Add(
            new MyGuiControlButton(new Vector2(0.3f, 0.993f)) { Name = "InGameExit" }
        );
    var existing = menu.Controls.Select(button => (button, button.Position, button.Size)).ToArray();
    Patch(menu, "RecreateControlsPostfix");
    var publish = menu.Controls.Single(button => button.Name == "ScriptDevPublish");
    var collisions = menu
        .Controls.Where(button => button != publish && Overlaps(publish, button))
        .Select(button => button.Name)
        .ToArray();
    var anchor = menu.Controls.Single(button =>
        button.Name == (worldLoading ? "WorldLoading1" : "Options")
    );
    var gap = publish.Position.X - publish.Size.X / 2 - anchor.Position.X - anchor.Size.X / 2;
    var anchored =
        publish.Position.Y == anchor.Position.Y
        && Math.Abs(gap - (0.06f - anchor.Size.Y)) < 0.0001f;
    var allUnchanged = existing.All(item =>
        item.button.Position.Equals(item.Position) && item.button.Size.Equals(item.Size)
    );
    var ok = collisions.Length == 0 && allUnchanged && anchored;
    Console.WriteLine(
        $"{(ok ? "PASS" : "FAIL")} multiplayer={multiplayer}, InGameExit={inGameExit}, WorldLoading={worldLoading}, anchored={anchored}: collisions=[{string.Join(", ", collisions)}], existing buttons unchanged={allUnchanged}"
    );
    if (!ok)
        failures++;
}
var main = new MyGuiScreenMainMenu();
main.Controls.Add(
    new MyGuiControlButton(
        new Vector2(0.1f, 0.7f),
        VRage.Game.MyGuiControlButtonStyleEnum.StripeLeft
    )
    {
        Name = "LoadWorld",
    }
);
var mainLoadPosition = main.Controls[0].Position;
var mainLoadSize = main.Controls[0].Size;
Patch(main, "RecreateControlsPostfix");
var load = main.Controls[0];
var mainPublish = main.Controls[1];
var mainOk =
    load.Position.Equals(mainLoadPosition)
    && load.Size.Equals(mainLoadSize)
    && mainPublish.Position.Y == load.Position.Y
    && mainPublish.Position.X > load.Position.X
    && !Overlaps(mainPublish, load);
Console.WriteLine($"{(mainOk ? "PASS" : "FAIL")} main menu: Publish beside Load Game");
return failures + (mainOk ? 0 : 1);

static void Patch(MyGuiScreenMainMenu menu, string method) =>
    typeof(MainMenuPatch)
        .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static)!
        .Invoke(null, new object[] { menu });
static bool Overlaps(MyGuiControlButton a, MyGuiControlButton b) =>
    Math.Abs(a.Position.X - b.Position.X) < (a.Size.X + b.Size.X) / 2 - 0.0001f
    && a.Position.Y - a.Size.Y < b.Position.Y - 0.0001f
    && b.Position.Y - b.Size.Y < a.Position.Y - 0.0001f;
