using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;

namespace Std.Ui.Logging
{
	public partial class LogView
	{
		public void Append(IEnumerable<LogEntry> entries)
		{
			foreach (var entry in entries)
			{
				string nameText = null;
				if (entry.Name != null)
				{
					nameText = entry.Name;
					if (nameText.StartsWith("+"))
					{
						nameText = nameText.Substring(1);
					}
				}
				var newBlock = new DisplayBlock
				{
					Entry = entry,
					NameText = nameText
				};
				newBlock.TimeString = _blockFormatter.FormatTimestamp(newBlock.Entry.Time);
				newBlock.FormattedName = _blockFormatter.FormatName(newBlock.Entry.Name);

				var offset = Blocks.Last?.CharEnd ?? 0;
				newBlock.CharStart = offset;
				offset += newBlock.TimeString.Length + newBlock.FormattedName.Length + newBlock.Entry.Text.Length;
				newBlock.CharEnd = offset;

				Blocks.AddLast(newBlock);
			}
			StartProcessingText();
		}

		public void Append(LogEntry line)
		{
			string nameText = null;
			if (line.Name != null)
			{
				nameText = line.Name;
				if (nameText.StartsWith("+"))
				{
					nameText = nameText.Substring(1);
				}
			}
			var newBlock = new DisplayBlock
			{
				Entry = line,
				NameText = nameText
			};

			newBlock.TimeString = _blockFormatter.FormatTimestamp(newBlock.Entry.Time);
			newBlock.FormattedName = _blockFormatter.FormatName(newBlock.Entry.Name);

			var offset = Blocks.Last?.CharEnd ?? 0;
			newBlock.CharStart = offset;
			offset += newBlock.TimeString.Length + 
				newBlock.FormattedName.Length + 
				newBlock.Entry.Text.Length;
			newBlock.CharEnd = offset;

			Blocks.AddLast(newBlock);
			FormatOne(newBlock, AutoSizeColumn);
			_bufferLines += newBlock.Text.Length;

			while (Blocks.Count > BufferLines)
			{
				if (Blocks.First.Text != null)
				{
					_bufferLines -= Blocks.First.Text.Length;
				}
				if (Blocks.First == _searchProvider.CurrentSearchBlock)
				{
					ClearSearch();
				}
				Blocks.RemoveFirst();
			}

			InvalidateScrollInfo();
			if (!_isAutoScrolling || _isSelecting)
			{
				_scrollPos += newBlock.Text.Length;
			}
			InvalidateVisual();
		}

		private void FormatOne(DisplayBlock displayBlock, bool autoSize, bool highlightName = false)
		{
			_blockFormatter.FormatBlock(displayBlock, autoSize, highlightName);
		}

		private void InvalidateAll(bool styleChanged)
		{
			_lineHeight = Math.Ceiling(FontSize * _typeface.FontFamily.LineSpacing);

			if (styleChanged)
			{
				var offset = 0;
				foreach (var block in Blocks)
				{
					block.CharStart = offset;
					block.TimeString = _blockFormatter.FormatTimestamp(block.Entry.Time);
					block.FormattedName = _blockFormatter.FormatName(block.Entry.Name);
					offset += block.TimeString.Length + block.FormattedName.Length + block.Entry.Text.Length;
					block.CharEnd = offset;
				}
			}

			StartProcessingText();
		}

		private void StartProcessingText()
		{
			_curBlock = Blocks.Last;
			_curLine = 0;
			if (!_isProcessingText)
			{
				_isProcessingText = true;
				Application.Current.Dispatcher.BeginInvoke((Action) ProcessText, DispatcherPriority.ApplicationIdle, null);
			}
		}

		private void ProcessText()
		{
			var count = 0;
			while (_curBlock != null &&
					count < TextProcessingBatchSize)
			{
				var oldLineCount = _curBlock.Text?.Length ?? 0;
				FormatOne(_curBlock, false);
				_curLine += _curBlock.Text.Length;
				var deltaLines = _curBlock.Text.Length - oldLineCount;
				_bufferLines += deltaLines;
				if (_curLine < _scrollPos)
				{
					_scrollPos += deltaLines;
				}
				_curBlock = _curBlock.Previous;
				count++;
			}

			if (_curBlock == null)
			{
				_isProcessingText = false;
			}
			else
			{
				Application.Current.Dispatcher.BeginInvoke((Action) ProcessText, DispatcherPriority.ApplicationIdle, null);
			}

			InvalidateScrollInfo();
			InvalidateVisual();
		}

		private long _drawCounter;

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var visual = PresentationSource.FromVisual(this);
			if (visual == null)
			{
				return;
			}
			var m = visual.CompositionTarget.TransformToDevice;
			var scaledPen = new Pen(DividerBrush, 1 / m.M11);
			var guidelineHeight = scaledPen.Thickness;

			var vPos = ActualHeight;
			var curLine = 0;
			BottomBlock = null;
			var guidelines = new GuidelineSet();

			dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Size(ViewportWidth, ActualHeight)));

			var node = Blocks.Last;
			do
			{
				if (node == null)
				{
					break;
				}

				var block = node;
				block.Y = double.NaN;

				var drawAny = false;
				if (block.Text == null ||
					block.Text.Length < 1)
				{
					continue;
				}
				for (var j = block.Text.Length - 1; j >= 0; --j)
				{
					var line = block.Text[j];
					if (curLine++ < _scrollPos)
					{
						continue;
					}
					vPos -= line.Height;
					drawAny = true;
				}
				if (drawAny)
				{
					block.Y = vPos;

					if ((block.Entry.Marker & LogEntryMarker.NewMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(NewMarkerColor,
							NewMarkerTransparentColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, block.Y), new Size(ViewportWidth, _lineHeight * 5)));
					}
					if ((block.Entry.Marker & LogEntryMarker.OldMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(OldMarkerTransparentColor,
							OldMarkerColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, (block.Y + block.Height) - _lineHeight * 5),
								new Size(ViewportWidth, _lineHeight * 5)));
					}

					if (BottomBlock == null)
					{
						BottomBlock = node;
					}
					guidelines.GuidelinesY.Add(vPos + guidelineHeight);
				}
			} while (node.Previous != null &&
					vPos >= -_lineHeight * 5.0 &&
					(node = node.Previous) != null);

			dc.PushGuidelineSet(guidelines);

			if (JustifyNameColumn)
			{
				var lineX = NameColumnWidth + _blockFormatter.SeparatorPadding;
				dc.DrawLine(scaledPen, new Point(lineX, 0.0), new Point(lineX, ActualHeight));
			}

			if (Blocks.Count < 1)
			{
				return;
			}

			_drawCounter += 1;

			do
			{
				var block = node;
				if (double.IsNaN(block.Y))
				{
					continue;
				}

				block.DrawCounter = _drawCounter;

				if ((block.Entry.Marker & LogEntryMarker.Attention) > 0)
				{
					var markerBrush = new SolidColorBrush(AttentionColor);
					dc.DrawRectangle(markerBrush, null,
						new Rect(new Point(block.TextX, block.Y), new Size(ViewportWidth - block.TextX, block.Height)));
				}

				block.Name.Draw(dc, new Point(block.NameX, block.Y), InvertAxes.None);
				block.Time?.Draw(dc, new Point(0.0, block.Y), InvertAxes.None);
				for (var k = 0; k < block.Text.Length; k++)
				{
					block.Text[k].Draw(dc, new Point(block.TextX, block.Y + k * _lineHeight), InvertAxes.None);
				}

				if (IsSelecting)
				{
					DrawSelectionHighlight(dc, block);
				}
				if (node == _searchProvider.CurrentSearchBlock)
				{
					DrawSearchHighlight(dc, node);
				}
			} while ((node = node.Next) != null);
		}

		public IReadOnlyList<DisplayBlock> HighlightEntriesByName(string entryName)
		{
			if (Blocks.Count == 0)
			{
				return new DisplayBlock[0];
			}

			var highlightedNicks = new List<DisplayBlock>();

			var node = Blocks.Last;
			do
			{
				var block = node;
				if (block.DrawCounter != _drawCounter)
				{
					break;
				}

				if (string.Compare(entryName, block.NameText, StringComparison.OrdinalIgnoreCase) == 0)
				{
					highlightedNicks.Add(block);
					FormatOne(block, false, true);
				}
			} while (node.Previous != null &&
					(node = node.Previous) != null);

			if (highlightedNicks.Count > 0)
			{
				InvalidateVisual();
			}

			return highlightedNicks;
		}

		public void UnhighlightEntries(IReadOnlyList<DisplayBlock> names)
		{
			if (names == null ||
				names.Count == 0)
			{
				return;
			}

			foreach (var nick in names)
			{
				FormatOne(nick, false);
			}

			InvalidateVisual();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			if (sizeInfo.WidthChanged)
			{
				InvalidateAll(false);
			}
		}
	}
}