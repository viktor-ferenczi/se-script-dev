# ScriptDev Plugin for Space Engineers

## Prerequisites

- [Space Engineers](https://store.steampowered.com/app/244850/Space_Engineers/)
- [Plugin Loader](https://github.com/sepluginloader)

## Features

This plugin automatically updates the code in programmable blocks
whenever the corresponding `Script.cs` changes. It is detected based
on the file's last modification time and polled every second.

Scripts of more than 100,000 characters can be loaded. This is useful
for offline development, but not compatible with multiplayer.

## Usage

Enable the **ScriptDev** plugin Plugin Loader, apply, restart the game.

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