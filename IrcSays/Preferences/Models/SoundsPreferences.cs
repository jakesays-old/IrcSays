namespace IrcSays.Preferences.Models
{
	public class SoundsPreferences : PreferenceBase
	{
		private bool _isEnabled;
		private string _connect;
		private string _disconnect;
		private string _privateMessage;
		private string _dccRequest;
		private string _dccComplete;
		private string _dccError;
		private string _notice;
		private string _activeAlert;
		private string _inactiveAlert;
		private string _beep;

		public SoundsPreferences()
		{
			_isEnabled = true;
			_connect = "";
			_disconnect = @"%SYSTEMROOT%\Media\Windows Ding.wav";
			_privateMessage = @"%SYSTEMROOT%\Media\Windows Ding.wav";
			_dccRequest = @"%SYSTEMROOT%\Media\Windows Ding.wav";
			_dccComplete = "";
			_dccError = @"%SYSTEMROOT%\Media\Windows Ding.wav";
			_notice = "";
			_activeAlert = "";
			_inactiveAlert = @"%SYSTEMROOT%\Media\Windows Ding.wav";
			_beep = @"%SYSTEMROOT%\Media\Windows Ding.wav";

		}
		
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}

		public string Connect
		{
			get { return _connect; }
			set
			{
				_connect = value;
				OnPropertyChanged();
			}
		}

		public string Disconnect
		{
			get { return _disconnect; }
			set
			{
				_disconnect = value;
				OnPropertyChanged();
			}
		}

		public string PrivateMessage
		{
			get { return _privateMessage; }
			set
			{
				_privateMessage = value;
				OnPropertyChanged();
			}
		}

		public string DccRequest
		{
			get { return _dccRequest; }
			set
			{
				_dccRequest = value;
				OnPropertyChanged();
			}
		}

		public string DccComplete
		{
			get { return _dccComplete; }
			set
			{
				_dccComplete = value;
				OnPropertyChanged();
			}
		}

		public string DccError
		{
			get { return _dccError; }
			set
			{
				_dccError = value;
				OnPropertyChanged();
			}
		}

		public string Notice
		{
			get { return _notice; }
			set
			{
				_notice = value;
				OnPropertyChanged();
			}
		}

		public string ActiveAlert
		{
			get { return _activeAlert; }
			set
			{
				_activeAlert = value;
				OnPropertyChanged();
			}
		}

		public string InactiveAlert
		{
			get { return _inactiveAlert; }
			set
			{
				_inactiveAlert = value;
				OnPropertyChanged();
			}
		}

		public string Beep
		{
			get { return _beep; }
			set
			{
				_beep = value;
				OnPropertyChanged();
			}
		}

	}
}
