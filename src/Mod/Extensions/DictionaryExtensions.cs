// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX;

// =============================================================================
static class DictionaryExtensions
{
	public static TValue GetOrAddDefault<TKey, TValue>(
		this IDictionary<TKey, TValue> dict,
		TKey key)
		where TValue : new()
	{
		if (!dict.TryGetValue(key, out var value)) {
			value = new TValue();
			dict.Add(key, value);
		}
		return value;
	}

	public static TValue GetOrAdd<TKey, TValue>(
		this IDictionary<TKey, TValue> dict,
		TKey key,
		Func<TValue> createValue)
	{
		if (!dict.TryGetValue(key, out var value)) {
			dict.Add(key, createValue());
		}
		return value;
	}
}
