using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Std.Ui.Logging
{
	public struct SearchMatch
	{
		public int Start { get; }
		public int End { get; }

		public SearchMatch(int start, int end)
		{
			Start = start;
			End = end;
		}
	}

	public interface ISearchProvider
	{
		DisplayBlock CurrentSearchBlock { get; }
		List<SearchMatch> Matches { get; }

		void Attach(LogView view);

		void Search(Regex pattern, SearchDirection dir);

		void ClearSearch();
	}
}