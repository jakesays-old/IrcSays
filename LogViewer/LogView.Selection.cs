using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Std.Ui.Logging
{
	public partial class LogView
	{
		private bool _isDragging;
		private bool _isSelecting;
		private Brush _selectBrush;
		private int _selStart = -1, _selEnd = -1;

		protected int SelectionStart
		{
			get { return Math.Min(_selStart, _selEnd); }
		}

		protected int SelectionEnd
		{
			get { return Math.Max(_selStart, _selEnd); }
		}

		protected bool IsSelecting
		{
			get { return _isSelecting && SelectionEnd >= SelectionStart && _selStart >= 0 && _selEnd >= 0; }
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (!_isDragging &&
				!_isSelecting)
			{
				var p = e.GetPosition(this);
				if (Math.Abs(p.X - (_blockFormatter.NameColumnWidth + _blockFormatter.SeparatorPadding)) <
					_blockFormatter.SeparatorPadding / 2.0 &&
					_blockFormatter.JustifyNameColumn)
				{
					_isDragging = true;
					CaptureMouse();
				}
				else
				{
					var idx = GetCharIndexAt(p, false);
					if (idx >= 0 &&
						idx < int.MaxValue)
					{
						_isSelecting = true;
						CaptureMouse();
						_selStart = idx;
					}
				}
			}

			OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			var p = e.GetPosition(this);

			if (_isSelecting)
			{
				Mouse.OverrideCursor = Cursors.IBeam;
				var newSelEnd = GetCharIndexAt(e.GetPosition(this));
				if (newSelEnd != _selEnd)
				{
					_selEnd = newSelEnd;
					InvalidateVisual();
				}
				e.Handled = true;
			}
			else if (_isDragging)
			{
				NameColumnWidth = Math.Max(0.0, Math.Min(ViewportWidth / 2.0, p.X));
				InvalidateAll(false);
			}
			else if (_blockFormatter.JustifyNameColumn &&
					Math.Abs(p.X - (_blockFormatter.NameColumnWidth + _blockFormatter.SeparatorPadding))
					< _blockFormatter.SeparatorPadding / 2.0)
			{
				Mouse.OverrideCursor = Cursors.SizeWE;
			}
			else
			{
				Mouse.OverrideCursor = null;
				SelectedLink = HitTest(p);
				if (SelectedLink != null)
				{
					Mouse.OverrideCursor = Cursors.Hand;
				}
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (_isDragging)
			{
				_isDragging = false;
				ReleaseMouseCapture();
				Mouse.OverrideCursor = null;
				InvalidateAll(false);
			}
			else if (_isSelecting)
			{
				if (_selStart >= 0 &&
					_selEnd >= 0)
				{
					var selText = GetSelectedText();
					if (selText.Length >= MinimumCopyLength)
					{
						try
						{
							Clipboard.SetText(selText);
						}
						catch
						{
							// sometimes another app locks the clipboard and WPF doesn't handle that
							// in that case, better to ignore it than to crash
						}
					}
				}

				ReleaseMouseCapture();
				if (Mouse.OverrideCursor == Cursors.IBeam)
				{
					Mouse.OverrideCursor = null;
				}
				_isSelecting = false;
				_selStart = -1;
				_selEnd = -1;

				if (_isAutoScrolling)
				{
					ScrollToEnd();
				}

				InvalidateVisual();
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			if (!_isSelecting &&
				!_isDragging)
			{
				Mouse.OverrideCursor = null;
				SelectedLink = null;
			}
			base.OnMouseLeave(e);
		}

		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			_isSelecting = false;
			_isDragging = false;
			InvalidateVisual();
			base.OnLostMouseCapture(e);
		}

		public string HitTest(Point p)
		{
			var block = GetBlockAt(p.Y);
			if (block != null)
			{
				if (p.Y >= block.Y &&
					p.Y < block.Y + block.Name.Height &&
					p.X >= block.NameX &&
					p.X < block.NameX + block.Name.Width &&
					block.Entry.Name != null)
				{
					return block.Entry.Name;
				}
				if (block.Entry.Links.Length > 0)
				{
					var line = (int) (p.Y - block.Y) / (int) _lineHeight;
					if (line >= 0 &&
						line < block.Text.Length &&
						p.X >= block.TextX &&
						p.X < block.TextX + block.Text[line].Width)
					{
						var ch = block.Text[line].GetCharacterHitFromDistance(p.X - block.TextX);
						foreach (var l in block.Entry.Links)
						{
							if (ch.FirstCharacterIndex >= l.Start &&
								ch.FirstCharacterIndex < l.End)
							{
								return block.Entry.Text.Substring(l.Start, l.End - l.Start);
							}
						}
					}
				}
			}
			return null;
		}

		private DisplayBlock GetBlockAt(double y)
		{
			if (BottomBlock == null)
			{
				return null;
			}
			var node = BottomBlock;
			do
			{
				if (y >= node.Y &&
					y < node.Y + node.Height)
				{
					return node;
				}
			} while ((node = node.Previous) != null &&
					y >= 0.0);
			return null;
		}

		private int GetCharIndexAt(Point p, bool allowSelectionAboveTopLine = true)
		{
			if (Blocks.Count < 1 ||
				BottomBlock == null)
			{
				return -1;
			}

			p.Y = Math.Min(ActualHeight - 1, Math.Max(0, p.Y));
			DisplayBlock displayBlock = null;
			var node = BottomBlock;
			do
			{
				if (p.Y >= node.Y &&
					p.Y < node.Y + node.Height)
				{
					break;
				}
			} while (node.Previous != null &&
					p.Y >= 0.0 &&
					(node = node.Previous) != null);
			displayBlock = node;
			if (!allowSelectionAboveTopLine &&
				p.Y < displayBlock.Y)
			{
				return -1;
			}
			p.Y = Math.Max(displayBlock.Y, p.Y);
			if (displayBlock.Text == null)
			{
				return -1;
			}

			var line = (int) (p.Y - displayBlock.Y) / (int) _lineHeight;
			if (line >= displayBlock.Text.Length)
			{
				line = displayBlock.Text.Length - 1;
			}
			var idx = 0;
			if (line > 0 ||
				p.X >= displayBlock.TextX)
			{
				idx += displayBlock.TimeString.Length + displayBlock.FormattedName.Length;
				var ch = displayBlock.Text[line].GetCharacterHitFromDistance(p.X - displayBlock.TextX);
				idx += Math.Min(ch.FirstCharacterIndex, displayBlock.Entry.Text.Length - 1);
			}
			else if (p.X >= displayBlock.NameX ||
					displayBlock.Time == null)
			{
				idx += displayBlock.TimeString.Length;
				var ch = displayBlock.Name.GetCharacterHitFromDistance(p.X - displayBlock.NameX);
				idx += Math.Min(ch.FirstCharacterIndex, displayBlock.FormattedName.Length - 1);
			}
			else
			{
				var ch = displayBlock.Time.GetCharacterHitFromDistance(p.X);
				idx += Math.Min(ch.FirstCharacterIndex, displayBlock.TimeString.Length - 1);
			}
			return idx + displayBlock.CharStart;
		}

		private void FindSelectedArea(int idx, int txtLen, int txtOffset, double x, TextLine line, ref double start,
			ref double end)
		{
			var first = Math.Max(txtOffset, SelectionStart - idx);
			var last = Math.Min(txtLen - 1 + txtOffset, SelectionEnd - idx);
			if (last >= first)
			{
				start = Math.Min(start, line.GetDistanceFromCharacterHit(new CharacterHit(first, 0)) + x);
				end = Math.Max(end, line.GetDistanceFromCharacterHit(new CharacterHit(last, 1)) + x);
			}
		}

		private void DrawSelectionHighlight(DrawingContext dc, DisplayBlock displayBlock)
		{
			if (SelectionEnd < displayBlock.CharStart ||
				SelectionStart >= displayBlock.CharEnd ||
				SelectionStart > SelectionEnd)
			{
				return;
			}

			var idx = displayBlock.CharStart;
			var txtOffset = 0;
			var y = displayBlock.Y;
			for (var i = 0; i < displayBlock.Text.Length; i++)
			{
				double start = double.MaxValue, end = double.MinValue;
				if (i == 0)
				{
					if (displayBlock.Time != null)
					{
						FindSelectedArea(idx, displayBlock.TimeString.Length, 0, 0.0, displayBlock.Time, ref start, ref end);
						idx += displayBlock.TimeString.Length;
					}
					FindSelectedArea(idx, displayBlock.FormattedName.Length, 0, displayBlock.NameX, displayBlock.Name, ref start,
						ref end);
					idx += displayBlock.FormattedName.Length;
				}
				FindSelectedArea(idx, displayBlock.Text[i].Length, txtOffset, displayBlock.TextX, displayBlock.Text[i], ref start,
					ref end);
				txtOffset += displayBlock.Text[i].Length;

				if (end >= start)
				{
					dc.DrawRectangle(_selectBrush, null,
						new Rect(new Point(start, y), new Point(end, y + _lineHeight)));
				}
				y += _lineHeight;
			}
		}

		private string GetSelectedText()
		{
			var output = new StringBuilder();
			foreach (var block in Blocks)
			{
				if (SelectionEnd < block.CharStart ||
					SelectionStart >= block.CharEnd)
				{
					continue;
				}

				var idx = block.CharStart;
				bool start, end;
				output.Append(GetSelectedText(idx, block.TimeString, output, out start, out end));
				idx += block.TimeString.Length;
				var nick = GetSelectedText(idx, block.FormattedName, output, out start, out end);
				if (start &&
					JustifyNameColumn &&
					block.Entry.Name != null)
				{
					output.Append('<');
				}
				output.Append(nick);
				if (nick.Length > 0 &&
					end &&
					JustifyNameColumn)
				{
					if (block.Entry.Name != null)
					{
						output.Append('>');
					}
					output.Append(' ');
				}
				idx += block.FormattedName.Length;
				output.Append(GetSelectedText(idx, block.Entry.Text, output, out start, out end));
				if (SelectionEnd >= block.CharEnd)
				{
					output.AppendLine();
				}
			}
			return output.ToString();
		}

		private string GetSelectedText(int idx, string s, StringBuilder output, out bool start, out bool end)
		{
			var first = Math.Max(0, SelectionStart - idx);
			var last = Math.Min(s.Length - 1, SelectionEnd - idx);
			start = first == 0;
			end = last >= s.Length - 1;
			return last >= first ? s.Substring(first, last - first + 1) : "";
		}
	}
}