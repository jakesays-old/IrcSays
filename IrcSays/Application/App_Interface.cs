using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using IrcSays.Communication.Irc;
using IrcSays.Communication.Network;
using IrcSays.Interop;
using IrcSays.Settings;
using IrcSays.Ui;
using Microsoft.Win32;

namespace IrcSays.Application
{
	public partial class App
	{
		public static ChatWindow ChatWindow
		{
			get { return Current.MainWindow as ChatWindow; }
		}

		public static ProxyInfo ProxyInfo
		{
			get
			{
				return Settings.Current.Network.UseSocks5Proxy
					? new ProxyInfo(Settings.Current.Network.ProxyHostname,
						Settings.Current.Network.ProxyPort,
						Settings.Current.Network.ProxyUsername,
						Settings.Current.Network.ProxyPassword)
					: null;
			}
		}

		public static ChatPage ActiveChatPage
		{
			get
			{
				var chatWindow = Current.Windows.OfType<ChatWindow>().FirstOrDefault((w) => w.IsActive);
				if (chatWindow != null)
				{
					return chatWindow.ActiveControl;
				}
				var channelWindow = Current.Windows.OfType<ChannelWindow>().FirstOrDefault((w) => w.IsActive);
				if (channelWindow != null)
				{
					return channelWindow.Page;
				}

				return null;
			}
		}

		public static void ShowSettings()
		{
			var settings = new SettingsWindow();
			settings.Owner = Current.MainWindow;
			settings.ShowDialog();
			RefreshAttentionPatterns();
		}

		public static bool Confirm(Window owner, string text, string caption)
		{
			var dummy = false;
			return Confirm(owner, text, caption, ref dummy);
		}

		public static bool Confirm(Window owner, string text, string caption, ref bool dontAskAgain)
		{
			var confirm = new ConfirmDialog(caption, text, dontAskAgain);
			confirm.Owner = owner;
			var result = confirm.ShowDialog().Value;
			dontAskAgain = confirm.IsDontAskAgainChecked;
			return result;
		}

		public static void Alert(Window window, string text)
		{
			WindowHelper.FlashWindow(window);
			var chatWindow = window as ChatWindow;
			if (chatWindow != null)
			{
				chatWindow.Alert(text);
			}
		}

		public static string OpenFileDialog(Window owner, string initialDirectory)
		{
			var dialog = new OpenFileDialog
			{
				CheckFileExists = true, 
				Multiselect = false, 
				InitialDirectory = initialDirectory
			};
			return dialog.ShowDialog(owner) == true ? dialog.FileName : null;
		}

		public static void BrowseTo(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error running browser process: " + ex.Message);
			}
		}

		public static void Create(IrcSession session, ChatPage page, bool makeActive)
		{
			if (Settings.Current.Windows.States.Exists(page.Id)
				? Settings.Current.Windows.States[page.Id].IsDetached
				: Settings.Current.Windows.DefaultQueryDetached)
			{
				var newWin = new ChannelWindow(page);
				if (!makeActive)
				{
					newWin.ShowActivated = false;
					newWin.WindowState = WindowState.Minimized;
				}
				newWin.Show();

				if (makeActive)
				{
					newWin.Activate();
				}
				else
				{
					WindowHelper.FlashWindow(newWin);
				}
			}
			else
			{
				var window = Current.MainWindow as ChatWindow;
				window.AddPage(page, makeActive);
				if (!window.IsActive)
				{
					WindowHelper.FlashWindow(window);
				}
			}
		}

		public static bool Create(IrcSession session, IrcTarget target, bool makeActive)
		{
			var detached = Current.Windows
				.OfType<ChannelWindow>()
				.FirstOrDefault(cw => cw.Page.Session == session
									&& target.Equals(cw.Page.Target)
									&& cw.Page.Type == ChatPageType.Chat);
			if (detached != null)
			{
				if (makeActive)
				{
					detached.Activate();
				}
				return false;
			}

			var window = ChatWindow;
			var page = window.FindPage(ChatPageType.Chat, session, target);
			if (page != null)
			{
				if (makeActive)
				{
					window.Show();
					if (window.WindowState == WindowState.Minimized)
					{
						window.WindowState = WindowState.Normal;
					}
					window.Activate();
					window.SwitchToPage(page);
				}
				return false;
			}
			page = new ChatControl(target == null ? ChatPageType.Server : ChatPageType.Chat, session, target);
			Create(session, page, makeActive);
			return true;
		}

		public static void ClosePage(ChatPage page)
		{
			var window = Window.GetWindow(page);
			if (window is ChannelWindow)
			{
				window.Close();
			}
			else
			{
				ChatWindow.RemovePage(page);
			}
		}
	}
}