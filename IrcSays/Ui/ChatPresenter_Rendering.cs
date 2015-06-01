using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;

namespace IrcSays.Ui
{
	public partial class ChatPresenter
	{
		private const double SeparatorPadding = 6.0;
		private const int TextProcessingBatchSize = 50;
		private const float MinNickBrightness = .2f;
		private const float NickBrightnessBand = .2f;

		private class Block
		{
			public ChatLine Source { get; set; }
			public Brush Foreground { get; set; }

			public string TimeString { get; set; }
			public string NickString { get; set; }

			public TextLine Time { get; set; }
			public TextLine Nick { get; set; }
			public TextLine[] Text { get; set; }

			public int CharStart { get; set; }
			public int CharEnd { get; set; }
			public double Y { get; set; }
			public double NickX { get; set; }
			public double TextX { get; set; }
			public double Height { get; set; }
		}

		private LinkedList<Block> _blocks = new LinkedList<Block>();
		private double _lineHeight;
		private LinkedListNode<Block> _bottomBlock, _curBlock;
		private int _curLine;
		private bool _isProcessingText;

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
				var b = new Block();
				b.Source = line;
				b.TimeString = FormatTime(b.Source.Time);
				b.NickString = FormatNick(b.Source.Nick);

				var offset = _blocks.Last != null ? _blocks.Last.Value.CharEnd : 0;
				b.CharStart = offset;
				offset += b.TimeString.Length + b.NickString.Length + b.Source.Text.Length;
				b.CharEnd = offset;

				_blocks.AddLast(b);
			}
			StartProcessingText();
		}

		public void AppendLine(ChatLine line)
		{
			var b = new Block();
			b.Source = line;

			b.TimeString = FormatTime(b.Source.Time);
			b.NickString = FormatNick(b.Source.Nick);

			var offset = _blocks.Last != null ? _blocks.Last.Value.CharEnd : 0;
			b.CharStart = offset;
			offset += b.TimeString.Length + b.NickString.Length + b.Source.Text.Length;
			b.CharEnd = offset;

			_blocks.AddLast(b);
			FormatOne(b, AutoSizeColumn);
			_bufferLines += b.Text.Length;

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
				_scrollPos += b.Text.Length;
			}
			InvalidateVisual();
		}

		private void FormatOne(Block b, bool autoSize)
		{
			b.Foreground = Palette[b.Source.ColorKey];

			var formatter = new ChatFormatter(Typeface, FontSize, Foreground, Palette);
			b.Time = formatter.Format(b.TimeString, null, ViewportWidth, b.Foreground, Background,
				TextWrapping.NoWrap).FirstOrDefault();
			b.NickX = b.Time != null ? b.Time.WidthIncludingTrailingWhitespace : 0.0;

			var nickBrush = b.Foreground;
			if (ColorizeNicknames && b.Source.NickHashCode != 0)
			{
				nickBrush = GetNickColor(b.Source.NickHashCode);
			}
			b.Nick = formatter.Format(b.NickString, null, ViewportWidth - b.NickX, nickBrush, Background,
				TextWrapping.NoWrap).First();
			b.TextX = b.NickX + b.Nick.WidthIncludingTrailingWhitespace;

			if (autoSize && b.TextX > ColumnWidth)
			{
				ColumnWidth = b.TextX;
				InvalidateAll(false);
			}

			if (UseTabularView)
			{
				b.TextX = ColumnWidth + SeparatorPadding * 2.0 + 1.0;
				b.NickX = ColumnWidth - b.Nick.WidthIncludingTrailingWhitespace;
			}

			b.Text = formatter.Format(b.Source.Text, b.Source, ViewportWidth - b.TextX, b.Foreground,
				Background, TextWrapping.Wrap).ToArray();
			b.Height = b.Text.Sum(t => t.Height);
		}

		private void InvalidateAll(bool styleChanged)
		{
			var formatter = new ChatFormatter(Typeface, FontSize, Foreground, Palette);
			_lineHeight = Math.Ceiling(FontSize * Typeface.FontFamily.LineSpacing);

			if (styleChanged)
			{
				var offset = 0;
				foreach (var b in _blocks)
				{
					b.CharStart = offset;
					b.TimeString = FormatTime(b.Source.Time);
					b.NickString = FormatNick(b.Source.Nick);
					offset += b.TimeString.Length + b.NickString.Length + b.Source.Text.Length;
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

			do
			{
				var block = node.Value;
				if (double.IsNaN(block.Y))
				{
					continue;
				}

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

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			if (sizeInfo.WidthChanged)
			{
				InvalidateAll(false);
			}
		}
	}
}