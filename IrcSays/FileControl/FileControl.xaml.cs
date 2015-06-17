using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using IrcSays.Application;
using IrcSays.Communication;
using IrcSays.Communication.Dcc;
using IrcSays.Communication.Irc;
using IrcSays.Communication.Network;
using IrcSays.Services;
using IrcSays.Ui;
using Microsoft.Win32;
using ErrorEventArgs = IrcSays.Communication.Irc.ErrorEventArgs;

namespace IrcSays.FileControl
{
	public partial class FileControl : ChatPage
	{
		private const int PollTime = 250;
		private const int SpeedUpdateInterval = 4;
		private const int SpeedWindow = 5;

		private FileInfo _fileInfo;
		private DccOperation _dcc;
		private Timer _pollTimer;
		private IPAddress _address;
		private int _port;
		private bool _isPortForwarding;

		public FileControl(IrcSession session, IrcTarget target, DccMethod method)
			: base(ChatPageType.DccFile, session, target, "DCC")
		{
			DccMethod = method;
			InitializeComponent();
			Id = "dcc-file";
			Header = Title = string.Format("{0} [DCC]", target.Name);
		}

		public void StartSend(FileInfo file, Action<int> readyCallback)
		{
			_fileInfo = file;
			Header = string.Format("[SEND] {0}", Target.Name);
			Title = string.Format("{0} - [DCC {1}] Sending file {2}", App.Product, Target.Name, _fileInfo.Name);
			Description = string.Format("Sending {0}...", file.Name);
			FileSize = file.Length;
			Status = FileStatus.Working;

			switch (DccMethod)
			{
				case DccMethod.Send:
					_dcc = new DccSendSender(_fileInfo);
					break;
				case DccMethod.Xmit:
					_dcc = new DccXmitSender(_fileInfo);
					break;
			}
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
			try
			{
				_port = _dcc.Listen(App.Settings.Current.Dcc.LowPort, App.Settings.Current.Dcc.HighPort);
			}
			catch (InvalidOperationException)
			{
				Status = FileStatus.Cancelled;
				StatusText = "Error: No ports available";
				_port = 0;
			}

			if (App.Settings.Current.Dcc.EnableUpnp &&
				NatHelper.IsAvailable)
			{
				StatusText = "Forwarding port";
				NatHelper.BeginAddForwardingRule(_port, ProtocolType.Tcp, "IrcSays DCC", o =>
				{
					Dispatcher.BeginInvoke((Action) (() =>
					{
						StatusText = "Listening for connection";
						readyCallback(_port);
					}));
				});
				_isPortForwarding = true;
			}
			else
			{
				StatusText = "Listening for connection";
				readyCallback(_port);
			}
		}

		public void StartReceive(IPAddress address, int port, string name, long size)
		{
			_fileInfo = new FileInfo(Path.Combine(App.Settings.Current.Dcc.DownloadFolder, name));
			Header = string.Format("[RECV] {0}", Target.Name);
			Title = string.Format("{0} - [DCC {1}] Receiving file {2}", App.Product, Target.Name, _fileInfo.Name);
			_address = address;
			_port = port;
			Description = string.Format("Receiving {0}...", name);
			FileSize = size;
			StatusText = "Waiting for confirmation";
			Status = FileStatus.Asking;

			CheckFileExtension(_fileInfo.Extension.StartsWith(".", StringComparison.Ordinal) && _fileInfo.Extension.Length > 1
				? _fileInfo.Extension.Substring(1)
				: _fileInfo.Extension);

			if (App.Settings.Current.Dcc.AutoAccept)
			{
				Accept(false);
			}
			ServiceManager.Sound.PlaySound("dccRequest");
		}

		public override bool CanClose()
		{
			if (Status == FileStatus.Working)
			{
				return App.Confirm(Window.GetWindow(this), "Are you sure you want to cancel this transfer in progress?",
					"Confirm Close");
			}
			if (Status == FileStatus.Asking)
			{
				Decline();
			}
			return true;
		}

		public override void Dispose()
		{
			base.Dispose();

			if (_dcc != null)
			{
				_dcc.Dispose();
			}
		}

		private void CheckFileExtension(string ext)
		{
			var extensions = App.Settings.Current.Dcc.DangerExtensions.ToUpperInvariant().Split(' ');
			IsDangerous = extensions.Contains(ext.ToUpperInvariant());
		}

		private void Accept(bool forceOverwrite)
		{
			Status = FileStatus.Working;
			StatusText = "Connecting";
			switch (DccMethod)
			{
				case DccMethod.Send:
					_dcc = new DccSendReceiver(_fileInfo) {ForceOverwrite = forceOverwrite};
					break;
				case DccMethod.Xmit:
					_dcc = new DccXmitReceiver(_fileInfo)
					{
						ForceOverwrite = forceOverwrite,
						ForceResume = chkForceResume.IsChecked == true
					};
					break;
			}
			_dcc.Connect(_address, _port);
			_dcc.Connected += dcc_Connected;
			_dcc.Disconnected += dcc_Disconnected;
			_dcc.Error += dcc_Error;
		}

		private void Decline()
		{
			Status = FileStatus.Cancelled;
			StatusText = "Declined";
			Session.SendCtcp(Target, new CtcpCommand("ERRMSG", "DCC", "XMIT", "declined"), true);
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
			StatusText = "Transferring";
			var iteration = SpeedUpdateInterval;
			var stats = new Queue<Tuple<long, long>>(SpeedWindow + 1);
			stats.Enqueue(new Tuple<long, long>(DateTime.UtcNow.Ticks, 0));
			_pollTimer = new Timer(o => Dispatcher.BeginInvoke((Action) (() =>
			{
				BytesTransferred = _dcc.BytesTransferred;
				if (--iteration == 0)
				{
					iteration = SpeedUpdateInterval;
					var now = DateTime.UtcNow.Ticks;
					stats.Enqueue(new Tuple<long, long>(now, BytesTransferred));
					while (stats.Count > SpeedWindow)
					{
						stats.Dequeue();
					}
					if (stats.Count > 1)
					{
						var timeDiff = (now - stats.Peek().Item1) / 10000;
						var newBytes = BytesTransferred - stats.Peek().Item2;
						if (timeDiff / 1000 > 0)
						{
							Speed = newBytes / (timeDiff / 1000);
						}
						if (Speed > 0)
						{
							EstimatedTime = (int) (FileSize / Speed);
						}
						if (FileSize > 0)
						{
							Progress = (double) BytesTransferred / (double) FileSize;
						}
					}
				}
			})), null, PollTime, PollTime);
		}

		private void dcc_Disconnected(object sender, EventArgs e)
		{
			_pollTimer.Dispose();
			BytesTransferred = _dcc.BytesTransferred;
			Speed = 0;
			EstimatedTime = 0;
			Progress = 1f;

			if (BytesTransferred < FileSize)
			{
				if (Status != FileStatus.Cancelled)
				{
					Status = FileStatus.Cancelled;
					StatusText = "Connection lost";
					ServiceManager.Sound.PlaySound("dccError");
				}
			}
			else
			{
				Status = (_dcc is DccXmitReceiver || _dcc is DccSendReceiver) ? FileStatus.Received : FileStatus.Sent;
				StatusText = "Finished";
				ServiceManager.Sound.PlaySound("dccComplete");
			}
			DeletePortForwarding();
		}

		private void dcc_Error(object sender, ErrorEventArgs e)
		{
			Status = FileStatus.Cancelled;
			StatusText = "Error: " + e.Exception.Message;
			if (_pollTimer != null)
			{
				_pollTimer.Dispose();
			}
			DeletePortForwarding();
			ServiceManager.Sound.PlaySound("dccError");
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Status = FileStatus.Cancelled;
			StatusText = "Cancelled";
			if (_dcc != null)
			{
				_dcc.Dispose();
			}
		}

		private void btnOpen_Click(object sender, RoutedEventArgs e)
		{
			var path = "";
			var receiver = _dcc as DccXmitReceiver;
			if (receiver != null)
			{
				path = receiver.FileSavedAs;
			}
			else
			{
				path = ((DccSendReceiver) _dcc).FileSavedAs;
			}
			if (File.Exists(path))
			{
				App.BrowseTo(path);
			}
		}

		private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
		{
			if (_fileInfo != null)
			{
				App.BrowseTo(_fileInfo.DirectoryName);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			Accept(false);
		}

		private void btnSaveAs_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				InitialDirectory = App.Settings.Current.Dcc.DownloadFolder,
				FileName = _fileInfo.Name
			};
			if (dialog.ShowDialog(Window.GetWindow(this)) == true)
			{
				_fileInfo = new FileInfo(dialog.FileName);
				Description = string.Format("Receiving {0}...", dialog.SafeFileName);
				Accept(true);
			}
		}

		private void btnDecline_Click(object sender, RoutedEventArgs e)
		{
			Decline();
		}
	}
}