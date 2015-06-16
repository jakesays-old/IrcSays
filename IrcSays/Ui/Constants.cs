using System.Text.RegularExpressions;

namespace IrcSays.Ui
{
	public static class Constants
	{
		private static readonly Regex _urlRegex = new Regex(@"(www\.|(http|https|ftp)+\:\/\/)[^\s]+", RegexOptions.IgnoreCase);

		public static Regex UrlRegex { get { return _urlRegex; } }
	}
}
