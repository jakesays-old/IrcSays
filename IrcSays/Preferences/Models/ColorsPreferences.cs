using System.Windows.Markup;

namespace IrcSays.Preferences.Models
{
	public class ColorsPreferences : PreferenceBase
	{
		private string _background;
		private string _editBackground;
		private string _edit;
		private string _default;
		private string _action;
		private string _ctcp;
		private string _info;
		private string _invite;
		private string _join;
		private string _kick;
		private string _mode;
		private string _nick;
		private string _notice;
		private string _own;
		private string _part;
		private string _quit;
		private string _topic;
		private string _error;
		private string _newMarker;
		private string _oldMarker;
		private string _attention;
		private string _noiseActivity;
		private string _chatActivity;
		private string _alert;
		private string _windowBackground;
		private string _windowForeground;
		private string _highlight;
		private string _color0;
		private string _color1;
		private string _color2;
		private string _color3;
		private string _color4;
		private string _color5;
		private string _color6;
		private string _color7;
		private string _color8;
		private string _color9;
		private string _color10;
		private string _color11;
		private string _color12;
		private string _color13;
		private string _color14;
		private string _color15;
		private string _transmit;
		private ChatPalette _palette;
		private bool _colorizeNicknames;
		private int _nicknameColorSeed;
		public ColorsPreferences()
		{
			_background = "Black";
			_editBackground = "Black";
			_edit = "White";
			_default = "White";
			_action = "White";
			_ctcp = "Yellow";
			_info = "White";
			_invite = "Yellow";
			_join = "Lavender";
			_kick = "Lavender";
			_mode = "Yellow";
			_nick = "Yellow";
			_notice = "Yellow";
			_own = "Gray";
			_part = "Lavender";
			_quit = "Lavender";
			_topic = "Yellow";
			_error = "Red";
			_newMarker = "#FF002B00";
			_oldMarker = "#FF3C0000";
			_attention = "#404000";
			_noiseActivity = "#647491";
			_chatActivity = "#A58F5A";
			_alert = "#FFFF00";
			_windowBackground = "#293955";
			_windowForeground = "White";
			_highlight = "#3399FF";
			_color0 = "#FFFFFF";
			_color1 = "#000000";
			_color2 = "#00007F";
			_color3 = "#009300";
			_color4 = "#FF0000";
			_color5 = "#7F0000";
			_color6 = "#9C009C";
			_color7 = "#FC7F00";
			_color8 = "#FFFF00";
			_color9 = "#00FC00";
			_color10 = "#009393";
			_color11 = "#00FFFF";
			_color12 = "#0000FC";
			_color13 = "#FF00FF";
			_color14 = "#7F7F7F";
			_color15 = "#D2D2D2";
			_transmit = "#00FF00";
			_palette = default(ChatPalette);
			_colorizeNicknames = true;
			_nicknameColorSeed = 20;
		}
		
		public string Background
		{
			get { return _background; }
			set
			{
				_background = value;
				OnPropertyChanged();
			}
		}

		public string EditBackground
		{
			get { return _editBackground; }
			set
			{
				_editBackground = value;
				OnPropertyChanged();
			}
		}

		public string Edit
		{
			get { return _edit; }
			set
			{
				_edit = value;
				OnPropertyChanged();
			}
		}

		public string Default
		{
			get { return _default; }
			set
			{
				_default = value;
				OnPropertyChanged();
			}
		}

		public string Action
		{
			get { return _action; }
			set
			{
				_action = value;
				OnPropertyChanged();
			}
		}

		public string Ctcp
		{
			get { return _ctcp; }
			set
			{
				_ctcp = value;
				OnPropertyChanged();
			}
		}

		public string Info
		{
			get { return _info; }
			set
			{
				_info = value;
				OnPropertyChanged();
			}
		}

		public string Invite
		{
			get { return _invite; }
			set
			{
				_invite = value;
				OnPropertyChanged();
			}
		}

		public string Join
		{
			get { return _join; }
			set
			{
				_join = value;
				OnPropertyChanged();
			}
		}

		public string Kick
		{
			get { return _kick; }
			set
			{
				_kick = value;
				OnPropertyChanged();
			}
		}

		public string Mode
		{
			get { return _mode; }
			set
			{
				_mode = value;
				OnPropertyChanged();
			}
		}

		public string Nick
		{
			get { return _nick; }
			set
			{
				_nick = value;
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

		public string Own
		{
			get { return _own; }
			set
			{
				_own = value;
				OnPropertyChanged();
			}
		}

		public string Part
		{
			get { return _part; }
			set
			{
				_part = value;
				OnPropertyChanged();
			}
		}

		public string Quit
		{
			get { return _quit; }
			set
			{
				_quit = value;
				OnPropertyChanged();
			}
		}

		public string Topic
		{
			get { return _topic; }
			set
			{
				_topic = value;
				OnPropertyChanged();
			}
		}

		public string Error
		{
			get { return _error; }
			set
			{
				_error = value;
				OnPropertyChanged();
			}
		}

		public string NewMarker
		{
			get { return _newMarker; }
			set
			{
				_newMarker = value;
				OnPropertyChanged();
			}
		}

		public string OldMarker
		{
			get { return _oldMarker; }
			set
			{
				_oldMarker = value;
				OnPropertyChanged();
			}
		}

		public string Attention
		{
			get { return _attention; }
			set
			{
				_attention = value;
				OnPropertyChanged();
			}
		}

		public string NoiseActivity
		{
			get { return _noiseActivity; }
			set
			{
				_noiseActivity = value;
				OnPropertyChanged();
			}
		}

		public string ChatActivity
		{
			get { return _chatActivity; }
			set
			{
				_chatActivity = value;
				OnPropertyChanged();
			}
		}

		public string Alert
		{
			get { return _alert; }
			set
			{
				_alert = value;
				OnPropertyChanged();
			}
		}

		public string WindowBackground
		{
			get { return _windowBackground; }
			set
			{
				_windowBackground = value;
				OnPropertyChanged();
			}
		}

		public string WindowForeground
		{
			get { return _windowForeground; }
			set
			{
				_windowForeground = value;
				OnPropertyChanged();
			}
		}

		public string Highlight
		{
			get { return _highlight; }
			set
			{
				_highlight = value;
				OnPropertyChanged();
			}
		}

		public string Color0
		{
			get { return _color0; }
			set
			{
				_color0 = value;
				OnPropertyChanged();
			}
		}

		public string Color1
		{
			get { return _color1; }
			set
			{
				_color1 = value;
				OnPropertyChanged();
			}
		}

		public string Color2
		{
			get { return _color2; }
			set
			{
				_color2 = value;
				OnPropertyChanged();
			}
		}

		public string Color3
		{
			get { return _color3; }
			set
			{
				_color3 = value;
				OnPropertyChanged();
			}
		}

		public string Color4
		{
			get { return _color4; }
			set
			{
				_color4 = value;
				OnPropertyChanged();
			}
		}

		public string Color5
		{
			get { return _color5; }
			set
			{
				_color5 = value;
				OnPropertyChanged();
			}
		}

		public string Color6
		{
			get { return _color6; }
			set
			{
				_color6 = value;
				OnPropertyChanged();
			}
		}

		public string Color7
		{
			get { return _color7; }
			set
			{
				_color7 = value;
				OnPropertyChanged();
			}
		}

		public string Color8
		{
			get { return _color8; }
			set
			{
				_color8 = value;
				OnPropertyChanged();
			}
		}

		public string Color9
		{
			get { return _color9; }
			set
			{
				_color9 = value;
				OnPropertyChanged();
			}
		}

		public string Color10
		{
			get { return _color10; }
			set
			{
				_color10 = value;
				OnPropertyChanged();
			}
		}

		public string Color11
		{
			get { return _color11; }
			set
			{
				_color11 = value;
				OnPropertyChanged();
			}
		}

		public string Color12
		{
			get { return _color12; }
			set
			{
				_color12 = value;
				OnPropertyChanged();
			}
		}

		public string Color13
		{
			get { return _color13; }
			set
			{
				_color13 = value;
				OnPropertyChanged();
			}
		}

		public string Color14
		{
			get { return _color14; }
			set
			{
				_color14 = value;
				OnPropertyChanged();
			}
		}

		public string Color15
		{
			get { return _color15; }
			set
			{
				_color15 = value;
				OnPropertyChanged();
			}
		}

		public string Transmit
		{
			get { return _transmit; }
			set
			{
				_transmit = value;
				OnPropertyChanged();
			}
		}

		public ChatPalette Palette
		{
			get { return _palette; }
			set
			{
				_palette = value;
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
	}
}
