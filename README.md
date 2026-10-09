# ValheimX

Valheim Xpanded.

ValheimX is a place for me to put all my various tweaks and QoL changes,
which (imo) make playing Valheim an even more enjoyable experience.

The only changes i consider adding, are those that both make sense (in the game world), and fit
within the game's vision. This means there will be NO massive changes like linking nearby chests to
crafting benches, and other "QoL" stuff like that (i think its lazy, and i don't believe in it).

Most changes are configurable either in-game (with [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager))
or in the config file `BepInEx/config/StolenByte.ValheimX.cfg`.

## Requirements

- [BepInEx](https://github.com/BepInEx/BepInEx)

> [!NOTE]
> I do not use the valheim-specific version of BepInEx from the thunderstore, despite contrary belief, it is not
> required at all (and is now actually a bad idea, since it explicitly sets the `IsModded` flag and disables
> achievements). But if that is what you have, this mod explicitly removes that flag from achievement checks.

## Installation

This mod does not support any mod installers/managers, and never will.

Just place `StolenByte.ValheimX.dll` in your `BepInEx/plugins` directory, and you're good to go!

## Building

Building from source requires the latest [dotnet sdk](https://dotnet.microsoft.com/en-us/download).

To configure the build, and setup paths to the game/unity assemblies:

    $ ./configure <path to valheim game directory>

If you are not on linux, or are otherwise unable to run the linux shell script, then instead use:

    $ dotnet run --project tools/Configure -c Release -- <path to valheim game directory>

You can view the list of configure script options by passing `-h` or `--help`.

Then to build:

    $ dotnet build -c Release

the built `dll` will be in the `build/bin/ValheimX/release` directory.

## Changes

For a list of features/changes/fixes, see [CHANGES](CHANGES.md).

## License

```
ValheimX is distributed completely free of charge: you can redistribute
it and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License for more details.
```

To see the full license text, see [LICENSE](LICENSE)
