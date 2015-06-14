using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using IrcSays.Application;
using IrcSays.Interop;

namespace IrcSays.Ui
{
	public partial class ChannelWindow : Window
	{
		public static readonly DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof (SolidColorBrush), typeof (ChannelWindow));

		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush) GetValue(UIBackgroundProperty); }
			set { SetValue(UIBackgroundProperty, value); }
		}

		private const double ResizeHeight = 4.0;
		private const double ResizeWidth = 6.0;
		private IntPtr _hWnd;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			var state = App.Settings.Current.Windows.States[Page.Id];
			if (!string.IsNullOrEmpty(state.Placement))
			{
				WindowHelper.Load(this, state.Placement);
			}
			if (!IsActive)
			{
				WindowHelper.FlashWindow(this);
			}

			var hwndSrc = PresentationSource.FromVisual(this) as HwndSource;
			hwndSrc.AddHook(WndProc);
			_hWnd = hwndSrc.Handle;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WindowFlags.WM_NCHITTEST)
			{
				var p = new Point(lParam.ToInt32() & 0xFFFF, lParam.ToInt32() >> 16);
				p = PointFromScreen(p);

				var htResult = HitTestValues.HTCLIENT;

				if ((ActualWidth - p.X <= ResizeWidth * 2.0 && ActualHeight - p.Y <= ResizeHeight) ||
					(ActualWidth - p.X <= ResizeWidth && ActualHeight - p.Y <= ResizeHeight * 2))
				{
					htResult = HitTestValues.HTBOTTOMRIGHT;
				}
				else if (p.X <= ResizeWidth)
				{
					if (p.Y <= ResizeHeight)
					{
						htResult = HitTestValues.HTTOPLEFT;
					}
					else if (ActualHeight - p.Y <= ResizeHeight)
					{
						htResult = HitTestValues.HTBOTTOMLEFT;
					}
					else
					{
						htResult = HitTestValues.HTLEFT;
					}
				}
				else if (ActualWidth - p.X <= ResizeWidth)
				{
					if (p.Y <= ResizeHeight)
					{
						htResult = HitTestValues.HTTOPRIGHT;
					}
					else if (ActualHeight - p.Y <= ResizeHeight)
					{
						htResult = HitTestValues.HTBOTTOMRIGHT;
					}
					else
					{
						htResult = HitTestValues.HTRIGHT;
					}
				}
				else if (p.Y <= ResizeHeight)
				{
					htResult = HitTestValues.HTTOP;
				}
				else if (ActualHeight - p.Y <= ResizeHeight)
				{
					htResult = HitTestValues.HTBOTTOM;
				}
				else if (p.Y <= grdRoot.RowDefinitions[0].Height.Value &&
						p.X <= grdRoot.ColumnDefinitions[0].ActualWidth)
				{
					htResult = HitTestValues.HTCAPTION;
				}

				handled = true;
				return (IntPtr) htResult;
			}
			if (msg == WindowFlags.WM_GETMINMAXINFO)
			{
				WindowHelper.GetMinMaxInfo(this, _hWnd, lParam);
				handled = true;
			}

			return IntPtr.Zero;
		}
	}
}