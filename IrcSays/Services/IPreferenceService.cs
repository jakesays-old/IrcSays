using IrcSays.Preferences.Models;

namespace IrcSays.Services
{
	public interface IPreferenceService
	{
		BufferPreferences Buffer { get; }

		ColorsPreferences Color { get; }

		NetworkPreferences Network { get; }

		ServerPreferences Server { get; }

		SoundsPreferences Sound { get; }

		UserPreferences User { get; }

		WindowsPreferences Window { get; }

	}
}