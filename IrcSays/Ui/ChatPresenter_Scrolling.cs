using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace IrcSays.Ui
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private int _bufferLines, _scrollPos;
		private bool _isAutoScrolling = true;

		public bool IsAutoScrolling
		{
			get { return _isAutoScrolling; }
		}

		public bool CanHorizontallyScroll
		{
			get { return false; }
			set { }
		}

		public bool CanVerticallyScroll
		{
			get { return true; }
			set { }
		}

		public double ExtentHeight
		{
			get { return _lineHeight * _bufferLines; }
		}

		public double ExtentWidth
		{
			get { return ActualWidth; }
		}

		public ScrollViewer ScrollOwner
		{
			get { return _viewer; }
			set { _viewer = value; }
		}

		public double ViewportHeight
		{
			get { return ActualHeight; }
		}

		public double ViewportWidth
		{
			get { return ActualWidth; }
		}

		public double HorizontalOffset
		{
			get { return 0.0; }
		}

		public double VerticalOffset
		{
			get { return (_bufferLines - _scrollPos) * _lineHeight - ActualHeight; }
		}

		public void LineUp()
		{
			ScrollTo(_scrollPos + 1);
		}

		public void LineDown()
		{
			ScrollTo(_scrollPos - 1);
		}

		public void MouseWheelUp()
		{
			ScrollTo(_scrollPos + SystemParameters.WheelScrollLines);
		}

		public void MouseWheelDown()
		{
			ScrollTo(_scrollPos - SystemParameters.WheelScrollLines);
		}

		public void PageUp()
		{
			ScrollTo(_scrollPos + VisibleLineCount - 1);
		}

		public void PageDown()
		{
			ScrollTo(_scrollPos - VisibleLineCount + 1);
		}

		public void ScrollTo(int pos)
		{
			pos = Math.Max(0, Math.Min(_bufferLines - VisibleLineCount + 1, pos));

			var delta = (pos - _scrollPos) * _lineHeight;
			_scrollPos = pos;

			InvalidateVisual();
			InvalidateScrollInfo();

			_isAutoScrolling = _scrollPos == 0;
		}

		public void SetVerticalOffset(double offset)
		{
			var pos = _bufferLines - (int) ((offset + ViewportHeight) / _lineHeight);
			ScrollTo(pos);
		}

		public void LineLeft()
		{
		}

		public void LineRight()
		{
		}

		public void PageLeft()
		{
		}

		public void PageRight()
		{
		}

		public void MouseWheelLeft()
		{
		}

		public void MouseWheelRight()
		{
		}

		public void SetHorizontalOffset(double offset)
		{
			throw new NotImplementedException();
		}

		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			return Rect.Empty;
		}

		public void ScrollToEnd()
		{
			_scrollPos = 0;
		}

		public void InvalidateScrollInfo()
		{
			if (_viewer != null)
			{
				_viewer.InvalidateScrollInfo();
			}
		}

		public int VisibleLineCount
		{
			get { return _lineHeight == 0.0 ? 0 : (int) Math.Ceiling(ActualHeight / _lineHeight); }
		}
	}
}