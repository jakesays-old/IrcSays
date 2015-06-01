using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using IrcSays.Application;
using IrcSays.Communication;
using IrcSays.Communication.Irc;

namespace IrcSays.Ui
{
	public partial class ChatControl
	{
		private const char CommandChar = '/';

		public static readonly RoutedUICommand WhoisCommand = new RoutedUICommand("Whois", "Whois", typeof (ChatControl));
		public static readonly RoutedUICommand OpenLinkCommand = new RoutedUICommand("Open", "OpenLink", typeof (ChatControl));
		public static readonly RoutedUICommand CopyLinkCommand = new RoutedUICommand("Copy", "CopyLink", typeof (ChatControl));
		public static readonly RoutedUICommand QuitCommand = new RoutedUICommand("Disconnect", "Quit", typeof (ChatControl));
		public static readonly RoutedUICommand ClearCommand = new RoutedUICommand("Clear", "Clear", typeof (ChatControl));
		public static readonly RoutedUICommand InsertCommand = new RoutedUICommand("Insert", "Insert", typeof (ChatControl));
		public static readonly RoutedUICommand OpCommand = new RoutedUICommand("Op", "Op", typeof (ChatControl));
		public static readonly RoutedUICommand DeopCommand = new RoutedUICommand("Deop", "Deop", typeof (ChatControl));
		public static readonly RoutedUICommand VoiceCommand = new RoutedUICommand("Voice", "Voice", typeof (ChatControl));
		public static readonly RoutedUICommand DevoiceCommand = new RoutedUICommand("Devoice", "Devoice", typeof (ChatControl));
		public static readonly RoutedUICommand KickCommand = new RoutedUICommand("Kick", "Kick", typeof (ChatControl));
		public static readonly RoutedUICommand BanCommand = new RoutedUICommand("Ban", "Ban", typeof (ChatControl));
		public static readonly RoutedUICommand UnbanCommand = new RoutedUICommand("Unban", "Unban", typeof (ChatControl));
		public static readonly RoutedUICommand SearchCommand = new RoutedUICommand("Search", "Search", typeof (ChatControl));

		public static readonly RoutedUICommand SearchPreviousCommand = new RoutedUICommand("Previous", "SearchPrevious",
			typeof (ChatControl));

		public static readonly RoutedUICommand SearchNextCommand = new RoutedUICommand("Next", "SearchNext",
			typeof (ChatControl));

		public static readonly RoutedUICommand SlapCommand = new RoutedUICommand("Slap!", "Slap", typeof (ChatControl));
		public static readonly RoutedUICommand DccChatCommand = new RoutedUICommand("Chat", "DccXmit", typeof (ChatControl));
		public static readonly RoutedUICommand DccXmitCommand = new RoutedUICommand("Xmit...", "DccXmit", typeof (ChatControl));
		public static readonly RoutedUICommand DccSendCommand = new RoutedUICommand("Send...", "DccSend", typeof (ChatControl));
		public static readonly RoutedUICommand JoinCommand = new RoutedUICommand("Join", "Join", typeof (ChatWindow));

		public static readonly RoutedUICommand ChannelPanelCommand = new RoutedUICommand("Channel Pane", "ChannelPane",
			typeof (ChatControl));

		public static readonly RoutedUICommand ListCommand = new RoutedUICommand("List", "List", typeof (ChatControl));

		private void CanExecuteConnectedCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = IsConnected;
		}

		private void CanExecuteChannelCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = IsConnected && IsChannel;
		}

		private void Insert(string s)
		{
			if (!string.IsNullOrEmpty(txtInput.SelectedText))
			{
				var pos = txtInput.CaretIndex;
				txtInput.SelectedText = s;
				txtInput.CaretIndex = pos;
			}
			else
			{
				var pos = txtInput.CaretIndex;
				txtInput.Text = txtInput.Text.Insert(txtInput.CaretIndex, s);
				txtInput.CaretIndex = pos + s.Length;
			}
		}

		private void ExecuteInsert(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				Insert(s);
			}
		}

		private void CanExecuteIsOp(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!IsChannel ||
				!_nickList.Contains(Session.Nickname))
			{
				e.CanExecute = false;
				return;
			}
			var nick = _nickList[Session.Nickname];
			e.CanExecute = nick != null && (nick.Level & ChannelLevel.Op) > 0;
		}

		private void CanExecuteIsHalfOp(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!IsChannel ||
				!_nickList.Contains(Session.Nickname))
			{
				e.CanExecute = false;
				return;
			}
			var nick = _nickList[Session.Nickname];
			e.CanExecute = nick != null && (nick.Level & (ChannelLevel.Op | ChannelLevel.HalfOp)) > 0;
		}

		private void ExecuteWhois(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				Session.WhoIs(s);
			}
		}

		private void ExecuteOpenLink(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				App.BrowseTo(s);
			}
		}

		private void ExecuteCopyLink(object sender, ExecutedRoutedEventArgs e)
		{
			var s = e.Parameter as string;
			if (!string.IsNullOrEmpty(s))
			{
				Clipboard.SetText(s);
			}
		}

		private void ExecuteQuit(object sender, RoutedEventArgs e)
		{
			try
			{
				Session.AutoReconnect = false;
				Session.Quit("Leaving");
			}
			catch
			{
			}
		}

		private void ExecuteClear(object sender, RoutedEventArgs e)
		{
			boxOutput.Clear();
		}

		private void ExecuteOp(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteOpVoice(e, 'o', true);
		}

		private void ExecuteDeop(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteOpVoice(e, 'o', false);
		}

		private void ExecuteVoice(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteOpVoice(e, 'v', true);
		}

		private void ExecuteDevoice(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteOpVoice(e, 'v', false);
		}

		private void ExecuteOpVoice(ExecutedRoutedEventArgs e, char mode, bool set)
		{
			IEnumerable<string> nicks;
			if (e.Parameter is IList)
			{
				nicks = ((IList) e.Parameter).OfType<NicknameItem>().Select(i => i.Nickname);
			}
			else
			{
				nicks = new[] {e.Parameter.ToString()};
			}

			Session.Mode(Target.Name,
				from nick in nicks
				select new IrcChannelMode(set, mode, nick));
		}

		private void ExecuteKick(object sender, ExecutedRoutedEventArgs e)
		{
			IEnumerable<string> nicks;
			if (e.Parameter is IList)
			{
				nicks = ((IList) e.Parameter).OfType<NicknameItem>().Select(i => i.Nickname);
			}
			else
			{
				nicks = new[] {e.Parameter.ToString()};
			}

			foreach (var nick in nicks)
			{
				Session.Kick(Target.Name, nick);
			}
		}

		private void ExecuteBan(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteBanOrUnban(e, true);
		}

		private void ExecuteUnban(object sender, ExecutedRoutedEventArgs e)
		{
			ExecuteBanOrUnban(e, false);
		}

		private void ExecuteBanOrUnban(ExecutedRoutedEventArgs e, bool banSet)
		{
			string[] nicks;
			if (e.Parameter is IList)
			{
				nicks = ((IList) e.Parameter).OfType<NicknameItem>().Select(i => i.Nickname).ToArray();
			}
			else
			{
				nicks = new[] {e.Parameter.ToString()};
			}

			for (var i = 0; i < nicks.Length; i += 3)
			{
				Session.AddHandler(new IrcCodeHandler(ee =>
				{
					if (ee.Message.Parameters.Count > 1)
					{
						var modes = from user in ee.Message.Parameters[1].Split(' ')
							let parts = user.Split('@')
							where parts.Length == 2
							select new IrcChannelMode(banSet, 'b', "*!*@" + parts[1]);
						Session.Mode(Target.Name, modes);
					}
					return true;
				}, IrcCode.RplUserHost));
				var chunk = nicks.Skip(i).Take(3).ToArray();
				Session.UserHost(chunk);
			}
		}

		private void ExecuteDccChat(object sender, ExecutedRoutedEventArgs e)
		{
			App.ChatWindow.DccChat(Session, new IrcTarget((string) e.Parameter));
		}

		private void ExecuteDccXmit(object sender, ExecutedRoutedEventArgs e)
		{
			var fileName = App.OpenFileDialog(_window, App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(fileName))
			{
				App.ChatWindow.DccXmit(Session, new IrcTarget((string) e.Parameter), new FileInfo(fileName));
			}
		}

		private void ExecuteDccSend(object sender, ExecutedRoutedEventArgs e)
		{
			var fileName = App.OpenFileDialog(_window, App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(fileName))
			{
				App.ChatWindow.DccSend(Session, new IrcTarget((string) e.Parameter), new FileInfo(fileName));
			}
		}

		private void ExecuteSearch(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleSearch();
		}

		private void ExecuteChannelPanel(object sender, ExecutedRoutedEventArgs e)
		{
			ToggleChannelPanel();
		}

		private void ExecuteSearchPrevious(object sender, ExecutedRoutedEventArgs e)
		{
			DoSearch(SearchDirection.Previous);
		}

		private void ExecuteSearchNext(object sender, ExecutedRoutedEventArgs e)
		{
			DoSearch(SearchDirection.Next);
		}

		private void ExecuteList(object sender, ExecutedRoutedEventArgs e)
		{
			Session.List();
		}

		private void ExecuteJoin(object sender, ExecutedRoutedEventArgs e)
		{
			var channel = e.Parameter as string;
			if (!string.IsNullOrEmpty(channel))
			{
				Session.Join(channel);
			}
		}

		private void Execute(string text, bool literal)
		{
			var chars = text.ToCharArray();
			for (var i = 0; i < chars.Length; i++)
			{
				if (chars[i] >= 0x2500 &&
					chars[i] <= 0x2520)
				{
					chars[i] = (char) (chars[i] - 0x2500);
				}
			}
			text = new string(chars);

			var command = text.Trim();

			if (command.Length > 0 &&
				command[0] == CommandChar &&
				!literal)
			{
				var args = string.Empty;
				command = command.Substring(1).TrimStart();
				var spaceIdx = command.IndexOf(' ');
				if (spaceIdx > 0)
				{
					args = command.Substring(spaceIdx + 1);
					command = command.Substring(0, spaceIdx);
				}
				if (command.Length > 0)
				{
					try
					{
						Execute(command.ToUpperInvariant(), args);
					}
					catch (CommandException ex)
					{
						Write("Error", ex.Message);
					}
				}
			}
			else
			{
				if (text.Trim().Length > 0 && IsConnected)
				{
					if (Type == ChatPageType.Chat)
					{
						Session.PrivateMessage(Target, text);
						Write("Own", 0, GetNickWithLevel(Session.Nickname), text, false);
					}
					else if (Type == ChatPageType.DccChat)
					{
						_dcc.QueueMessage(text);
						Write("Own", 0, Session.Nickname, text, false);
					}
					else
					{
						Write("Error", "Can't talk in this window.");
					}
				}
				else
				{
					App.DoEvent("beep");
				}
			}
		}

		private void Execute(string command, string arguments)
		{
			switch (command)
			{
				case IrcCommands.Quote:
					ExecuteQuoteCommand(command, arguments);
					break;
				case IrcCommands.Quit:
					ExecuteQuitCommand(command, arguments);
					break;
				case IrcCommands.Nick:
					ExecuteNickCommand(command, arguments);
					break;
				case IrcCommands.Notice:
					ExecuteNoticeCommand(command, arguments);
					break;
				case IrcCommands.Join:
				case IrcCommands.J:
					ExecuteJoinCommand(command, arguments);
					break;
				case IrcCommands.Part:
				case IrcCommands.Leave:
					ExecutePartLeaveCommands(command, arguments);
					break;
				case IrcCommands.Topic:
					ExecuteTopicCommand(command, arguments);
					break;
				case IrcCommands.Invite:
					ExecuteInviteCommand(command, arguments);
					break;
				case IrcCommands.Kick:
					ExecuteKickCommand(command, arguments);
					break;
				case IrcCommands.Motd:
					ExecuteMotdCommand(command, arguments);
					break;
				case IrcCommands.Who:
					ExecuteWhoCommand(command, arguments);
					break;
				case IrcCommands.Whois:
					ExecuteWhoisCommand(command, arguments);
					break;
				case IrcCommands.Whowas:
					ExecuteWhoWasCommand(command, arguments);
					break;
				case IrcCommands.Away:
					ExecuteAwayCommand(command, arguments);
					break;
				case IrcCommands.Userhost:
					ExecuteUserHostCommand(command, arguments);
					break;
				case IrcCommands.Mode:
					ExecuteModeCommand(command, arguments);
					break;
				case IrcCommands.Server:
					ExecuteServerCommand(command, arguments);
					break;
				case IrcCommands.Me:
				case IrcCommands.Action:
					ExecuteActionCommand(command, arguments);
					break;
				case IrcCommands.Setup:
					App.ShowSettings();
					break;
				case IrcCommands.Clear:
					boxOutput.Clear();
					break;
				case IrcCommands.Msg:
					ExecuteMsgCommand(command, arguments);
					break;
				case IrcCommands.List:
					ExecuteListCommand(command, arguments);
					break;
				case IrcCommands.Op:
				case IrcCommands.Deop:
				case IrcCommands.Voice:
				case IrcCommands.Devoice:
					ExecuteOpVoiceCommands(command, arguments);
					break;
				case IrcCommands.Help:
					ExecuteHelpCommand();
					break;
				case IrcCommands.Ctcp:
					ExecuteCtcpCommand(command, arguments);
					break;
				case IrcCommands.Query:
					ExecuteQueryCommand(command, arguments);
					break;
				case IrcCommands.Ban:
					ExecuteBanCommand(command, arguments);
					break;
				case IrcCommands.Unban:
					ExecuteUnbanCommand(command, arguments);
					break;
				case IrcCommands.Ignore:
					ExecuteIgnoreCommand(command, arguments);
					break;
				case IrcCommands.Unignore:
					ExecuteUnignoreCommand(command, arguments);
					break;
				case IrcCommands.Dcc:
					ExecuteDccCommand(command, arguments);
					break;
				default:
					Write("Error", string.Format("Unrecognized command: {0}", command));
					break;
			}
		}

		private void ExecuteUnbanCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			UnbanCommand.Execute(args[0], this);
		}

		private void ExecuteWhoCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			Session.Who(args[0]);
		}

		private void ExecuteQuoteCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			Session.Quote(args[0]);
		}

		private void ExecuteQuitCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 0, 1);
			Session.AutoReconnect = false;
			Session.Quit(args.Length == 0 ? "Leaving" : args[0]);
		}

		private void ExecuteNickCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			Session.Nick(args[0]);
		}

		private void ExecuteNoticeCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 2, 2);
			Session.Notice(new IrcTarget(args[0]), args[1]);
		}

		private void ExecuteJoinCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 2);
			if (args.Length == 2)
			{
				Session.Join(args[0], args[1]);
			}
			else
			{
				Session.Join(args[0]);
			}
		}

		private void ExecutePartLeaveCommands(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1, true);
			Session.Part(args[0]);
		}

		private void ExecuteTopicCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 2, true);
			if (args.Length > 1)
			{
				Session.Topic(args[0], args[1]);
			}
			else
			{
				Session.Topic(args[0]);
			}
		}

		private void ExecuteInviteCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 2, 2);
			Session.Invite(args[1], args[0]);
		}

		private void ExecuteKickCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 2, 3, true);
			if (args.Length > 2)
			{
				Session.Kick(args[0], args[1], args[2]);
			}
			else
			{
				Session.Kick(args[0], args[1]);
			}
		}

		private void ExecuteMotdCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 0, 1);
			if (args.Length > 0)
			{
				Session.Motd(args[0]);
			}
			else
			{
				Session.Motd();
			}
		}

		private void ExecuteWhoisCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 2);
			if (args != null)
			{
				if (args.Length == 2)
				{
					Session.WhoIs(args[0], args[1]);
				}
				else if (args.Length == 1)
				{
					Session.WhoIs(args[0]);
				}
			}
		}

		private void ExecuteWhoWasCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			Session.WhoWas(args[0]);
		}

		private void ExecuteAwayCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 0, 1);
			if (args.Length > 0)
			{
				Session.Away(args[0]);
			}
			else
			{
				Session.UnAway();
			}
		}

		private void ExecuteUserHostCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, int.MaxValue);
			Session.UserHost(args);
			return;
		}

		private void ExecuteModeCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 2);
			var target = new IrcTarget(args[0]);
			if (!target.IsChannel)
			{
				if (!Session.IsSelf(target))
				{
					throw new CommandException("Can't change modes for another user.");
				}
				if (args.Length > 1)
				{
					Session.Mode(args[1]);
				}
				else
				{
					Session.Mode("");
				}
			}
			else
			{
				if (args.Length > 1)
				{
					Session.Mode(target.Name, args[1]);
				}
				else
				{
					Session.Mode(target.Name, "");
				}
			}
		}

		private void ExecuteMsgCommand(string command, string arguments)
		{
			if (IsConnected)
			{
				var args = Split(command, arguments, 2, 2);
				Session.PrivateMessage(new IrcTarget(args[0]), args[1]);
				Write("Own", string.Format("-> [{0}] {1}", args[0], args[1]));
			}
		}

		private void ExecuteServerCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 3);
			var port = 0;
			var useSsl = false;
			if (args.Length > 1 &&
				(args[1] = args[1].Trim()).Length > 0)
			{
				if (args[1][0] == '+')
				{
					useSsl = true;
				}
				int.TryParse(args[1], out port);
			}
			string password = null;
			if (args.Length > 2)
			{
				password = args[2];
			}
			if (port == 0)
			{
				port = 6667;
			}
			if (IsConnected)
			{
				Session.AutoReconnect = false;
				Session.Quit("Changing servers");
			}
			Perform = "";
			Connect(args[0], port, useSsl, false, password);
		}

		private void ExecuteActionCommand(string command, string arguments)
		{
			if (IsServer)
			{
				Write("Error", "Can't talk in this window.");
			}
			if (IsConnected)
			{
				var args = Split(command, arguments, 1, int.MaxValue);
				Write("Own", string.Format("{0} {1}", Session.Nickname, string.Join(" ", args)));
				if (Type == ChatPageType.Chat)
				{
					Session.SendCtcp(Target, new CtcpCommand(IrcCommands.Action, args), false);
				}
				else if (Type == ChatPageType.DccChat)
				{
					_dcc.QueueMessage(string.Format("\u0001ACTION {0}\u0001", string.Join(" ", args)));
				}
			}
		}

		private void ExecuteListCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 0, 2);
			if (args.Length > 1)
			{
				Session.List(args[0], args[1]);
			}
			else if (args.Length > 0)
			{
				Session.List(args[0]);
			}
			else
			{
				Session.List();
			}
		}

		private void ExecuteOpVoiceCommands(string command, string arguments)
		{
			if (!IsChannel)
			{
				Write("Error", "Cannot perform that action in this window.");
			}
			else
			{
				var mode = (command == IrcCommands.Op || command == IrcCommands.Deop) ? 'o' : 'v';
				var args = Split(command, arguments, 1, int.MaxValue);
				var modes = from s in args
							select new IrcChannelMode(command == IrcCommands.Op || 
								command == IrcCommands.Voice, mode, s);
				Session.Mode(Target.Name, modes);
			}
		}

		private void ExecuteHelpCommand()
		{
			foreach (var s in App.HelpText.Split(Environment.NewLine.ToCharArray()))
			{
				if (s.Length > 0)
				{
					Write("Client", s);
				}
			}
		}

		private void ExecuteCtcpCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 2, int.MaxValue);
			Session.SendCtcp(new IrcTarget(args[0]),
				new CtcpCommand(args[1], args.Skip(2).ToArray()), false);
		}

		private void ExecuteQueryCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			ChatWindow.ChatCommand.Execute(args[0], this);
		}

		private void ExecuteBanCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 1);
			BanCommand.Execute(args[0], this);
		}

		private void ExecuteIgnoreCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 0, 2);
			if (args.Length == 0)
			{
				var ignores = App.GetIgnoreInfo();
				if (ignores.Any())
				{
					Write("Own", "Ignore list:");
					foreach (var i in ignores)
					{
						Write("Own", "  " + i);
					}
				}
				else
				{
					Write("Own", "Ignore list is empty.");
				}
				return;
			}

			var mask = args[0];
			var sactions = args.Length > 1 ? args[1] : "All";
			IgnoreActions actions;
			if (!Enum.TryParse(sactions, true, out actions))
			{
				Write("Error", "Invalid ignore action(s).");
				return;
			}

			if (!mask.Contains('!') &&
				!mask.Contains('@'))
			{
				mask = mask + "!*@*";
			}
			App.AddIgnore(mask, actions);
			Write("Own", "Added to ignore list: " + mask);
		}

		private void ExecuteUnignoreCommand(string command, string arguments)
		{
			var args = Split(command, arguments, 1, 2);
			var mask = args[0];

			var sactions = args.Length > 1 ? args[1] : "All";
			IgnoreActions actions;
			if (!Enum.TryParse(sactions, true, out actions))
			{
				Write("Error", "Invalid ignore action(s).");
				return;
			}
			if (!mask.Contains('!') &&
				!mask.Contains('@'))
			{
				mask = mask + "!*@*";
			}
			if (App.RemoveIgnore(mask, actions))
			{
				Write("Own", "Removed from ignore list: " + mask);
			}
			else
			{
				Write("Error", "Specified pattern was not on ignore list.");
			}
		}

		private void ExecuteDccCommand(string command, string arguments)
		{
			{
				if (!IsConnected)
				{
					return;
				}
				var args = Split(command, arguments, 2, 3);
				var dccCmd = args[0].ToUpperInvariant();

				switch (dccCmd)
				{
					case IrcCommands.Chat:
						App.ChatWindow.DccChat(Session, new IrcTarget(args[1]));
						break;
					case IrcCommands.Send:
					case IrcCommands.Xmit:
						string path = null;
						if (args.Length < 3)
						{
							Write("Error", "File name is required.");
							break;
						}
						try
						{
							if (Path.IsPathRooted(args[2]) &&
								File.Exists(args[2]))
							{
								path = args[2];
							}
							else if (!File.Exists(path = Path.Combine(App.Settings.Current.Dcc.DownloadFolder, args[2])))
							{
								Write("Error", "Could not find file " + args[2]);
								break;
							}
						}
						catch (ArgumentException)
						{
							Write("Error", String.Format((string) "Invalid pathname: {0}", (object) args[2]));
							break;
						}
						if (dccCmd == IrcCommands.Xmit)
						{
							App.ChatWindow.DccXmit(Session, new IrcTarget(args[1]), new FileInfo(path));
						}
						else
						{
							App.ChatWindow.DccSend(Session, new IrcTarget(args[1]), new FileInfo(path));
						}
						break;
					default:
						Write("Error", "Unsupported DCC mode " + args[0]);
						break;
				}
			}
		}

		private string[] Split(string command, string args, int minArgs, int maxArgs)
		{
			return Split(command, args, minArgs, maxArgs, false);
		}

		private string[] Split(string command, string args, int minArgs, int maxArgs, bool isChannelRequired)
		{
			var parts = Split(args, maxArgs);
			if (isChannelRequired && (parts.Length < 1 || !IrcTarget.IsChannelName(parts[0])))
			{
				if (!IsChannel)
				{
					throw new CommandException("Not on a channel.");
				}
				parts = new[] {Target.Name}.Union(Split(args, maxArgs - 1)).ToArray();
			}
			if (parts.Length < minArgs)
			{
				throw new CommandException(string.Format("{0} requires {1} parameters.", command, minArgs));
			}
			return parts;
		}

		private static string[] Split(string str, int maxParts)
		{
			if (maxParts == 1)
			{
				str = str.Trim();
				return str.Length > 0 ? new[] {str} : new string[0];
			}

			var parts = new List<string>();
			var part = new StringBuilder();

			for (var i = 0; i < str.Length; i++)
			{
				if (maxParts == 1)
				{
					var remainder = str.Substring(i).Trim();
					if (remainder.Length > 0)
					{
						parts.Add(remainder);
					}
					break;
				}

				if (str[i] == ' ')
				{
					if (part.Length > 0)
					{
						parts.Add(part.ToString());
						part.Length = 0;
						--maxParts;
					}
				}
				else
				{
					part.Append(str[i]);
				}
			}
			if (part.Length > 0)
			{
				parts.Add(part.ToString());
			}

			return parts.ToArray();
		}

		private void ToggleSearch()
		{
			if (pnlSearch.Visibility == Visibility.Visible)
			{
				pnlSearch.Visibility = Visibility.Collapsed;
				boxOutput.ClearSearch();
			}
			else
			{
				pnlSearch.Visibility = Visibility.Visible;
				txtSearchTerm.Focus();
				txtSearchTerm.SelectAll();
			}
		}

		private void ToggleChannelPanel()
		{
			if (pnlChannel.Visibility == Visibility.Visible)
			{
				pnlChannel.Visibility = Visibility.Collapsed;
			}
			else
			{
				pnlChannel.Visibility = Visibility.Visible;
				txtChannel.Focus();
				if (txtChannel.Text.Length < 1 ||
					txtChannel.Text == "#")
				{
					txtChannel.Text = "#";
					txtChannel.CaretIndex = 1;
				}
				else
				{
					txtChannel.SelectAll();
				}
			}
		}

		private void DoSearch(SearchDirection dir)
		{
			Regex pattern = null;

			try
			{
				pattern = new Regex(
					chkUseRegEx.IsChecked.Value ? txtSearchTerm.Text : Regex.Escape(txtSearchTerm.Text),
					chkMatchCase.IsChecked.Value ? RegexOptions.None : RegexOptions.IgnoreCase);
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show("The regular expression was not valid: " + Environment.NewLine + ex.Message,
					"Invalid pattern", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			boxOutput.Search(pattern, dir);
		}
	}
}