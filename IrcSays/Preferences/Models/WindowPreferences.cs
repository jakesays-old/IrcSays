using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using IrcSays.Configuration;
using JetBrains.Annotations;

namespace IrcSays.Preferences.Models
{
	public class WindowPreferences : PreferenceBase
	{
		private bool _suppressWarningOnQuit;
		private bool _defaultQueryDetached;
		private double _minTabWidth;
		private TabStripPosition _tabStripPosition;
		private bool _minimizeToSysTray;
		private double _backgroundOpacity;
		private double _chromeOpacity;
		private double _inactiveOpacity;
		private double _activeOpacity;
		private string _customColors;
		private string _placement;
		private List<ChannelState> _states;

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
				if (value.Equals(_activeOpacity))
				{
					return;
				}
				_activeOpacity = value;
				OnPropertyChanged();
			}
		}

		public double InactiveOpacity
		{
			get { return _inactiveOpacity; }
			set
			{
				_inactiveOpacity = Math.Round(value, 2);
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
