using System;
using System.Windows;
using IrcSays.Communication.Network;
using IrcSays.Services;
using IrcSays.Ui;

namespace IrcSays.Application
{
	public partial class App
	{
		public App()
		{
//			AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogUnhandledException(e.ExceptionObject);

			NatHelper.BeginDiscover(ar => NatHelper.EndDiscover(ar));
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ServiceManager.Initialize();

			var window = new ChatWindow();
			window.Closed += window_Closed;
			window.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Settings.Save();
		}

		private void window_Closed(object sender, EventArgs e)
		{
			if (Windows.Count == 0)
			{
				Shutdown();
			}
		}
	}
}