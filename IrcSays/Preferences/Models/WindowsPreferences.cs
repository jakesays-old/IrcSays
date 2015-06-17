using System.Collections.Generic;

namespace IrcSays.Preferences.Models
{
	public class WindowsPreferences : PreferenceBase
	{
		private string _placement;
		private string _customColors;
		private double _activeOpacity;
		private double _inactiveOpacity;
		private double _chromeOpacity;
		private double _backgroundOpacity;
		private bool _minimizeToSysTray;
		private TabStripPosition _tabStripPosition;
		private double _minTabWidth;
		private bool _defaultQueryDetached;
		private bool _suppressWarningOnQuit;
		private List<ChannelState> _states;

		public WindowsPreferences()
		{
			_placement = null;
			_customColors = "";
			_activeOpacity = 1;
			_inactiveOpacity = 1;
			_chromeOpacity = 1;
			_backgroundOpacity = 0.92;
			_minimizeToSysTray = false;
			_tabStripPosition = TabStripPosition.Bottom;
			_minTabWidth = 75;
			_defaultQueryDetached = false;
			_suppressWarningOnQuit = false;
			_states = null;
		}
		
		public string Placement
		{
			get { return _placement; }
			set
			{
				_placement = value;
				OnPropertyChanged();
			}
		}

		public string CustomColors
		{
			get { return _customColors; }
			set
			{
				_customColors = value;
				OnPropertyChanged();
			}
		}

		public double ActiveOpacity
		{
			get { return _activeOpacity; }
			set
			{
				_activeOpacity = value;
				OnPropertyChanged();
			}
		}

		public double InactiveOpacity
		{
			get { return _inactiveOpacity; }
			set
			{
				_inactiveOpacity = value;
				OnPropertyChanged();
			}
		}

		public double ChromeOpacity
		{
			get { return _chromeOpacity; }
			set
			{
				_chromeOpacity = value;
				OnPropertyChanged();
			}
		}

		public double BackgroundOpacity
		{
			get { return _backgroundOpacity; }
			set
			{
				_backgroundOpacity = value;
				OnPropertyChanged();
			}
		}

		public bool MinimizeToSysTray
		{
			get { return _minimizeToSysTray; }
			set
			{
				_minimizeToSysTray = value;
				OnPropertyChanged();
			}
		}

		public TabStripPosition TabStripPosition
		{
			get { return _tabStripPosition; }
			set
			{
				_tabStripPosition = value;
				OnPropertyChanged();
			}
		}

		public double MinTabWidth
		{
			get { return _minTabWidth; }
			set
			{
				_minTabWidth = value;
				OnPropertyChanged();
			}
		}

		public bool DefaultQueryDetached
		{
			get { return _defaultQueryDetached; }
			set
			{
				_defaultQueryDetached = value;
				OnPropertyChanged();
			}
		}

		public bool SuppressWarningOnQuit
		{
			get { return _suppressWarningOnQuit; }
			set
			{
				_suppressWarningOnQuit = value;
				OnPropertyChanged();
			}
		}

		public List<ChannelState> States
		{
			get { return _states ?? (_states = new List<ChannelState>()); }
			set
			{
				_states = value;
				OnPropertyChanged();
			}
		}
	}
}
