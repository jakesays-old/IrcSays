using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using IrcSays.Configuration;

namespace IrcSays.Application
{
	public static class AppInfo
	{
		private static string _helpText;

		public static string Product
		{
			get { return "IrcSays"; }
		}

		public static string HelpText
		{
			get
			{
				if (_helpText == null)
				{
					using (var sr = new StreamReader(typeof(AppInfo).Assembly.GetManifestResourceStream(
						"IrcSays.Resources.Help.txt")))
					{
						_helpText = sr.ReadToEnd();
					}
				}
				return _helpText;
			}
		}

		public static string Version
		{
			get { return typeof (App).Assembly.GetName().Version.ToString(); }
		}
	}
}