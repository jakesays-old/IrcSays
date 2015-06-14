using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using IrcSays.Communication.Irc;

namespace IrcSays.Ui
{
	public partial class ChatWindow : Window
	{
		private WindowInteropHelper _mainWindowHelper;

		private static ChatWindow _activeWindow;

		public static IntPtr ActiveWindowHandle
		{
			get
			{
				if (_activeWindow == null)
				{
					return IntPtr.Zero;
				}

				return _activeWindow._mainWindowHelper.Handle;
			}
		}

		public ObservableCollection<ChatTabItem> Items { get; private set; }

		public ChatControl ActiveControl
		{
			get { return tabsChat.SelectedContent as ChatControl; }
		}

		public ChatWindow()
		{
			Items = new ObservableCollection<ChatTabItem>();
			DataContext = this;
			InitializeComponent();

			_mainWindowHelper = new WindowInteropHelper(this);

			Loaded += ChatWindow_Loaded;
		}

		public void AddPage(ChatPage page, bool switchToPage)
		{
			var item = new ChatTabItem(page);

			if (page.Type == ChatPageType.Server)
			{
				Items.Add(item);
				SubscribeEvents(page.Session);
			}
			else
			{
				for (var i = Items.Count - 1; i >= 0; --i)
				{
					if (Items[i].Page.Session == page.Session)
					{
						Items.Insert(i + 1, item);
						break;
					}
				}
			}
			if (switchToPage)
			{
				var oldItem = tabsChat.SelectedItem as TabItem;
				if (oldItem != null)
				{
					oldItem.IsSelected = false;
				}
				item.IsSelected = true;
			}
		}

		public void RemovePage(ChatPage page)
		{
			if (page.Type == ChatPageType.Server)
			{
				UnsubscribeEvents(page.Session);
			}
			page.Dispose();
			Items.Remove(Items.FirstOrDefault(i => i.Page == page));
		}

		public void SwitchToPage(ChatPage page)
		{
			var index = Items.Where(tab => tab.Page == page).Select((t, i) => i).FirstOrDefault();
			tabsChat.SelectedIndex = index;
		}

		public ChatPage FindPage(ChatPageType type, IrcSession session, IrcTarget target)
		{
			return Items.Where(i => i.Page.Type == type && i.Page.Session == session && i.Page.Target != null &&
									i.Page.Target.Equals(target)).Select(i => i.Page).FirstOrDefault();
		}

		public void Attach(ChatPage page)
		{
			for (var i = Items.Count - 1; i >= 0; --i)
			{
				if (Items[i].Page.Session == page.Session)
				{
					Items.Insert(++i, new ChatTabItem(page));
					tabsChat.SelectedIndex = i;
					break;
				}
			}

			SwitchToPage(page);
		}

		public void Alert(string text)
		{
			if (_notifyIcon != null &&
				_notifyIcon.IsVisible)
			{
				_notifyIcon.Show("IRC Alert", text);
			}
		}

		private void QuitAllSessions()
		{
			foreach (var i in Items.Where(i => i.Page.Type == ChatPageType.Server).Select(i => i))
			{
				if (i.Page.Session.State == IrcSessionState.Connected)
				{
					i.Page.Session.AutoReconnect = false;
					i.Page.Session.Quit("Leaving");
				}
			}
		}
	}
}