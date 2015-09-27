using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Std.Ui.Logging
{
	public partial class LogView
	{
		private static readonly Lazy<Brush> _searchBrush = new Lazy<Brush>(() =>
		{
			var c = SystemColors.HotTrackColor;
			c.A = 102;
			return new SolidColorBrush(c);
		});

		public void Search(Regex pattern, SearchDirection dir)
		{
			_searchProvider.Search(pattern, dir);
		}

		public void ClearSearch()
		{
			_searchProvider.ClearSearch();
		}

		public void ScrollIntoView(IntrusiveListNode<DisplayBlock> targetNode)
		{
			var pos = 0;
			var node = Blocks.Last;
			while (node != null &&
					node != targetNode)
			{
				pos += node.Text.Length;
				node = node.Previous;
			}
			_scrollPos = Math.Max(
				Math.Min(_scrollPos, Math.Max(0, pos - VisibleLineCount / 2)),
				Math.Min(_bufferLines - VisibleLineCount + 1, pos - VisibleLineCount / 2 + node.Text.Length));
			InvalidateScrollInfo();
		}

		private void DrawSearchHighlight(DrawingContext dc, DisplayBlock displayBlock)
		{
			foreach (var pair in _searchProvider.Matches)
			{
				var txtOffset = 0;
				var y = displayBlock.Y;

				for (var i = 0; i < displayBlock.Text.Length; i++)
				{
					var start = Math.Max(txtOffset, pair.Start);
					var end = Math.Min(txtOffset + displayBlock.Text[i].Length, pair.End);

					if (end > start)
					{
						var x1 = displayBlock.Text[i].GetDistanceFromCharacterHit(new CharacterHit(start, 0)) + displayBlock.TextX;
						var x2 = displayBlock.Text[i].GetDistanceFromCharacterHit(new CharacterHit(end, 0)) + displayBlock.TextX;

						dc.DrawRectangle(_searchBrush.Value, null,
							new Rect(new Point(x1, y), new Point(x2, y + _lineHeight)));
					}

					y += _lineHeight;
					txtOffset += displayBlock.Text[i].Length;
				}
			}
		}
	}
}