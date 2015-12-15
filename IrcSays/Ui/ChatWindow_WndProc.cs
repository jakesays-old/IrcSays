using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using IrcSays.Application;
using IrcSays.Interop;
using Point = System.Windows.Point;

namespace IrcSays.Ui
{
	public partial class ChatWindow : Window
	{
		public static readonly DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof (SolidColorBrush), typeof (ChatWindow));

		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush) GetValue(UIBackgroundProperty); }
			set { SetValue(UIBackgroundProperty, value); }
		}

		private const double ResizeHeight = 4.0;
		private const double ResizeWidth = 6.0;
		private bool _isInModalDialog = false;
		private bool _isShuttingDown = false;
		private NotifyIcon _notifyIcon;
		private WindowState _oldWindowState = WindowState.Normal;
		private IntPtr _hWnd;

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			WindowHelper.Load(this, App.Settings.Current.Windows.Placement);

			var hwndSrc = PresentationSource.FromVisual(this) as HwndSource;
			hwndSrc.AddHook(WndProc);
			_hWnd = hwndSrc.Handle;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WindowFlags.WM_NCHITTEST:
				{
					var x = (short) (lParam.ToInt32() & 0xFFFF);
					var p = new Point(x, lParam.ToInt32() >> 16);
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

					var s = InputHitTest(p) as StackPanel;
					if (s != null &&
						s.TemplatedParent is TabControl)
					{
						htResult = HitTestValues.HTCAPTION;
					}

					handled = true;
					return (IntPtr) htResult;
				}
				case WindowFlags.WM_GETMINMAXINFO:
				{
					WindowHelper.GetMinMaxInfo(this, _hWnd, lParam);
					handled = true;
				}
					break;
				case WindowFlags.WM_QUERYENDSESSION:
					_isShuttingDown = true;
					handled = true;
					return (IntPtr) 1;
				case WindowFlags.WM_ENDSESSION:
					if (wParam == (IntPtr) 0)
					{
						_isShuttingDown = false;
					}
					break;
			}

			return IntPtr.Zero;
		}

		protected override void OnActivated(EventArgs e)
		{
			_activeWindow = this;
			Opacity = App.Settings.Current.Windows.ActiveOpacity;

			base.OnActivated(e);
		}

		protected override void OnDeactivated(EventArgs e)
		{
			if (_activeWindow == this)
			{
				_activeWindow = null;
			}

			if (OwnedWindows.Count == 0 &&
				!_isInModalDialog)
			{
				Opacity = App.Settings.Current.Windows.InactiveOpacity;
			}

			base.OnDeactivated(e);
		}

		private static readonly Lazy<Icon> _appIcon = new Lazy<Icon>(() =>
		{
			using (var stream = typeof(App).Assembly.GetManifestResourceStream(
				"IrcSays.Resources.App.ico"))
			{
				return new Icon(stream);
			}
		});

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == WindowState.Minimized &&
				App.Settings.Current.Windows.MinimizeToSysTray)
			{
				if (_notifyIcon == null)
				{
					_notifyIcon = new NotifyIcon(this, _appIcon.Value);
					_notifyIcon.DoubleClicked += (sender, args) =>
					{
						Show();
						WindowState = _oldWindowState;
						Activate();
						_notifyIcon.Hide();
					};
					_notifyIcon.RightClicked += (sender, args) =>
					{
						var menu = FindResource("NotifyMenu") as ContextMenu;
						if (menu != null)
						{
							menu.IsOpen = true;
							CommandManager.InvalidateRequerySuggested();
						}
					};
				}
				Hide();
				_notifyIcon.Show();
			}

			base.OnStateChanged(e);
		}

		private bool ConfirmQuit(string text, string caption)
		{
			_isInModalDialog = true;
			var dontAskAgain = true;
			var result = App.Confirm(this, text, caption, ref dontAskAgain);
			if (dontAskAgain)
			{
				App.Settings.Current.Windows.SuppressWarningOnQuit = true;
			}
			_isInModalDialog = false;
			return result;
		}
	}
}