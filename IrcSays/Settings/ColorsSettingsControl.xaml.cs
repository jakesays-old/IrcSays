using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IrcSays.Settings
{
	public partial class ColorsSettingsControl : UserControl
	{
		public ColorsSettingsControl()
		{
			InitializeComponent();
			Picker.SelectedColorChanged += PickerOnSelectedColorChanged;
			AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClicked));
		}

		private void PickerOnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> args)
		{
			if (_currentColorButton != null)
			{
				_currentColorButton.Foreground = new SolidColorBrush(args.NewValue);
				Picker.IsOpen = false;
			}
		}

		private Button _currentColorButton;

		private void OnButtonClicked(object sender, RoutedEventArgs e)
		{
			var colorButton = e.OriginalSource as Button;
			if (colorButton == null)
			{
				return;
			}
			_currentColorButton = null;
			var color = ((SolidColorBrush) colorButton.Foreground).Color;
			Picker.SelectedColor = color;
			Picker.IsOpen = true;
			_currentColorButton = colorButton;
		}
	}
}
