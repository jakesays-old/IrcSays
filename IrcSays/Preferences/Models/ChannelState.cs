using System;
using IrcSays.Utility;

namespace IrcSays.Preferences.Models
{
	public sealed class ChannelState : PreferenceBase
	{
		private string _name;

		public ChannelState()
		{
			Placement = "";
			NickListWidth = 115.0;
			ColumnWidth = 125.0;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value.SafeTrim();
				if (_name == null)
				{
					throw new ArgumentException("Name cannot be empty.");
				}
			}
		}
		
		public bool IsDetached { get; set; }

		public string Placement { get; set; }


		public double NickListWidth { get; set; }


		public double ColumnWidth { get; set; }
	}
}