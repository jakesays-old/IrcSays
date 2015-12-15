using System.Collections.Generic;
using IrcSays.Preferences.Models;

namespace IrcSays.Services
{
	public interface IPreferenceService
	{
		BufferPreferences Buffer { get; }

		ColorsPreferences Color { get; }

		NetworkPreferences Network { get; }

		ServerPreferences Servers { get; }

		SoundsPreferences Sound { get; }

		UserPreferences User { get; }

		WindowsPreferences Window { get; }

		FormattingPreferences Formatting { get; }

	}
}