using System;
using System.IO;

namespace IrcSays.Application
{
	public partial class App
	{
		private const string LogsFolder = "Logs";

		public static string LoggingPathBase
		{
			get { return Path.Combine(Settings.BasePath, LogsFolder); }
		}

		public static LogFileHandle OpenLogFile(string name)
		{
			return new LogFileHandle(LoggingPathBase, string.Format("{0}.log", name),
				Settings.Current.Buffer.BufferLines);
		}

		public static void LogUnhandledException(object exceptionObject)
		{
			var path = Path.Combine(LoggingPathBase, "exception.txt");
			using (var sw = new StreamWriter(path))
			{
				sw.WriteLine("-{2}{0}{2}{1}{2}", DateTime.Now, exceptionObject, Environment.NewLine);
				sw.Flush();
			}
		}
	}
}