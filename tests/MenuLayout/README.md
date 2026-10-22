Run `dotnet run --project tests/MenuLayout` with .NET 10. No game installation or NuGet packages are needed.

The check compiles the production menu patch against a minimal GUI surface, using the game's button sizes, spacing, and pause-menu rows. It checks overlap with In Game World Loading (`b31a23d`) in single-player and multiplayer layouts, with and without In Game Exit and In Game World Loading. It checks the localized Load Game anchor, the Options fallback without In Game World Loading, and spacing with different button widths. It also checks that vanilla buttons stay put and the main-menu Publish button stays beside Load Game.

These are geometry checks; they do not verify rendering or mouse input in the game.
