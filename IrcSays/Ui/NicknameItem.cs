using System;
using System.Linq;
using System.Windows;

namespace IrcSays.Ui
{
	public class NicknameItem : DependencyObject, IComparable<NicknameItem>, IComparable
	{
		private static readonly char[] _nickSpecialChars = {'[', ']', '\\', '`', '_', '^', '{', '|', '}', '-'};
		private string _nickname;

		public static bool IsNickChar(char c)
		{
			return char.IsLetterOrDigit(c) || _nickSpecialChars.Contains(c);
		}

		public NicknameItem(ChannelLevel level, string nick)
		{
			Nickname = nick;
			Level = level;
		}

		public NicknameItem(string nick)
		{
			var level = ChannelLevel.Normal;
			var i = 0;
			for (; i < nick.Length && !IsNickChar(nick[i]); i++)
			{
				switch (nick[0])
				{
					case '@':
						level |= ChannelLevel.Op;
						break;
					case '%':
						level |= ChannelLevel.HalfOp;
						break;
					case '+':
						level |= ChannelLevel.Voice;
						break;
				}
			}
			if (i < nick.Length)
			{
				nick = nick.Substring(i);
			}
			else
			{
				nick = "";
			}
			Nickname = nick;
			Level = level;
		}

		public string Nickname
		{
			get { return _nickname; }
			set
			{
				_nickname = value;
				NickLowerCase = value.ToLowerInvariant();
			}
		}

		public string NickLowerCase { get; private set; }

		public ChannelLevel Level { get; set; }

		public string NickWithLevel
		{
			get { return ToString(); }
		}

		private ChannelLevel HighestLevel
		{
			get
			{
				if ((Level & ChannelLevel.Op) > 0)
				{
					return ChannelLevel.Op;
				}
				if ((Level & ChannelLevel.HalfOp) > 0)
				{
					return ChannelLevel.HalfOp;
				}
				if ((Level & ChannelLevel.Voice) > 0)
				{
					return ChannelLevel.Voice;
				}
				return ChannelLevel.Normal;
			}
		}

		public override string ToString()
		{
			string prefix;
			switch (HighestLevel)
			{
				case ChannelLevel.Op:
					prefix = "@";
					break;
				case ChannelLevel.HalfOp:
					prefix = "%";
					break;
				case ChannelLevel.Voice:
					prefix = "+";
					break;
				default:
					prefix = "";
					break;
			}

			return prefix + Nickname;
		}

		public int CompareTo(NicknameItem other)
		{
			if (other == null)
			{
				return 1;
			}
			if (HighestLevel == other.HighestLevel)
			{
				return string.Compare(Nickname, other.Nickname, StringComparison.InvariantCultureIgnoreCase);
			}
			return (int) other.HighestLevel - (int) HighestLevel;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as NicknameItem);
		}
	}
}