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
		}

		public static ISoundService Sound { get; private set; }
	}
}