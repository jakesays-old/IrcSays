using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IrcSays.Application;
using IrcSays.Communication.Irc;

namespace IrcSays.Ui
{
	public partial class ListControl : ChatPage
	{
		public class ChannelItem : IComparable<ChannelItem>
		{
			public string Name { get; private set; }
			public int Count { get; private set; }
			public string Topic { get; private set; }

			public ChannelItem(string name, int count, string topic)
			{
				Name = name;
				Count = count;
				Topic = topic;
			}

			public int CompareTo(ChannelItem other)
			{
				return string.Compare(Name, other.Name);
			}
		}

		public static readonly DependencyProperty CountProperty =
			DependencyProperty.Register("Count", typeof (int), typeof (ListControl));

		public int Count
		{
			get { return (int) GetValue(CountProperty); }
			set { SetValue(CountProperty, value); }
		}

		private readonly List<ChannelItem> _channels;

		public ListControl(IrcSession session)
			: base(ChatPageType.ChannelList, session, null, "chan-list")
		{
			_channels = new List<ChannelItem>();
			InitializeComponent();
			Header = "Channel List";
			Title = string.Format("{0} - {1} - {2} Channel List", AppInfo.Product, Session.Nickname, Session.NetworkName);
			IsCloseable = false;
			Session.InfoReceived += Session_InfoReceived;

			var menu = Resources["cmChannels"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
		}

		public void ExecuteJoin(object sender, ExecutedRoutedEventArgs e)
		{
			var channel = e.Parameter as string;
			if (!string.IsNullOrEmpty(channel))
			{
				Session.Join(channel);
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.RplList:
					int count;
					if (e.Message.Parameters.Count == 4 &&
						int.TryParse(e.Message.Parameters[2], out count))
					{
						_channels.Add(new ChannelItem(e.Message.Parameters[1], count, e.Message.Parameters[3]));
						Count++;
					}
					break;
				case IrcCode.RplListEnd:
					IsCloseable = true;
					Session.InfoReceived -= Session_InfoReceived;
					_channels.Sort();
					foreach (var c in _channels)
					{
						lstChannels.Items.Add(c);
					}
					break;
			}
		}

		private void lstChannels_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var chanItem = ((ListBoxItem) e.Source).Content as ChannelItem;
			if (chanItem != null)
			{
				ChatControl.JoinCommand.Execute(chanItem.Name, this);
			}
		}
	}
}