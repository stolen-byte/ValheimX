// SPDX-License-Identifier: GPL-3.0-or-later
namespace StolenByte.ValheimX.Tasks;

using Microsoft.Build.Utilities;

// =============================================================================
public abstract class BaseTask : Task
{
	public sealed override bool Execute()
	{
		try {
			OnExecute();
		}
		catch (Exception ex) {
			Log.LogErrorFromException(ex);
		}
		return !Log.HasLoggedErrors;
	}

	protected abstract void OnExecute();
}
