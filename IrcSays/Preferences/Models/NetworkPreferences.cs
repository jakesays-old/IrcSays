namespace IrcSays.Preferences.Models
{
	public class NetworkPreferences : PreferenceBase
	{
		private bool _useSocks5Proxy;
		private string _proxyHostname;
		private int _proxyPort;
		private string _proxyUsername;
		private string _proxyPassword;

		public NetworkPreferences()
		{
			_useSocks5Proxy = false;
			_proxyHostname = "127.0.0.1";
			_proxyPort = 1080;
			_proxyUsername = "";
			_proxyPassword = "";

		}
		
		public bool UseSocks5Proxy
		{
			get { return _useSocks5Proxy; }
			set
			{
				_useSocks5Proxy = value;
				OnPropertyChanged();
			}
		}

		public string ProxyHostname
		{
			get { return _proxyHostname; }
			set
			{
				_proxyHostname = value;
				OnPropertyChanged();
			}
		}

		public int ProxyPort
		{
			get { return _proxyPort; }
			set
			{
				_proxyPort = value;
				OnPropertyChanged();
			}
		}

		public string ProxyUsername
		{
			get { return _proxyUsername; }
			set
			{
				_proxyUsername = value;
				OnPropertyChanged();
			}
		}

		public string ProxyPassword
		{
			get { return _proxyPassword; }
			set
			{
				_proxyPassword = value;
				OnPropertyChanged();
			}
		}

	}
}
