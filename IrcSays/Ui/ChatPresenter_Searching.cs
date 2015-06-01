using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace IrcSays.Ui
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private LinkedListNode<Block> _curSearchBlock;
		private List<Tuple<int, int>> _curSearchMatches;

		private static readonly Lazy<Brush> _searchBrush = new Lazy<Brush>(() =>
		{
			var c = SystemColors.HotTrackColor;
			c.A = 102;
			return new SolidColorBrush(c);
		});

		public void Search(Regex pattern, SearchDirection dir)
		{
			var node = _curSearchBlock;

			// No search in progress; set current node to the bottom visible block
			if (node == null)
			{
				node = _bottomBlock ?? _blocks.Last;
			}
			else
			{
				// Move back to the previous node. If we're at the top or bottom, do nothing.
				node = dir == SearchDirection.Previous ? _curSearchBlock.Previous : _curSearchBlock.Next;
				if (node == null)
				{
					return;
				}
			}

			while (node != null)
			{
				var matches = (from Match m in pattern.Matches(node.Value.Source.Text)
					select new Tuple<int, int>(m.Index, m.Index + m.Length)).ToList();
				if (matches.Count > 0)
				{
					_curSearchBlock = node;
					_curSearchMatches = matches;
					break;
				}
				node = dir == SearchDirection.Previous ? node.Previous : node.Next;
			}

			if (_curSearchBlock != null)
			{
				ScrollIntoView(_curSearchBlock);
				InvalidateVisual();
			}
		}

		public void ClearSearch()
		{
			_curSearchBlock = null;
			InvalidateVisual();
		}

		private void ScrollIntoView(LinkedListNode<Block> targetNode)
		{
			var pos = 0;
			var node = _blocks.Last;
			while (node != null &&
					node != targetNode)
			{
				pos += node.Value.Text.Length;
				node = node.Previous;
			}
			_scrollPos = Math.Max(
				Math.Min(_scrollPos, Math.Max(0, pos - VisibleLineCount / 2)),
				Math.Min(_bufferLines - VisibleLineCount + 1, pos - VisibleLineCount / 2 + node.Value.Text.Length));
			InvalidateScrollInfo();
		}

		private void DrawSearchHighlight(DrawingContext dc, Block block)
		{
			foreach (var pair in _curSearchMatches)
			{
				var txtOffset = 0;
				var y = block.Y;

				for (var i = 0; i < block.Text.Length; i++)
				{
					var start = Math.Max(txtOffset, pair.Item1);
					var end = Math.Min(txtOffset + block.Text[i].Length, pair.Item2);

					if (end > start)
					{
						var x1 = block.Text[i].GetDistanceFromCharacterHit(new CharacterHit(start, 0)) + block.TextX;
						var x2 = block.Text[i].GetDistanceFromCharacterHit(new CharacterHit(end, 0)) + block.TextX;

						dc.DrawRectangle(_searchBrush.Value, null,
							new Rect(new Point(x1, y), new Point(x2, y + _lineHeight)));
					}

					y += _lineHeight;
					txtOffset += block.Text[i].Length;
				}
			}
		}
	}
}