# Changes

## Gameplay

- Prevent `Game.isModded` flag from disabling achievements.
- Removed 100hp cap on fall damage.
- Allow using most 1handed equipment in water, where sensible.
- Allow extinguishing/re-lighting fires. (keybind: Alt Place + Use).
- When removing/destroying a bed, the player's spawn point automatically moves to the next nearest bed. (EXPERIMENTAL)

  Note: spawn points are saved in `BepInEx/plugins/StolenByte.ValheimX/<character>/<world>.sp`.
  This feature is _highly_ experimental, as it has to hook into many parts of the game code to function, also,
  due to a limitation in the game's code, your spawn point will not be moved if a bed is destroyed while your
  character is in another "chunk" (should only really happen in multiplayer, and rarely at that).

## Combat

- Removed damage penalty when attacking multiple targets.
- Disable enemy ranged attacks when underwater.

## Exploration

- Configurable wisp light radius.
- Dynamic minimap exploration radius, based on altitude/weather/lighting/biome.

## Building/Crafting

- Leather armour actually require leather, instead of bones.
- Floor/Stair pieces count as roofs.
- Allow building some structures inside dungeons.
- Crafting station build ranges daisy-chain from each other.
- Cultivator now removes all non-plant pickables (branches/stones/dandelions etc).
- Paving with the Hoe now removes all plants/pickables (including bushes).
- Allow removing basic build pieces without a workbench nearby. (Only pieces that require a workbench).

# Commands

- `printenv` - Print current biome/environment information
- `printobjects` - List names of all objects within the given radius
- `dumpobjects` - Dump all items/environments to the specified directory, each categorised in its own file

- Replaced `help` command with a better version, which allows displaying only commands matching a given name.

- Remove `IsCheat` flag from some debug commands:

  - `dpsdebug`
  - `listkeys`
  - `printcreatures`
  - `printnetobj`
  - `printlocations`
  - `biomeinfo`

# Fixes

- Fix `Character.IsSwimming` call in `BaseAI.CanUseAttack` to use `Character.InLiquidSwimDepth` instead.

  Reasoning: `IsSwimming` is used to determine if the character is underwater, however it returns false for any mob
  that cannot swim (ie: does not have a swimming animation, and walks along the sea floor).
  This (imo) is technically incorrect, and `InLiquidSwimDepth` is more appropriate/correct.
  This change allows weapon/attack checks which use `SharedData.m_aiWhenSwiming` to work correctly.

- Fixed dupe glitch with placeable stones, when resource modifier is > 1.

  Previously: the placeable stone only costs 1 stone, but when picked up could give 2+ back, resulting in
  basically an infinite stone source, this removes that by disabling drop-scaling for the pickup.
  (Doesn't affect regular stone pickups found in the wild).
