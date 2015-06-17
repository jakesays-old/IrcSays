namespace IrcSays.Preferences.Models
{
	public class FormattingPreferences : PreferenceBase
	{
		private string _fontFamily;
		private double _fontSize;
		private string _fontStyle;
		private string _fontWeight;
		private bool _showTimestamp;
		private string _timestampFormat;
		private bool _useTabularView;
		private bool _colorizeNicknames;
		private int _nicknameColorSeed;
		private bool _attentionOnOwnNickname;
		private string _attentionPatterns;
		private bool _autoSizeColumn;

		public FormattingPreferences()
		{
			_fontFamily = "Consolas";
			_fontSize = 14;
			_fontStyle = "Normal";
			_fontWeight = "Black";
			_showTimestamp = true;
			_timestampFormat = "[HH:mm]";
			_useTabularView = true;
			_colorizeNicknames = true;
			_nicknameColorSeed = 0;
			_attentionOnOwnNickname = false;
			_attentionPatterns = "";
			_autoSizeColumn = true;

		}
		
		public string FontFamily
		{
			get { return _fontFamily; }
			set
			{
				_fontFamily = value;
				OnPropertyChanged();
			}
		}

		public double FontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = value;
				OnPropertyChanged();
			}
		}

		public string FontStyle
		{
			get { return _fontStyle; }
			set
			{
				_fontStyle = value;
				OnPropertyChanged();
			}
		}

		public string FontWeight
		{
			get { return _fontWeight; }
			set
			{
				_fontWeight = value;
				OnPropertyChanged();
			}
		}

		public bool ShowTimestamp
		{
			get { return _showTimestamp; }
			set
			{
				_showTimestamp = value;
				OnPropertyChanged();
			}
		}

		public string TimestampFormat
		{
			get { return _timestampFormat; }
			set
			{
				_timestampFormat = value;
				OnPropertyChanged();
			}
		}

		public bool UseTabularView
		{
			get { return _useTabularView; }
			set
			{
				_useTabularView = value;
				OnPropertyChanged();
			}
		}

		public bool ColorizeNicknames
		{
			get { return _colorizeNicknames; }
			set
			{
				_colorizeNicknames = value;
				OnPropertyChanged();
			}
		}

		public int NicknameColorSeed
		{
			get { return _nicknameColorSeed; }
			set
			{
				_nicknameColorSeed = value;
				OnPropertyChanged();
			}
		}

		public bool AttentionOnOwnNickname
		{
			get { return _attentionOnOwnNickname; }
			set
			{
				_attentionOnOwnNickname = value;
				OnPropertyChanged();
			}
		}

		public string AttentionPatterns
		{
			get { return _attentionPatterns; }
			set
			{
				_attentionPatterns = value;
				OnPropertyChanged();
			}
		}

		public bool AutoSizeColumn
		{
			get { return _autoSizeColumn; }
			set
			{
				_autoSizeColumn = value;
				OnPropertyChanged();
			}
		}

	}
}
