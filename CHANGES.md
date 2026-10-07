# Changes

## Gameplay

- Prevent `Game.isModded` flag from disabling achievements.
- Removed 100hp cap on fall damage.

## Combat

- Removed damage penalty when attacking multiple targets.

## Exploration

- Configurable wisp light radius.

# Commands

- `printenv` - Print current biome/environment information
- `printobjects` - List names of all objects within the given radius
- `dumpobjects` - Dump all items/environments to the specified directory, each categorised in its own file

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
