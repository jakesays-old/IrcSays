using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using IrcSays.Communication.Network;
using IrcSays.Configuration;
using IrcSays.Services;
using IrcSays.Ui;

namespace IrcSays.Application
{
	public partial class App
	{
		public static PersistentSettings Settings { get; private set; }

		static App()
		{
			try
			{
				Settings = new PersistentSettings(AppInfo.Product);
			}
			catch (ConfigurationException ex)
			{
				MessageBox.Show(
					string.Format(
						"Unable to load user configuration. You may want to delete the configuration file and try again. {0}",
						ex.Message));
				Environment.Exit(-1);
			}

			RefreshAttentionPatterns();
			LoadIgnoreMasks();
		}

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