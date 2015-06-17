using System;
using System.Diagnostics;
using System.Media;
using IrcSays.Application;
using IrcSays.Utility;

namespace IrcSays.Services
{
	public class SoundService : ISoundService
	{
		private SoundPlayer _player;

		public SoundService()
		{			
		}

		public void PlaySound(string name)
		{
			if (App.Settings.Current.Sounds.IsEnabled)
			{
				var path = App.Settings.Current.Sounds.GetPathByName(name);
				if (path.NotNullOrEmpty())
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