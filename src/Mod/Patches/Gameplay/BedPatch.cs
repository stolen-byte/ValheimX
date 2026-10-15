// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
[HarmonyPatch]
static class BedPatch
{
	const string SaveFileExt = ".sp";
	static HashSet<Vector3> m_spawnPoints = new();

	static readonly Func<Instruction, bool>[] m_codeMatcher = [
		x => x.Match(OpCodes.Call) || x.Match(OpCodes.Callvirt),
		x => x.MatchLdloc(0),
		x => x.MatchCallOrCallvirt<Bed>(nameof(Bed.GetSpawnPoint)),
		x => x.MatchCallOrCallvirt<Game>(nameof(Game.RemoveCustomSpawnPoint))
	];

	static readonly Func<Instruction, bool>[] m_varMatcher = [
		x => x.Match(OpCodes.Ldloc_S),
		x => x.MatchLdcR4(5),
		x => x.MatchCallOrCallvirt<Game>(nameof(Game.FindBedNearby))
	];

	[HarmonyPatch(typeof(Bed), nameof(Bed.Awake)), HarmonyPostfix]
	static void BedAwake(Bed __instance)
		=> AddSpawnPoint(__instance);

	[HarmonyPatch(typeof(Bed), nameof(Bed.RPC_SetOwner)), HarmonyPostfix]
	static void BedClaimed(Bed __instance)
		=> AddSpawnPoint(__instance);

	[HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy)), HarmonyILManipulator]
	static void WearNTearDestroy(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(BedPatch), static (ctx, cursor) => {
			if (!cursor.TryGotoNext(MoveType.Before, m_codeMatcher))
				throw new Exception("Failed patching WearNTear::Destroy - failed to find code location");

			// Replace call to Game::RemoveCustomSpawnPoint with our own handler
			cursor
				.RemoveRange(m_codeMatcher.Length)
				.Emit(OpCodes.Ldloc_0)
				.EmitCall(OnBedDestroyed);
		});
	}

	[HarmonyPatch(typeof(Game), nameof(Game.Shutdown)), HarmonyPostfix]
	static void Shutdown()
	{
		// will have already been saved, if needed.
		m_spawnPoints.Clear();
	}

	[HarmonyPatch(typeof(Game), nameof(Game.FindSpawnPoint)), HarmonyILManipulator]
	static void FindSpawnPoint(ILContext ctx)
	{
		ctx.ApplyPatch(nameof(BedPatch), static (ctx, cursor) => {
			static bool matchClearCall(Instruction inst)
				=> inst.MatchCallOrCallvirt<PlayerProfile>(nameof(PlayerProfile.ClearCustomSpawnPoint));

			if (!cursor.TryGotoNext(MoveType.Before, m_varMatcher))
				throw new Exception("Unable to find Vector3 local variable reference");

			var spRef = cursor.Next.Operand;
			cursor.Goto(0);

			// insert call to remove spawn point after `ClearCustomSpawnPoint`
			if (cursor.TryGotoNext(MoveType.After, matchClearCall)) {
				cursor
					.Emit(OpCodes.Ldloc_S, spRef)
					.EmitCall(RemoveSpawnPoint)
					.Emit(OpCodes.Pop);
			}
		});
	}

	[HarmonyPatch(typeof(ZNet), nameof(ZNet.SaveWorldThread)), HarmonyPostfix]
	static void SaveWorld()
	{
		if (ZNet.m_world is not { } world)
			return;
		if (Game.instance is null || Game.instance.GetPlayerProfile() is not { } profile)
			return;

		File.WriteWith(ModSave.GetCharacterFilePath(profile, GetSaveFileName(world)), static w => {
			w.Write(m_spawnPoints.Count);
			foreach (var sp in m_spawnPoints) {
				w.Write(sp);
			}
		});
	}

	[HarmonyPatch(typeof(ZNet), nameof(ZNet.LoadWorld)), HarmonyPostfix]
	static void LoadWorld()
	{
		if (ZNet.m_world is not { } world)
			return;
		if (Game.instance is null || Game.instance.GetPlayerProfile() is not { } profile)
			return;

		try {
			var spawnList = new HashSet<Vector3>();
			using (var reader = File.OpenBinaryRead(ModSave.GetCharacterFilePath(profile, GetSaveFileName(world)))) {
				var count = reader.ReadInt32();
				while (count > 0) {
					spawnList.Add(reader.ReadVector3());
					--count;
				}
			}
			m_spawnPoints = spawnList;
		} catch (DirectoryNotFoundException) {
			// ignore
		} catch (FileNotFoundException) {
			// ignore
		} catch (Exception ex) {
			Log.Exception($"Error loading spawn points for '{profile.m_filename}', world: '{world.m_worldName}'", ex);
		}
	}

	[HarmonyPatch(typeof(ManageSavesMenu), nameof(ManageSavesMenu.DeleteSaveFile)), HarmonyPostfix]
	static void DeleteSaveFile(SaveFile file, bool isBackup)
	{
		if (isBackup)
			return;

		var save = file.ParentSaveWithBackups;
		if (save.ParentSaveCollection.m_dataType == SaveDataType.Character) {
			var path = ModSave.GetCharacterDirectory(save.Name);
			Directory.TryDelete(path);
		} else {
			foreach (var name in ModSave.GetCharacters()) {
				var path = ModSave.GetCharacterFilePath(name, GetSaveFileName(save.Name));
				File.TryDelete(path);
			}
		}
	}

	static void AddSpawnPoint(Bed bed)
	{
		if (bed.IsMine()) {
			m_spawnPoints.Add(bed.GetSpawnPoint());
		}
	}

	static bool RemoveSpawnPoint(Vector3 point)
		=> m_spawnPoints.Remove(point);

	static void OnBedDestroyed(Bed bed)
	{
		var oldSpawn = bed.GetSpawnPoint();
		if (!RemoveSpawnPoint(oldSpawn))
			return;

		if (Game.instance is null || Game.instance.GetPlayerProfile() is not { } profile)
			return;

		if (profile.HaveCustomSpawnPoint() && profile.GetCustomSpawnPoint() == oldSpawn) {
			if (FindNearestSpawnPoint(oldSpawn) is { } newSpawn) {
				profile.SetCustomSpawnPoint(newSpawn);
				Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_spawnpointset");
			} else {
				profile.ClearCustomSpawnPoint();
			}
		}
	}

	static Vector3? FindNearestSpawnPoint(Vector3 pos)
	{
		Vector3? result = null;
		var nearest = float.PositiveInfinity;

		foreach (var sp in m_spawnPoints) {
			var dist = (sp - pos).sqrMagnitude;
			if (dist < nearest) {
				result = sp;
				nearest = dist;
			}
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static string GetSaveFileName(string worldName)
		=> string.Concat(Path.SanitizeFileName(worldName), SaveFileExt);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static string GetSaveFileName(World world)
		=> GetSaveFileName(world.m_worldName);
}
