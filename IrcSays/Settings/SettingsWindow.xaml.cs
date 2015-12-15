using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IrcSays.Application;
using IrcSays.Services;

namespace IrcSays.Settings
{
	public partial class SettingsWindow : Window
	{
		private IPreferenceService context = new PreferenceService();
		public SettingsWindow()
		{
			InitializeComponent();
			this.DataContext = context;
			


			if (lstCategories.SelectedIndex < 0)
			{
				lstCategories.SelectedIndex = 0;
			}

			AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
			AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText));
			AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText));
		}

		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			App.Settings.Save();
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			App.Settings.Load();
			Close();
		}

		private void lstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			for (var i = 0; i < grdSettings.Children.Count; i++)
			{
				grdSettings.Children[i].Visibility = i == lstCategories.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			DependencyObject parent = e.OriginalSource as UIElement;
			while (parent != null &&
					!(parent is TextBox))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			if (parent != null)
			{
				var textBox = (TextBox) parent;
				if (!textBox.IsKeyboardFocusWithin &&
					!textBox.AcceptsReturn)
				{
					textBox.Focus();
					e.Handled = true;
				}
			}
		}

		private void SelectAllText(object sender, RoutedEventArgs e)
		{
			var textBox = e.OriginalSource as TextBox;
			if (textBox != null &&
				!textBox.AcceptsReturn)
			{
				textBox.SelectAll();
			}
		}
	}
}