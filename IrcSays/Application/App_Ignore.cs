using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IrcSays.Communication.Irc;

namespace IrcSays.Application
{
	public partial class App : System.Windows.Application
	{
		private class IgnoreInfo
		{
			private Regex _pattern;

			public string Mask { get; private set; }
			public IgnoreActions Actions { get; set; }

			public IgnoreInfo(string mask)
			{
				Mask = mask;
			}

			public bool IsMatch(string prefix, IgnoreActions action)
			{
				if (_pattern == null)
				{
					_pattern = new Regex("^" + Regex.Escape(Mask).Replace("\\*", ".*").Replace("\\?", ".") + "$",
						RegexOptions.IgnoreCase);
				}
				return (Actions & action) != IgnoreActions.None && _pattern.IsMatch(prefix);
			}
		}

		private static readonly Dictionary<string, IgnoreInfo> _ignores = new Dictionary<string, IgnoreInfo>();

		public static IEnumerable<string> GetIgnoreInfo()
		{
			foreach (var i in _ignores.Values)
			{
				yield return string.Format("{0} {1}", i.Mask, i.Actions);
			}
		}

		public static void AddIgnore(string mask, IgnoreActions actions)
		{
			var key = mask.ToUpperInvariant();
			if (!_ignores.ContainsKey(key))
			{
				_ignores.Add(key, new IgnoreInfo(mask));
			}
			_ignores[key].Actions |= actions;
			SaveIgnores();
		}

		public static bool RemoveIgnore(string mask, IgnoreActions actions)
		{
			var key = mask.ToUpperInvariant();
			if (!_ignores.ContainsKey(key) ||
				(_ignores[key].Actions & actions) == 0)
			{
				return false;
			}

			_ignores[key].Actions &= ~actions;
			if (_ignores[key].Actions == IgnoreActions.None)
			{
				_ignores.Remove(key);
			}
			SaveIgnores();
			return true;
		}

		public static bool IsIgnoreMatch(IrcPrefix prefix, IgnoreActions action)
		{
			switch (action)
			{
				case IgnoreActions.Join:
				case IgnoreActions.Part:
				case IgnoreActions.Quit:
					return true;
			}

			if (prefix == null)
			{
				return false;
			}

			return (from ignore in _ignores.Values
				where ignore.IsMatch(prefix.Prefix, action)
				select true).Any();
		}

		private static void LoadIgnoreMasks()
		{
			_ignores.Clear();
			foreach (var i in Settings.Current.Ignore.Split(','))
			{
				var parts = i.Split(' ');
				if (i.Trim().Length < 1)
				{
					continue;
				}
				var mask = parts[0];
				int iactions;
				if (!(parts.Length > 1 && int.TryParse(parts[1], out iactions)))
				{
					iactions = (int) IgnoreActions.All;
				}
				_ignores.Add(mask.ToUpperInvariant(), new IgnoreInfo(mask) {Actions = (IgnoreActions) iactions});
			}
		}

		private static void SaveIgnores()
		{
			Settings.Current.Ignore = string.Join(",",
				from i in _ignores.Values
				select string.Format("{0} {1}", i.Mask, (int) i.Actions));
		}
	}
}