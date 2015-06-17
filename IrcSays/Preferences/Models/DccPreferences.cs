namespace IrcSays.Preferences.Models
{
	public class DccPreferences : PreferenceBase
	{
		private int _lowPort;
		private int _highPort;
		private string _downloadFolder;
		private bool _autoAccept;
		private bool _findExternalAddress;
		private string _dangerExtensions;
		private bool _enableUpnp;

		public DccPreferences()
		{
			_lowPort = 57000;
			_highPort = 58000;
			_downloadFolder = "";
			_autoAccept = false;
			_findExternalAddress = true;
			_dangerExtensions = "386 bat chm cmd com dll doc docx dot dotx drv exe hlp inf ini js jse lnk msi msp ocx ovl pif reg scr sys vb vbe vbs wsc wsf wsh xls xlsx";
			_enableUpnp = true;

		}
		
		public int LowPort
		{
			get { return _lowPort; }
			set
			{
				_lowPort = value;
				OnPropertyChanged();
			}
		}

		public int HighPort
		{
			get { return _highPort; }
			set
			{
				_highPort = value;
				OnPropertyChanged();
			}
		}

		public string DownloadFolder
		{
			get { return _downloadFolder; }
			set
			{
				_downloadFolder = value;
				OnPropertyChanged();
			}
		}

		public bool AutoAccept
		{
			get { return _autoAccept; }
			set
			{
				_autoAccept = value;
				OnPropertyChanged();
			}
		}

		public bool FindExternalAddress
		{
			get { return _findExternalAddress; }
			set
			{
				_findExternalAddress = value;
				OnPropertyChanged();
			}
		}

		public string DangerExtensions
		{
			get { return _dangerExtensions; }
			set
			{
				_dangerExtensions = value;
				OnPropertyChanged();
			}
		}

		public bool EnableUpnp
		{
			get { return _enableUpnp; }
			set
			{
				_enableUpnp = value;
				OnPropertyChanged();
			}
		}

	}
}
