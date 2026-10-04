# ScriptDev Plugin for Space Engineers

This plugin automatically updates the code in programmable blocks
whenever the corresponding `Script.cs` changes. It is detected based
on the file's last modification time and polled every second.

Scripts of more than 100,000 characters can be loaded. This is useful
for offline development, but not compatible with multiplayer.

Please consider supporting my work on [Patreon](https://www.patreon.com/semods) or one time via [PayPal](https://www.paypal.com/paypalme/vferenczi/).

*Thank you and enjoy!*

## Prerequisites

- [Space Engineers](https://store.steampowered.com/app/244850/Space_Engineers/)
- [Plugin Loader](https://github.com/sepluginloader)

## Usage

Enable the **ScriptDev** plugin in Plugin Loader, apply the change and restart the game.

The name of the PB must include the script's name in square brackets.
For example: `Programmable Block [Name Of My Script]`

Script subdirectories also work, separate them by forward slashes.
For example: `Programmable Block [Script Subdir/Name Of My Script]`

Scripts are under this folder: `%AppData%\SpaceEngineers\IngameScripts\local`

Use the [In-game Script Merge Tool](https://github.com/viktor-ferenczi/se-script-merge)
for convenient in-game script development in a proper IDE. It allows for
merging your script from multiple files, sharing code between scripts,
introducing unit tests not copied into the script and minifying your 
script for release.

## Remarks

- This plugin is designed solely for local script development.
- It works only in offline and locally hosted games.
- It is not scalable to a large number of PBs.

## Development

The build finds the game through Steam. To override its location or set the deploy folders
below, use `Directory.Build.props.user`, which is not committed. `setup.py` creates it.

Load the working copy through a Pulsar development folder: start Pulsar with `-sources`,
then add this repository with the Sources button.

Builds deploy nothing by default. To deploy, set the target folder in
`Directory.Build.props.user` or pass it on the command line:

- `Pulsar`, for example `dotnet build -p:Pulsar=$HOME/.config/Pulsar`: the client plugin goes to
  `<Pulsar>/Legacy/Local/ScriptDev/` (net48) or `<Pulsar>/Interim/Local/ScriptDev/` (net10.0,
  falls back to `Legacy` if there is no `Interim` folder)
- `Torch`: the Torch plugin goes to `<Torch>/Plugins/ScriptDev/`, the Dedicated Server plugin
  to `<Torch>/DedicatedServer64/Plugins/`

The Torch and Dedicated Server projects are Windows only. They build against the `Torch`
folder link created by `Edit-and-run-before-opening-solution.bat`.

## Want to know more?

- [SE Mods Discord](https://discord.gg/PYPFPGf3Ca) FAQ, Troubleshooting, Support, Bug Reports, Discussion
- [Plugin Loader Discord](https://discord.gg/6ETGRU3CzR) Everything about plugins
