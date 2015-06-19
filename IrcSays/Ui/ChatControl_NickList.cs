using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IrcSays.Ui
{
	public partial class ChatControl : ChatPage
	{
		private string[] _nickCandidates;
		private readonly NicknameList _nickList;
		private readonly List<string> _mostRecentTalkers = new List<string>();
		private readonly LinkedList<string> _mostRecentlyReferenced = new LinkedList<string>();
		private string _lastReferencedNick;

		public NicknameList Nicknames
		{
			get { return _nickList; }
		}

		private string GetNickWithLevel(string nick)
		{
			return IsChannel && _nickList.Contains(nick) ? _nickList[nick].ToString() : nick;
		}

		private string GetNickWithoutLevel(string nick)
		{
			return (nick.Length > 1 && (nick[0] == '@' || nick[0] == '+' || nick[0] == '%')) ? nick.Substring(1) : nick;
		}

		private int _nickListPosition = 0;
		private int _currentNickStart = 0;
		private int _currentNickEnd = 0;
		private IReadOnlyList<DisplayBlock> _highlightedNicks;
 
		private void ResetNickCompletion()
		{
			boxOutput.Presenter.ClearNicks(_highlightedNicks);
			_nickListPosition = 0;
			_tabKeyCount = 0;
			if (_lastReferencedNick != null)
			{
				_mostRecentlyReferenced.Remove(_lastReferencedNick);
				_mostRecentlyReferenced.AddFirst(_lastReferencedNick);
				if (_mostRecentlyReferenced.Count > 10)
				{
					_mostRecentlyReferenced.RemoveLast();
				}
				_lastReferencedNick = null;
			}
		}

		private bool CycleThroughNickCandidates()
		{
			if (_tabKeyCount > 1 &&
				_nickCandidates != null &&
				_nickCandidates.Length > 0)
			{
				_nickListPosition += 1;
				if (_nickListPosition >= _nickCandidates.Length)
				{
					_nickListPosition = 0;
				}

				InsertNick(_nickCandidates[_nickListPosition], txtInput.Text);
				return true;
			}

			return false;
		}

		class NickMatch
		{
			public string Nick { get; private set; }
			public Match Match { get; private set; }

			public NickMatch(string nick, Match match)
			{
				Nick = nick;
				Match = match;
			}
		}

		private void DoNickCompletion()
		{
			if (CycleThroughNickCandidates())
			{
				return;
			}

			var input = txtInput.Text;

			_currentNickStart = 0;
			_currentNickEnd = 0;
			if (input.Length > 0)
			{
				_currentNickStart = Math.Max(0, txtInput.CaretIndex - 1);
				_currentNickEnd = _currentNickStart < input.Length ? _currentNickStart + 1 : _currentNickStart;

				while (_currentNickStart >= 0 &&
						NicknameItem.IsNickChar(input[_currentNickStart]))
				{
					_currentNickStart--;
				}
				_currentNickStart++;

				while (_currentNickEnd < input.Length &&
						NicknameItem.IsNickChar(input[_currentNickEnd]))
				{
					_currentNickEnd++;
				}
			}
			else
			{
				_currentNickStart = _currentNickEnd = 0;
			}

			if (_currentNickEnd - _currentNickStart > 0)
			{
				var nickPart = input.Substring(_currentNickStart, _currentNickEnd - _currentNickStart);
				string nextNick = null;
				if (_nickCandidates == null)
				{
					var filter = new Regex(nickPart, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
					_nickCandidates = _mostRecentTalkers
						.Select(n => new NickMatch(n, filter.Match(n)))
						.Where(m => m.Match.Success)
						.OrderBy(m => m.Match.Index)
						.Concat(
							Nicknames.Select(n => new NickMatch(n.Nickname, filter.Match(n.Nickname)))
								.Where(m => m.Match.Success)
								.OrderBy(m => m.Match.Index)
						)
						.Select(m => m.Nick)
						.Distinct()
						.ToArray();

					if (_nickCandidates.Length > 0)
					{
						nextNick = _nickCandidates[0];
					}
					else
					{
						return;
					}
				}

				for (var i = 0; i < _nickCandidates.Length; i++)
				{
					if (string.Compare(_nickCandidates[i], nickPart, StringComparison.InvariantCulture) == 0)
					{
						nextNick = _nickCandidates[i];
						_nickListPosition = i;
						break;
					}
				}

				InsertNick(nextNick, input);
			}
			else if (_mostRecentlyReferenced.Count > 0)
			{
				_nickCandidates = _mostRecentlyReferenced.ToArray();
				InsertNick(_nickCandidates[0], "");
			}
		}

		private void InsertNick(string nextNick, string input)
		{
			boxOutput.Presenter.ClearNicks(_highlightedNicks);

			if (nextNick != null)
			{
				_lastReferencedNick = nextNick;
				_highlightedNicks = boxOutput.Presenter.HighlightNick(nextNick);
				if (_currentNickStart <= 1)
				{
					nextNick += ": ";
				}
				txtInput.Text = input.Substring(0, _currentNickStart) + nextNick + input.Substring(_currentNickEnd);
				txtInput.CaretIndex = _currentNickStart + nextNick.Length;
				_currentNickEnd = _currentNickStart + nextNick.Length;
			}
		}
	}
}