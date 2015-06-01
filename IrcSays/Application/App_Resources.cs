using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IrcSays.Configuration;

namespace IrcSays.Application
{
	public partial class App
	{
		public static PersistentSettings Settings { get; private set; }
		public static string Product { get; private set; }
		public static string HelpText { get; private set; }

		private static readonly Lazy<ImageSource> _appImage = new Lazy<ImageSource>(() =>
		{
			using (var stream = typeof (App).Assembly.GetManifestResourceStream(
				"IrcSays.Resources.App.ico"))
			{
				return BitmapFrame.Create(stream);
			}
		});

		public static ImageSource ApplicationImage
		{
			get { return _appImage.Value; }
		}

		private static readonly Lazy<Icon> _appIcon = new Lazy<Icon>(() =>
		{
			using (var stream = typeof (App).Assembly.GetManifestResourceStream(
				"IrcSays.Resources.App.ico"))
			{
				return new Icon(stream);
			}
		});

		public static Icon ApplicationIcon
		{
			get { return _appIcon.Value; }
		}

		public static string Version
		{
			get { return typeof (App).Assembly.GetName().Version.ToString(); }
		}

		static App()
		{
			Product = typeof (App).Assembly.GetCustomAttributes(
				typeof (AssemblyProductAttribute), false).OfType<AssemblyProductAttribute>().FirstOrDefault().Product;

			try
			{
				Settings = new PersistentSettings(Product);
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

			var rsrcs = typeof (App).Assembly.GetManifestResourceNames();
			using (var sr = new StreamReader(typeof (App).Assembly.GetManifestResourceStream(
				"IrcSays.Resources.Help.txt")))
			{
				HelpText = sr.ReadToEnd();
			}
		}
	}
}