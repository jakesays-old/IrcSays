namespace IrcSays.Preferences.Models
{
	public class ServerPreferences : PreferenceBase
	{
		private string _name;
		private string _hostname;
		private int _port;
		private string _password;
		private bool _isSecure;
		private bool _connectOnStartup;
		private bool _autoReconnect;
		private string _onConnect;

		public ServerPreferences()
		{
			_name = null;
			_hostname = null;
			_port = 6667;
			_password = null;
			_isSecure = false;
			_connectOnStartup = false;
			_autoReconnect = true;
			_onConnect = "";

		}
		
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public string Hostname
		{
			get { return _hostname; }
			set
			{
				_hostname = value;
				OnPropertyChanged();
			}
		}

		public int Port
		{
			get { return _port; }
			set
			{
				ValidateRange(value, 0, 65535);
				_port = value;
				OnPropertyChanged();
			}
		}

		public string Password
		{
			get { return _password; }
			set
			{
				_password = value;
				OnPropertyChanged();
			}
		}

		public bool IsSecure
		{
			get { return _isSecure; }
			set
			{
				_isSecure = value;
				OnPropertyChanged();
			}
		}

		public bool ConnectOnStartup
		{
			get { return _connectOnStartup; }
			set
			{
				_connectOnStartup = value;
				OnPropertyChanged();
			}
		}

		public bool AutoReconnect
		{
			get { return _autoReconnect; }
			set
			{
				_autoReconnect = value;
				OnPropertyChanged();
			}
		}

		public string OnConnect
		{
			get { return _onConnect; }
			set
			{
				_onConnect = value;
				OnPropertyChanged();
			}
		}

	}
}
