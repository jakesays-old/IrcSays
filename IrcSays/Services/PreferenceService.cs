using System;
using System.IO;
using IrcSays.Preferences;
using IrcSays.Preferences.Models;
using IrcSays.Utility;

namespace IrcSays.Services
{
	public class PreferenceService : IPreferenceService
	{
		private FileSystemPropertyService _properties;

		private BufferPreferences _buffer;
		private ColorsPreferences _color;
		private NetworkPreferences _network;
		private ServerPreferences _server;
		private SoundsPreferences _sound;
		private UserPreferences _user;
		private WindowsPreferences _window;

		public void Initialize()
		{
			FileSystemPropertyService.LockKey = "IrcSays-5C63666E-CDB6-41A0-898C-3CD18EFDFC13";

			var propsBasePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"IrcSays");

			var exists = true;
			var configPath = new DirectoryName(Path.Combine(propsBasePath, "Config"));
			if (!Directory.Exists(configPath))
			{
				exists = false;
				Directory.CreateDirectory(configPath);
			}
			var dataPath = new DirectoryName(Path.Combine(propsBasePath, "Data"));
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}

			_properties = new FileSystemPropertyService(configPath, dataPath, "IrcSaysProperties");

			if (_properties.IsNew)
			{
				_buffer = _properties.Set("BufferProperties", new BufferPreferences());
				_color = _properties.Set("ColorPreferences", new ColorsPreferences());
				_network = _properties.Set("NetworkPreferences", new NetworkPreferences());
				_server = _properties.Set("ServerPreferences", new ServerPreferences());
				_sound = _properties.Set("SoundPreferences", new SoundsPreferences());
				_user = _properties.Set("UserPreferences", new UserPreferences());
				_window = _properties.Set("WindowPreferences", new WindowsPreferences());

				_properties.Save();
			}
			else
			{
				_buffer = _properties.Get("BufferProperties", () => new BufferPreferences());
				_color = _properties.Get("ColorPreferences", () => new ColorsPreferences());
				_network = _properties.Get("NetworkPreferences", () => new NetworkPreferences());
				_server = _properties.Get("ServerPreferences", () => new ServerPreferences());
				_sound = _properties.Get("SoundPreferences", () => new SoundsPreferences());
				_user = _properties.Get("UserPreferences", () => new UserPreferences());
				_window = _properties.Get("WindowPreferences", () => new WindowsPreferences());

			}
		}

		public BufferPreferences Buffer
		{
			get { return _buffer; }
		}

		public ColorsPreferences Color
		{
			get { return _color; }
		}

		public NetworkPreferences Network
		{
			get { return _network; }
		}

		public ServerPreferences Server
		{
			get { return _server; }
		}

		public SoundsPreferences Sound
		{
			get { return _sound; }
		}

		public UserPreferences User
		{
			get { return _user; }
		}

		public WindowsPreferences Window
		{
			get { return _window; }
		}
	}
}