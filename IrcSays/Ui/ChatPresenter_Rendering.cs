using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using IrcSays.Utility;

namespace IrcSays.Ui
{
	public partial class ChatPresenter
	{
		private const double SeparatorPadding = 6.0;
		private const int TextProcessingBatchSize = 50;
		private const float MinNickBrightness = .2f;
		private const float NickBrightnessBand = .2f;

		private LinkedList<DisplayBlock> _blocks = new LinkedList<DisplayBlock>();
		private double _lineHeight;
		private LinkedListNode<DisplayBlock> _bottomBlock, _curBlock;
		private int _curLine;
		private bool _isProcessingText;
		private SolidColorBrush _nickHighlightFg = Brushes.Black;
		private SolidColorBrush _nickHighlightBg = Brushes.White;

		private Typeface Typeface
		{
			get { return new Typeface(FontFamily, FontStyle, FontWeight, FontStretch); }
		}

		private Color BackgroundColor
		{
			get
			{
				if (Background is SolidColorBrush)
				{
					return ((SolidColorBrush) Background).Color;
				}
				return Colors.Black;
			}
		}

		private string FormatNick(string nick)
		{
			if (!UseTabularView)
			{
				if (nick == null)
				{
					nick = "* ";
				}
				else
				{
					nick = string.Format("<{0}> ", nick);
				}
			}
			return nick ?? "*";
		}

		private string FormatTime(DateTime time)
		{
			return ShowTimestamp ? time.ToString(TimestampFormat + " ") : "";
		}

		private Brush GetNickColor(int hashCode)
		{
			var rand = new Random(hashCode * (NicknameColorSeed + 1));
			var bgv = Math.Max(Math.Max(BackgroundColor.R, BackgroundColor.G), BackgroundColor.B) / 255f;

			var v = (float) rand.NextDouble() * NickBrightnessBand + (bgv < 0.5f ? (1f - NickBrightnessBand) : MinNickBrightness);
			var h = 360f * (float) rand.NextDouble();
			var s = .4f + (.6f * (float) rand.NextDouble());
			return new SolidColorBrush(new HsvColor(1f, h, s, v).ToColor());
		}

		public void AppendBulkLines(IEnumerable<ChatLine> lines)
		{
			foreach (var line in lines)
			{
				string nickText = null;
				if (line.Nick != null)
				{
					nickText = line.Nick;
					if (nickText.StartsWith("+"))
					{
						nickText = nickText.Substring(1);
					}
				}
				var newBlock = new DisplayBlock
				{
					Source = line,
					NickText = nickText
				};
				newBlock.TimeString = FormatTime(newBlock.Source.Time);
				newBlock.FormattedNick = FormatNick(newBlock.Source.Nick);

				var offset = _blocks.Last != null ? _blocks.Last.Value.CharEnd : 0;
				newBlock.CharStart = offset;
				offset += newBlock.TimeString.Length + newBlock.FormattedNick.Length + newBlock.Source.Text.Length;
				newBlock.CharEnd = offset;

				_blocks.AddLast(newBlock);
			}
			StartProcessingText();
		}

		public void AppendLine(ChatLine line)
		{
			string nickText = null;
			if (line.Nick != null)
			{
				nickText = line.Nick;
				if (nickText.StartsWith("+"))
				{
					nickText = nickText.Substring(1);
				}
			}
			var newBlock = new DisplayBlock
			{
				Source = line,
				NickText = nickText
			};

			newBlock.TimeString = FormatTime(newBlock.Source.Time);
			newBlock.FormattedNick = FormatNick(newBlock.Source.Nick);

			var offset = _blocks.Last != null ? _blocks.Last.Value.CharEnd : 0;
			newBlock.CharStart = offset;
			offset += newBlock.TimeString.Length + newBlock.FormattedNick.Length + newBlock.Source.Text.Length;
			newBlock.CharEnd = offset;

			_blocks.AddLast(newBlock);
			FormatOne(newBlock, AutoSizeColumn);
			_bufferLines += newBlock.Text.Length;

			while (_blocks.Count > BufferLines)
			{
				if (_blocks.First.Value.Text != null)
				{
					_bufferLines -= _blocks.First.Value.Text.Length;
				}
				if (_blocks.First == _curSearchBlock)
				{
					ClearSearch();
				}
				_blocks.RemoveFirst();
			}

			InvalidateScrollInfo();
			if (!_isAutoScrolling || _isSelecting)
			{
				_scrollPos += newBlock.Text.Length;
			}
			InvalidateVisual();
		}

		private void FormatOne(DisplayBlock displayBlock, bool autoSize, bool highlightNick = false)
		{
			displayBlock.Foreground = Palette[displayBlock.Source.ColorKey];

			var formatter = new ChatFormatter(Typeface, FontSize, Foreground, Palette);
			displayBlock.Time = formatter.Format(displayBlock.TimeString, null, ViewportWidth, displayBlock.Foreground, Background,
				TextWrapping.NoWrap).FirstOrDefault();
			displayBlock.NickX = displayBlock.Time != null ? displayBlock.Time.WidthIncludingTrailingWhitespace : 0.0;

			var nickBrush = displayBlock.Foreground;
			if (ColorizeNicknames && displayBlock.Source.NickHashCode != 0)
			{
				nickBrush = GetNickColor(displayBlock.Source.NickHashCode);
			}

			var nickFg = nickBrush;
			var nickBg = Background;
			var forceNickBackground = false;
			if (highlightNick)
			{
				nickFg = Background;
				nickBg = nickBrush;
				forceNickBackground = true;
			}
			displayBlock.Nick = formatter.Format(displayBlock.FormattedNick, null, ViewportWidth - displayBlock.NickX, nickFg, nickBg,
				TextWrapping.NoWrap, forceNickBackground).First();
			displayBlock.TextX = displayBlock.NickX + displayBlock.Nick.WidthIncludingTrailingWhitespace;

			if (autoSize && displayBlock.TextX > ColumnWidth)
			{
				ColumnWidth = displayBlock.TextX;
				InvalidateAll(false);
			}

			if (UseTabularView)
			{
				displayBlock.TextX = ColumnWidth + SeparatorPadding * 2.0 + 1.0;
				displayBlock.NickX = ColumnWidth - displayBlock.Nick.WidthIncludingTrailingWhitespace;
			}

			displayBlock.Text = formatter.Format(displayBlock.Source.Text, displayBlock.Source, ViewportWidth - displayBlock.TextX, displayBlock.Foreground,
				Background, TextWrapping.Wrap).ToArray();
			displayBlock.Height = displayBlock.Text.Sum(t => t.Height);
		}

		private void InvalidateAll(bool styleChanged)
		{
			_lineHeight = Math.Ceiling(FontSize * Typeface.FontFamily.LineSpacing);

			if (styleChanged)
			{
				var offset = 0;
				foreach (var b in _blocks)
				{
					b.CharStart = offset;
					b.TimeString = FormatTime(b.Source.Time);
					b.FormattedNick = FormatNick(b.Source.Nick);
					offset += b.TimeString.Length + b.FormattedNick.Length + b.Source.Text.Length;
					b.CharEnd = offset;
				}
			}

			StartProcessingText();
		}

		private void StartProcessingText()
		{
			_curBlock = _blocks.Last;
			_curLine = 0;
			if (!_isProcessingText)
			{
				_isProcessingText = true;
				System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) ProcessText, DispatcherPriority.ApplicationIdle, null);
			}
		}

		private void ProcessText()
		{
			var count = 0;
			while (_curBlock != null &&
					count < TextProcessingBatchSize)
			{
				var oldLineCount = _curBlock.Value.Text != null ? _curBlock.Value.Text.Length : 0;
				FormatOne(_curBlock.Value, false);
				_curLine += _curBlock.Value.Text.Length;
				var deltaLines = _curBlock.Value.Text.Length - oldLineCount;
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
				System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) ProcessText, DispatcherPriority.ApplicationIdle, null);
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
			_bottomBlock = null;
			var guidelines = new GuidelineSet();

			dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Size(ViewportWidth, ActualHeight)));

			var node = _blocks.Last;
			do
			{
				if (node == null)
				{
					break;
				}

				var block = node.Value;
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

					if ((block.Source.Marker & ChatMarker.NewMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(NewMarkerColor,
							NewMarkerTransparentColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, block.Y), new Size(ViewportWidth, _lineHeight * 5)));
					}
					if ((block.Source.Marker & ChatMarker.OldMarker) > 0)
					{
						var markerBrush = new LinearGradientBrush(OldMarkerTransparentColor,
							OldMarkerColor, 90.0);
						dc.DrawRectangle(markerBrush, null,
							new Rect(new Point(0.0, (block.Y + block.Height) - _lineHeight * 5),
								new Size(ViewportWidth, _lineHeight * 5)));
					}

					if (_bottomBlock == null)
					{
						_bottomBlock = node;
					}
					guidelines.GuidelinesY.Add(vPos + guidelineHeight);
				}
			} while (node.Previous != null &&
					vPos >= -_lineHeight * 5.0 &&
					(node = node.Previous) != null);

			dc.PushGuidelineSet(guidelines);

			if (UseTabularView)
			{
				var lineX = ColumnWidth + SeparatorPadding;
				dc.DrawLine(scaledPen, new Point(lineX, 0.0), new Point(lineX, ActualHeight));
			}

			if (_blocks.Count < 1)
			{
				return;
			}

			_drawCounter += 1;

			do
			{
				var block = node.Value;
				if (double.IsNaN(block.Y))
				{
					continue;
				}

				block.DrawCounter = _drawCounter;

				if ((block.Source.Marker & ChatMarker.Attention) > 0)
				{
					var markerBrush = new SolidColorBrush(AttentionColor);
					dc.DrawRectangle(markerBrush, null,
						new Rect(new Point(block.TextX, block.Y), new Size(ViewportWidth - block.TextX, block.Height)));
				}

				block.Nick.Draw(dc, new Point(block.NickX, block.Y), InvertAxes.None);
				if (block.Time != null)
				{
					block.Time.Draw(dc, new Point(0.0, block.Y), InvertAxes.None);
				}
				for (var k = 0; k < block.Text.Length; k++)
				{
					block.Text[k].Draw(dc, new Point(block.TextX, block.Y + k * _lineHeight), InvertAxes.None);
				}

				if (IsSelecting)
				{
					DrawSelectionHighlight(dc, block);
				}
				if (node == _curSearchBlock)
				{
					DrawSearchHighlight(dc, node.Value);
				}
			} while ((node = node.Next) != null);
		}

		public IReadOnlyList<DisplayBlock> HighlightNick(string nick)
		{
			if (_blocks.Count == 0)
			{
				return new DisplayBlock[0];
			}

			var highlightedNicks = new List<DisplayBlock>();

			var node = _blocks.Last;
			do
			{
				var block = node.Value;
				if (block.DrawCounter != _drawCounter)
				{
					break;
				}

				if (string.Compare(nick, block.NickText, StringComparison.OrdinalIgnoreCase) == 0)
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

		public void ClearNicks(IReadOnlyList<DisplayBlock> nicks)
		{
			if (nicks.IsNullOrEmpty())
			{
				return;
			}

			foreach (var nick in nicks)
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