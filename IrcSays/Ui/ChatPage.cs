using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using IrcSays.Communication.Irc;

namespace IrcSays.Ui
{
	public class ChatPage : UserControl, IDisposable
	{
		public IrcSession Session { get; protected set; }
		public IrcTarget Target { get; protected set; }
		public ChatPageType Type { get; protected set; }
		public string Id { get; protected set; }

		public bool IsServer
		{
			get { return Type == ChatPageType.Server; }
		}

		public virtual void Dispose()
		{
		}

		public static readonly DependencyProperty UIBackgroundProperty = DependencyProperty.Register("UIBackground",
			typeof (SolidColorBrush), typeof (ChatPage));

		public SolidColorBrush UIBackground
		{
			get { return (SolidColorBrush) GetValue(UIBackgroundProperty); }
			set { SetValue(UIBackgroundProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof (string), typeof (ChatPage));

		public string Header
		{
			get { return (string) GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register("Title", typeof (string), typeof (ChatPage));

		public string Title
		{
			get { return (string) GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty ProgressProperty =
			DependencyProperty.Register("Progress", typeof (double), typeof (ChatPage));

		public double Progress
		{
			get { return (double) GetValue(ProgressProperty); }
			set { SetValue(ProgressProperty, value); }
		}

		public static readonly DependencyProperty NotifyStateProperty =
			DependencyProperty.Register("NotifyState", typeof (NotifyState), typeof (ChatPage));

		public NotifyState NotifyState
		{
			get { return (NotifyState) GetValue(NotifyStateProperty); }
			set { SetValue(NotifyStateProperty, value); }
		}

		public static readonly DependencyProperty IsCloseableProperty =
			DependencyProperty.Register("IsCloseable", typeof (bool), typeof (ChatPage));

		public bool IsCloseable
		{
			get { return (bool) GetValue(IsCloseableProperty); }
			set
			{
				SetValue(IsCloseableProperty, value);
				CommandManager.InvalidateRequerySuggested();
			}
		}

		public ChatPage()
		{
			Header = Title = "";
			Loaded += new RoutedEventHandler(ChatPage_Loaded);
		}

		public ChatPage(ChatPageType type, IrcSession session, IrcTarget target, string id)
			: this()
		{
			Type = type;
			Session = session;
			Target = target;
			Id = id;
			IsCloseable = true;
		}

		public virtual bool CanClose()
		{
			return true;
		}

		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);

			var bgBinding = new Binding();
			bgBinding.Source = Window.GetWindow(this);
			bgBinding.Path = new PropertyPath("UIBackground");
			bgBinding.Mode = BindingMode.OneWay;
			SetBinding(UIBackgroundProperty, bgBinding);
		}

		private void ChatPage_Loaded(object sender, RoutedEventArgs e)
		{
			if (IsVisible)
			{
				NotifyState = NotifyState.None;
			}
		}
	}
}