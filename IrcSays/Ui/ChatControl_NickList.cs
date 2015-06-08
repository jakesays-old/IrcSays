using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IrcSays.Ui
{
	public partial class ChatControl : ChatPage
	{
		private string[] _nickCandidates;
		private readonly NicknameList _nickList;

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

		private void ResetNickCompletion()
		{
			_nickListPosition = 0;
			_tabKeyCount = 0;
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
					_nickCandidates = Nicknames.Select(n => new
					{
						Nick = n,
						Match = filter.Match(n.Nickname)
					})
						.Where(m => m.Match.Success)
						.OrderBy(m => m.Match.Index)
						.Select(m => m.Nick.Nickname)
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
						//nextNick = i < _nickCandidates.Length - 1 ? _nickCandidates[i + 1] : _nickCandidates[0];
						break;
					}
				}

				var keepNickCandidates = _nickCandidates;
				InsertNick(nextNick, input);
				_nickCandidates = keepNickCandidates;
			}
		}

		private void InsertNick(string nextNick, string input)
		{
			if (nextNick != null)
			{
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