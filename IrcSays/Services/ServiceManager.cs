namespace IrcSays.Services
{
	/// <summary>
	/// Manages various service singletons.
	/// 
	/// NOTE: this is a temporary class that
	/// will eventually be replaced with the IoC
	/// </summary>
	public static class ServiceManager
	{
		public static void Initialize()
		{
			Sound = new SoundService();
			var preferences = new PreferenceService();
			preferences.Initialize();
			Preferences = preferences;
		}

		public static ISoundService Sound { get; private set; }
		public static IPreferenceService Preferences { get; private set; }
	}
}