using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using IrcSays.Application;
using IrcSays.Communication;
using IrcSays.Communication.Dcc;
using IrcSays.Communication.Irc;
using IrcSays.Communication.Network;

namespace IrcSays.Ui
{
	public partial class ChatControl
	{
		private DccChat _dcc;
		private IPAddress _address;
		private int _port;
		private bool _isPortForwarding;

		public void StartListen(Action<int> readyCallback)
		{
			_dcc = new DccChat();
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			_dcc.MessageReceived += dcc_MessageReceived;

			try
			{
				_port = _dcc.Listen(App.Settings.Current.Dcc.LowPort, App.Settings.Current.Dcc.HighPort);
			}
			catch (InvalidOperationException)
			{
				Write("Error", "No available ports.");
				_port = 0;
			}

			if (App.Settings.Current.Dcc.EnableUpnp && NatHelper.IsAvailable)
			{
				Write("Client", "Forwarding port...");
				NatHelper.BeginAddForwardingRule(_port, ProtocolType.Tcp, "IrcSays DCC", 
					o => Dispatcher.BeginInvoke((Action)(() =>
				{
					Write("Client", "Waiting for connection...");
					readyCallback(_port);
				})));
				_isPortForwarding = true;
			}
			else
			{
				Write("Client", "Waiting for connection...");
				readyCallback(_port);
			}
		}

		public void StartAccept(IPAddress address, int port)
		{
			_address = address;
			_port = port;
			if (App.Settings.Current.Dcc.AutoAccept)
			{
				AcceptChat();
			}
			else
			{
				lblDccChat.Content = string.Format("Do you want DCC chat with {0}?", Target.Name);
				pnlDccChat.Visibility = Visibility.Visible;
			}
			App.DoEvent("dccRequest");
		}

		private void DeletePortForwarding()
		{
			if (_isPortForwarding)
			{
				NatHelper.BeginDeleteForwardingRule(_port, ProtocolType.Tcp, ar => NatHelper.EndDeleteForwardingRule(ar));
				_isPortForwarding = false;
			}
		}

		private void dcc_Connected(object sender, EventArgs e)
		{
			Write("Client", "Connected");
			IsConnected = true;
		}

		private void dcc_Disconnected(object sender, EventArgs e)
		{
			Write("Error", "Disconnected");
			IsConnected = false;
			DeletePortForwarding();
		}

		private void dcc_Error(object sender, ErrorEventArgs e)
		{
			Write("Error", e.Exception.Message);
			IsConnected = false;
			DeletePortForwarding();
		}

		private void dcc_MessageReceived(object sender, DccChatEventArgs e)
		{
			string text = e.Text;
			if (text.StartsWith("\u0001ACTION ") && text.EndsWith("\u0001") &&
				text.Length > 9)
			{
				text = text.Substring(8, text.Length - 9);
				Write("Default", string.Format("{0} {1}", Target.Name, text));
			}
			else
			{
				Write("Default", 0, Target.Name, e.Text, false);
			}
		}

		private void AcceptChat()
		{
			_dcc = new DccChat();
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			_dcc.MessageReceived += dcc_MessageReceived;
			Write("Client", "Connecting...");
			_dcc.Connect(_address, _port);
		}

		private void btnAccept_Click(object sender, RoutedEventArgs e)
		{
			pnlDccChat.Visibility = Visibility.Collapsed;
			AcceptChat();
		}

		private void btnDecline_Click(object sender, RoutedEventArgs e)
		{
			Session.SendCtcp(Target, new CtcpCommand("ERRMSG", "DCC", "CHAT", "declined"), true);
			App.ClosePage(this);
		}
	}
}
