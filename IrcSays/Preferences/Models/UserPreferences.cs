namespace IrcSays.Preferences.Models
{
	public class UserPreferences : PreferenceBase
	{
		private string _nickname;
		private string _alternateNickname;
		private string _username;
		private string _fullName;
		private string _hostname;
		private bool _invisible;

		public UserPreferences()
		{
			_nickname = "";
			_alternateNickname = "";
			_username = "";
			_fullName = "";
			_hostname = "";
			_invisible = false;

		}
		
		public string Nickname
		{
			get { return _nickname; }
			set
			{
				_nickname = value;
				OnPropertyChanged();
			}
		}

		public string AlternateNickname
		{
			get { return _alternateNickname; }
			set
			{
				_alternateNickname = value;
				OnPropertyChanged();
			}
		}

		public string Username
		{
			get { return _username; }
			set
			{
				_username = value;
				OnPropertyChanged();
			}
		}

		public string FullName
		{
			get { return _fullName; }
			set
			{
				_fullName = value;
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

		public bool Invisible
		{
			get { return _invisible; }
			set
			{
				_invisible = value;
				OnPropertyChanged();
			}
		}

	}
}
