using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Std.Ui.Logging;

namespace IrcSays.Ui.Logging
{
	class IrcSearchProvider : ISearchProvider
	{
		private LogView _view;
		private DisplayBlock _currentSearchBlock;

		public DisplayBlock CurrentSearchBlock => _currentSearchBlock;

        public List<SearchMatch> Matches { get; private set; }

		public void Attach(LogView view)
        {
	        _view = view;
        }

		public void Search(Regex pattern, SearchDirection dir)
		{
			var node = _currentSearchBlock;

			// No search in progress; set current node to the bottom visible block
			if (node == null)
			{
				node = _view.BottomBlock ?? _view.Blocks.Last;
			}
			else
			{
				// Move back to the previous node. If we're at the top or bottom, do nothing.
				node = dir == SearchDirection.Previous ? _currentSearchBlock.Previous : _currentSearchBlock.Next;
				if (node == null)
				{
					return;
				}
			}

			while (node != null)
			{
				var matches = (from Match m in pattern.Matches(node.Entry.Text)
							   select new SearchMatch(m.Index, m.Index + m.Length)).ToList();
				if (matches.Count > 0)
				{
					_currentSearchBlock = node;
					Matches = matches;
					break;
				}
				node = dir == SearchDirection.Previous ? node.Previous : node.Next;
			}

			if (_currentSearchBlock != null)
			{
				_view.ScrollIntoView(_currentSearchBlock);
				_view.InvalidateVisual();
			}
		}

		public void ClearSearch()
		{
			Matches.Clear();
			_currentSearchBlock = null;
			_view.InvalidateVisual();
		}
	}
}