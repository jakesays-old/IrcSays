using System;
using System.Diagnostics;
using System.Media;

namespace IrcSays.Application
{
	public partial class App
	{
		private static SoundPlayer _player;

		public static void DoEvent(string eventName)
		{
			if (Settings.Current.Sounds.IsEnabled)
			{
				var path = Settings.Current.Sounds.GetPathByName(eventName);
				if (!string.IsNullOrEmpty(path))
				{
					if (_player != null)
					{
						_player.Dispose();
					}
					try
					{
						_player = new SoundPlayer(path);
						_player.Play();
					}
					catch (Exception ex)
					{
						_player = null;
						Debug.WriteLine("Unable to play audio file {0}: {1}", path, ex.Message);
					}
				}
			}
		}
	}
}