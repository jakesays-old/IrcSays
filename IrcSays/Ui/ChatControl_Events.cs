using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IrcSays.Application;
using IrcSays.Communication.Irc;
using IrcSays.Configuration;
using IrcSays.Interop;

namespace IrcSays.Ui
{
	public partial class ChatControl : ChatPage
	{
		private char[] _channelModes = new char[0];
		private string _topic = "", _prefix;
		private bool _hasDeactivated = false;
		private bool _usingAlternateNick = false;
		private Window _window;

		private void Session_StateChanged(object sender, EventArgs e)
		{
			var state = Session.State;
			IsConnected = state != IrcSessionState.Disconnected;

			if (state == IrcSessionState.Disconnected)
			{
				Write("Error", "Disconnected");
			}

			if (IsServer)
			{
				switch (state)
				{
					case IrcSessionState.Connecting:
						_usingAlternateNick = false;
						Header = Session.NetworkName;
						Write("Client", string.Format(
							"Connecting to {0}:{1}", Session.Server, Session.Port));
						break;
					case IrcSessionState.Connected:
						Header = Session.NetworkName;
						App.DoEvent("connect");
						if (Perform != null)
						{
							DoPerform(0);
						}
						break;
				}
				SetTitle();
			}
		}

		private void Session_ConnectionError(object sender, ErrorEventArgs e)
		{
			if (IsServer)
			{
				Write("Error", string.IsNullOrEmpty(e.Exception.Message) ? e.Exception.GetType().Name : e.Exception.Message);
			}
		}

		private void Session_Noticed(object sender, IrcMessageEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Notice))
			{
				return;
			}

			if (IsServer)
			{
				if (e.From is IrcPeer)
				{
					Write("Notice", e.From, e.Text, false);
				}
				else if (IsServer)
				{
					Write("Notice", e.Text);
				}
				App.DoEvent("notice");
			}
		}

		private void Session_PrivateMessaged(object sender, IrcMessageEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, e.To.IsChannel ? IgnoreActions.Channel : IgnoreActions.Private))
			{
				return;
			}

			if (!IsServer)
			{
				if ((Target.IsChannel && Target.Equals(e.To)) ||
					(!Target.IsChannel && Target.Equals(new IrcTarget(e.From)) && !e.To.IsChannel))
				{
					var attn = false;
					if (App.IsAttentionMatch(Session.Nickname, e.Text))
					{
						attn = true;
						if (_window != null)
						{
							App.Alert(_window, string.Format("You received an alert from {0}", Target.Name));
						}
						if (VisualParent == null)
						{
							NotifyState = NotifyState.Alert;
							App.DoEvent("inactiveAlert");
						}
						else if (_window != null &&
								!_window.IsActive)
						{
							App.DoEvent("inactiveAlert");
						}
						else
						{
							App.DoEvent("activeAlert");
						}
					}

					Write("Default", e.From, e.Text, attn);
                    if (e.From.Nickname != null)
                    {
                        var gonnaRemove = _mostRecentTalkers.FirstOrDefault(x => x == e.From.Nickname);
                        if (gonnaRemove != null)
                        {
                            _mostRecentTalkers.Remove(gonnaRemove);
                        }
                        _mostRecentTalkers.Insert(0, e.From.Nickname);
                        if (_mostRecentTalkers.Count > 10)
                            _mostRecentTalkers.RemoveAt(_mostRecentTalkers.Count - 1);
                    }
					if (!Target.IsChannel)
					{
						if (e.From.Prefix != _prefix)
						{
							_prefix = e.From.Prefix;
							SetTitle();
						}
						WindowHelper.FlashWindow(_window);
						if (VisualParent == null)
						{
							App.DoEvent("privateMessage");
						}
					}
				}
			}
		}

		private void Session_Kicked(object sender, IrcKickEventArgs e)
		{
			if (!IsServer &&
				Target.Equals(e.Channel))
			{
				Write("Kick",
					e.Kicker == null
						? string.Format("{0} has been kicked ({1}", e.KickeeNickname, e.Text)
						: string.Format("{0} has been kicked by {1} ({2})", e.KickeeNickname, e.Kicker.Nickname, e.Text));
				_nickList.Remove(e.KickeeNickname);
			}
		}

		private void Session_SelfKicked(object sender, IrcKickEventArgs e)
		{
			if (IsServer)
			{
				Write("Kick", string.Format("You have been kicked from {0} by {1} ({2})",
					e.Channel, e.Kicker.Nickname, e.Text));
			}
		}

		private void Session_InfoReceived(object sender, IrcInfoEventArgs e)
		{
			switch (e.Code)
			{
				case IrcCode.ErrNicknameInUse:
					if (IsServer && Session.State == IrcSessionState.Connecting)
					{
						if (_usingAlternateNick || string.IsNullOrEmpty(App.Settings.Current.User.AlternateNickname))
						{
							SetInputText("/nick ");
						}
						else
						{
							Session.Nick(App.Settings.Current.User.AlternateNickname);
							_usingAlternateNick = true;
						}
					}
					break;
				case IrcCode.RplTopic:
					if (e.Message.Parameters.Count == 3 &&
						!IsServer &&
						Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						_topic = e.Message.Parameters[2];
						SetTitle();
						Write("Topic", string.Format("Topic is: {0}", _topic));
					}
					return;
				case IrcCode.RplTopicSetBy:
					if (e.Message.Parameters.Count == 4 &&
						!IsServer &&
						Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						Write("Topic", string.Format("Topic set by {0} on {1}", e.Message.Parameters[2],
							FormatTime(e.Message.Parameters[3])));
					}
					return;
				case IrcCode.RplChannelCreatedOn:
					if (e.Message.Parameters.Count == 3 &&
						!IsServer &&
						Target.Equals(new IrcTarget(e.Message.Parameters[1])))
					{
						//this.Write("ServerInfo", string.Format("* Channel created on {0}", this.FormatTime(e.Message.Parameters[2])));
					}
					return;
				case IrcCode.RplWhoisUser:
				case IrcCode.RplWhoWasUser:
					if (e.Message.Parameters.Count == 6 && IsDefault)
					{
						Write("ServerInfo",
							string.Format("{1} " + (e.Code == IrcCode.RplWhoWasUser ? "was" : "is") + " {2}@{3} {4} {5}",
								(object[]) e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RplWhoisChannels:
					if (e.Message.Parameters.Count == 3 && IsDefault)
					{
						Write("ServerInfo", string.Format("{1} is on {2}",
							(object[]) e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RplWhoisServer:
					if (e.Message.Parameters.Count == 4 && IsDefault)
					{
						Write("ServerInfo", string.Format("{1} using {2} {3}",
							(object[]) e.Message.Parameters));
						return;
					}
					break;
				case IrcCode.RplWhoisIdle:
					if (e.Message.Parameters.Count == 5 && IsDefault)
					{
						Write("ServerInfo", string.Format("{0} has been idle {1}, signed on {2}",
							e.Message.Parameters[1], FormatTimeSpan(e.Message.Parameters[2]),
							FormatTime(e.Message.Parameters[3])));
						return;
					}
					break;
				case IrcCode.RplInviting:
					if (e.Message.Parameters.Count == 3 && IsDefault)
					{
						Write("ServerInfo", string.Format("Invited {0} to channel {1}",
							e.Message.Parameters[1], e.Message.Parameters[2]));
						return;
					}
					break;
				case IrcCode.RplList:
				case IrcCode.RplListStart:
				case IrcCode.RplListEnd:
					e.Handled = true;
					break;
			}

			if (!e.Handled &&
				((int) e.Code < 200 && IsServer || IsDefault))
			{
				Write("ServerInfo", e.Text);
			}
		}

		private bool IsDefault
		{
			get
			{
				if (_window is ChannelWindow &&
					_window.IsActive)
				{
					return true;
				}
				else if (_window is ChatWindow)
				{
					if (IsVisible)
					{
						return true;
					}

					if (IsServer &&
						!((ChatWindow) _window).Items.Any(item => item.IsVisible && item.Page.Session == Session) &&
						!App.Current.Windows.OfType<ChannelWindow>().Any(cw => cw.Session == Session && cw.IsActive))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void Session_CtcpCommandReceived(object sender, CtcpEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Ctcp))
			{
				return;
			}

			if (((IsChannel && Target.Equals(e.To)) ||
				(IsNickname && Target.Equals(new IrcTarget(e.From)) && !e.To.IsChannel))
				&&
				e.Command.Command == "ACTION")
			{
				var text = string.Join(" ", e.Command.Arguments);
				var attn = false;
				if (App.IsAttentionMatch(Session.Nickname, text))
				{
					attn = true;
					if (_window != null)
					{
						WindowHelper.FlashWindow(_window);
					}
				}

				Write("Action", string.Format("{0} {1}", e.From.Nickname, text, attn));
			}
			else if (IsServer &&
					e.Command.Command != "ACTION" &&
					e.From != null)
			{
				Write("Ctcp", e.From, string.Format("[CTCP {0}] {1}", e.Command.Command,
					e.Command.Arguments.Length > 0 ? string.Join(" ", e.Command.Arguments) : ""), false);
			}
		}

		private void Session_Joined(object sender, IrcJoinEventArgs e)
		{
			var isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Join);

			if (!IsServer &&
				Target.Equals(e.Channel))
			{
				if (!isIgnored)
				{
					Write("Join", string.Format("{0} ({1}@{2}) has joined channel {3}",
						e.Who.Nickname, e.Who.Username, e.Who.Hostname, Target.ToString()));
				}
				_nickList.Add(e.Who.Nickname);
			}
		}

		private void Session_Parted(object sender, IrcPartEventArgs e)
		{
			var isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Part);

			if (!IsServer &&
				Target.Equals(e.Channel))
			{
				if (!isIgnored)
				{
					Write("Part", string.Format("{0} ({1}@{2}) has left channel {3}",
						e.Who.Nickname, e.Who.Username, e.Who.Hostname, Target.ToString()));
				}
				_nickList.Remove(e.Who.Nickname);
			}
		}

		private void Session_NickChanged(object sender, IrcNickEventArgs e)
		{
			var isIgnored = App.IsIgnoreMatch(e.Message.From, IgnoreActions.NickChange);

			if (IsChannel && _nickList.Contains(e.OldNickname))
			{
				if (!isIgnored)
				{
					Write("Nick", string.Format("{0} is now known as {1}", e.OldNickname, e.NewNickname));
				}
				_nickList.ChangeNick(e.OldNickname, e.NewNickname);
			}
		}

		private void Session_SelfNickChanged(object sender, IrcNickEventArgs e)
		{
			if (IsServer || IsChannel)
			{
				Write("Nick", string.Format("You are now known as {0}", e.NewNickname));
			}
			SetTitle();

			if (IsChannel)
			{
				_nickList.ChangeNick(e.OldNickname, e.NewNickname);
			}
		}

		private void Session_TopicChanged(object sender, IrcTopicEventArgs e)
		{
			if (!IsServer &&
				Target.Equals(e.Channel))
			{
				Write("Topic", string.Format("{0} changed topic to: {1}", e.Who.Nickname, e.Text));
				_topic = e.Text;
				SetTitle();
			}
		}

		private void Session_UserModeChanged(object sender, IrcUserModeEventArgs e)
		{
			if (IsServer)
			{
				Write("Mode", string.Format("You set mode: {0}", IrcUserMode.RenderModes(e.Modes)));
			}
			SetTitle();
		}

		private void Session_UserQuit(object sender, IrcQuitEventArgs e)
		{
			var isIgnored = App.IsIgnoreMatch(e.Who, IgnoreActions.Quit);

			if (IsChannel && _nickList.Contains(e.Who.Nickname))
			{
				if (!isIgnored)
				{
					Write("Quit", string.Format("{0} has quit ({1})", e.Who.Nickname, e.Text));
				}
				_nickList.Remove(e.Who.Nickname);
			}
		}

		private void Session_ChannelModeChanged(object sender, IrcChannelModeEventArgs e)
		{
			if (!IsServer &&
				Target.Equals(e.Channel))
			{
				if (e.Who != null)
				{
					Write("Mode", string.Format("{0} set mode: {1}", e.Who.Nickname,
						string.Join(" ", IrcChannelMode.RenderModes(e.Modes))));

					_channelModes = (from m in e.Modes.Where(newMode => newMode.Parameter == null && newMode.Set).
						Select(newMode => newMode.Mode).Union(_channelModes).Distinct()
						where !e.Modes.Any(newMode => !newMode.Set && newMode.Mode == m)
						select m).ToArray();
				}
				else
				{
					_channelModes = (from m in e.Modes
						where m.Set && m.Parameter == null
						select m.Mode).ToArray();
				}
				SetTitle();
				foreach (var mode in e.Modes)
				{
					_nickList.ProcessMode(mode);
				}
			}
		}

		private void Session_Invited(object sender, IrcInviteEventArgs e)
		{
			if (App.IsIgnoreMatch(e.From, IgnoreActions.Invite))
			{
				return;
			}

			if (IsDefault || IsServer)
			{
				Write("Invite", string.Format("{0} invited you to channel {1}", e.From.Nickname, e.Channel));
			}
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
			{
				var ctrl = 0;
				switch (e.Key)
				{
					case Key.B:
						ctrl = 2;
						break;
					case Key.K:
						ctrl = 3;
						break;
					case Key.R:
						ctrl = 22;
						break;
					case Key.O:
						ctrl = 15;
						break;
					case Key.U:
						ctrl = 31;
						break;
				}
				if (ctrl != 0)
				{
					var text = new string((char) (ctrl + 0x2500), 1);
					Insert(text);
				}
			}

			switch (e.Key)
			{
				case Key.Enter:
					SubmitInput();
					break;
			}
		}

		private void txtInput_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof (string)))
			{
				var text = e.DataObject.GetData(typeof (string)) as string;
				if (text.Contains(Environment.NewLine))
				{
					e.CancelCommand();

					var parts = text.Split(Environment.NewLine.ToCharArray()).Where(s => s.Trim().Length > 0).ToArray();
					if (parts.Length > App.Settings.Current.Buffer.MaximumPasteLines)
					{
						Dispatcher.BeginInvoke((Action) (() =>
						{
							if (!App.Confirm(_window, string.Format("Are you sure you want to paste more than {0} lines?",
								App.Settings.Current.Buffer.MaximumPasteLines), "Paste Warning"))
							{
								return;
							}
							foreach (var part in parts)
							{
								txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
								txtInput.Text += part;
								SubmitInput();
							}
						}));
					}
					else
					{
						foreach (var part in parts)
						{
							txtInput.Text = txtInput.Text.Substring(0, txtInput.SelectionStart);
							txtInput.Text += part;
							SubmitInput();
						}
					}
				}
			}
		}

		private void lstNicknames_MouseDoubleClick(object sender, RoutedEventArgs e)
		{
			var listItem = e.Source as ListBoxItem;
			if (listItem != null)
			{
				var nickItem = listItem.Content as NicknameItem;
				if (nickItem != null)
				{
					ChatWindow.ChatCommand.Execute(nickItem.Nickname, this);
				}
			}
		}

		private void OnWindowDeactivated(object sender, EventArgs e)
		{
			_hasDeactivated = true;
			SelectedLink = null;
		}

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			_hasDeactivated = false;
			if (e.NewFocus == txtInput)
			{
				lstNicknames.SelectedItem = null;
			}
		}

		private void boxOutput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var link = boxOutput.SelectedLink;
			if (!string.IsNullOrEmpty(link))
			{
				if (Constants.UrlRegex.IsMatch(link))
				{
					App.BrowseTo(link);
				}
				else
				{
					ChatWindow.ChatCommand.Execute(GetNickWithoutLevel(link), this);
				}
			}
		}

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			SelectedLink = boxOutput.SelectedLink;
			if (!string.IsNullOrEmpty(SelectedLink))
			{
				if (Constants.UrlRegex.IsMatch(SelectedLink))
				{
					boxOutput.ContextMenu = Resources["cmHyperlink"] as ContextMenu;
				}
				else
				{
					if (Type == ChatPageType.DccChat)
					{
						return;
					}
					SelectedLink = GetNickWithoutLevel(SelectedLink);
					boxOutput.ContextMenu = Resources["cmNickname"] as ContextMenu;
				}
				boxOutput.ContextMenu.IsOpen = true;
				e.Handled = true;
			}
			else
			{
				boxOutput.ContextMenu = GetDefaultContextMenu();
				if (IsServer && boxOutput.ContextMenu != null)
				{
					boxOutput.ContextMenu.Items.Refresh();
				}
			}

			base.OnContextMenuOpening(e);
		}

		private void connect_Click(object sender, RoutedEventArgs e)
		{
			var item =
				((MenuItem) boxOutput.ContextMenu.Items[0]).ItemContainerGenerator.ItemFromContainer(
					(DependencyObject) e.OriginalSource)
					as ServerElement;
			if (item != null)
			{
				if (IsConnected)
				{
					Session.Quit("Changing servers");
				}
				Connect(item);
			}
		}

		private void ChatControl_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(txtInput);
			SetTitle();

			if (_window == null)
			{
				_window = Window.GetWindow(this);
				if (_window != null)
				{
					_window.Deactivated += OnWindowDeactivated;
				}
			}
			else
			{
				_window = Window.GetWindow(this);
				NotifyState = NotifyState.None;
			}
		}

		private void ChatControl_Unloaded(object sender, RoutedEventArgs e)
		{
			_hasDeactivated = true;
			SelectedLink = null;
			if (_window != null)
			{
				_window.Deactivated -= OnWindowDeactivated;
			}
		}

		private void txtInput_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (_tabKeyCount == 0)
			{
				_nickCandidates = null;
			}
		}

		protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
		{
			SelectedLink = null;
			base.OnPreviewMouseRightButtonDown(e);
		}

		private int _tabKeyCount = 0;

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			var focused = FocusManager.GetFocusedElement(this);
			if (focused is TextBox &&
				focused != txtInput)
			{
				return;
			}

			if ((Keyboard.Modifiers & ModifierKeys.Alt) == 0 &&
				(Keyboard.Modifiers & ModifierKeys.Control) == 0 &&
				!(FocusManager.GetFocusedElement(this) is ListBoxItem))
			{
				e.Handled = true;

				if (e.Key != Key.Tab)
				{
					ResetNickCompletion();
				}

				switch (e.Key)
				{
					case Key.PageUp:
						boxOutput.PageUp();
						break;
					case Key.PageDown:
						boxOutput.PageDown();
						break;
					case Key.Up:
						if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) > 0)
						{
							e.Handled = false;
							return;
						}
						if (_historyNode != null)
						{
							if (_historyNode.Next != null)
							{
								_historyNode = _historyNode.Next;
								SetInputText(_historyNode.Value);
							}
						}
						else if (_history.First != null)
						{
							_historyNode = _history.First;
							SetInputText(_historyNode.Value);
						}
						break;
					case Key.Down:
						if (txtInput.GetLineIndexFromCharacterIndex(txtInput.CaretIndex) < txtInput.LineCount - 1)
						{
							e.Handled = false;
							return;
						}
						if (_historyNode != null)
						{
							_historyNode = _historyNode.Previous;
							if (_historyNode != null)
							{
								SetInputText(_historyNode.Value);
							}
							else
							{
								txtInput.Clear();
							}
						}
						else
						{
							txtInput.Clear();
						}
						break;
					case Key.Tab:
						_tabKeyCount += 1;
						if (IsChannel || IsNickname)
						{
							DoNickCompletion();
						}
						break;
					default:
						Keyboard.Focus(txtInput);
						e.Handled = false;
						break;
				}
			}
			else if (e.Key >= Key.A &&
					e.Key <= Key.Z)
			{
				Keyboard.Focus(txtInput);
			}

			base.OnPreviewKeyDown(e);
		}

		protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				boxOutput.MouseWheelUp();
			}
			else
			{
				boxOutput.MouseWheelDown();
			}
			e.Handled = true;

			base.OnPreviewMouseWheel(e);
		}

		private void SubscribeEvents()
		{
			Session.StateChanged += Session_StateChanged;
			Session.ConnectionError += Session_ConnectionError;
			Session.Noticed += Session_Noticed;
			Session.PrivateMessaged += Session_PrivateMessaged;
			Session.Kicked += Session_Kicked;
			Session.SelfKicked += Session_SelfKicked;
			Session.InfoReceived += Session_InfoReceived;
			Session.CtcpCommandReceived += Session_CtcpCommandReceived;
			Session.Joined += Session_Joined;
			Session.Parted += Session_Parted;
			Session.NickChanged += Session_NickChanged;
			Session.SelfNickChanged += Session_SelfNickChanged;
			Session.TopicChanged += Session_TopicChanged;
			Session.UserModeChanged += Session_UserModeChanged;
			Session.ChannelModeChanged += Session_ChannelModeChanged;
			Session.UserQuit += Session_UserQuit;
			Session.Invited += Session_Invited;
			DataObject.AddPastingHandler(txtInput, txtInput_Pasting);

			IsConnected = Session.State != IrcSessionState.Disconnected;
		}

		private void UnsubscribeEvents()
		{
			Session.StateChanged -= Session_StateChanged;
			Session.ConnectionError -= Session_ConnectionError;
			Session.Noticed -= Session_Noticed;
			Session.PrivateMessaged -= Session_PrivateMessaged;
			Session.Kicked -= Session_Kicked;
			Session.SelfKicked -= Session_SelfKicked;
			Session.InfoReceived -= Session_InfoReceived;
			Session.CtcpCommandReceived -= Session_CtcpCommandReceived;
			Session.Joined -= Session_Joined;
			Session.Parted -= Session_Parted;
			Session.NickChanged -= Session_NickChanged;
			Session.SelfNickChanged -= Session_SelfNickChanged;
			Session.TopicChanged -= Session_TopicChanged;
			Session.UserModeChanged -= Session_UserModeChanged;
			Session.ChannelModeChanged -= Session_ChannelModeChanged;
			Session.UserQuit -= Session_UserQuit;
			Session.Invited -= Session_Invited;
			DataObject.RemovePastingHandler(txtInput, txtInput_Pasting);

			if (_window != null)
			{
				_window.Deactivated -= OnWindowDeactivated;
			}
		}

		private void PrepareContextMenus()
		{
			var menu = Resources["cmServer"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = Resources["cmNickList"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = Resources["cmNickname"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = Resources["cmHyperlink"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
			menu = Resources["cmChannel"] as ContextMenu;
			if (menu != null)
			{
				NameScope.SetNameScope(menu, NameScope.GetNameScope(this));
			}
		}

		private string FormatTime(string text)
		{
			var seconds = 0;
			if (!int.TryParse(text, out seconds))
			{
				return "";
			}
			var ts = new TimeSpan(0, 0, seconds);
			return new DateTime(1970, 1, 1).Add(ts).ToLocalTime().ToString();
		}

		private string FormatTimeSpan(string text)
		{
			var seconds = 0;
			if (!int.TryParse(text, out seconds))
			{
				return "";
			}
			return new TimeSpan(0, 0, seconds).ToString();
		}
	}
}