using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit;

namespace IrcSays.Settings
{
	/// <summary>
	/// Simple extension to the xceed color picker.
	/// Its purpose is to hide the toggle button.
	/// </summary>
	public class ColorPickerEx : ColorPicker
	{
		private const string PART_ColorPickerToggleButton = "PART_ColorPickerToggleButton";

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var button = Template.FindName(PART_ColorPickerToggleButton, this) as ToggleButton;
			button.Visibility = Visibility.Collapsed;
		}
	}
}