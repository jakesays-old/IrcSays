using System.Windows;
using System.Windows.Controls;
using IrcSays.Application;

namespace IrcSays.Settings
{
	public partial class BufferSettingsControl : UserControl
	{
		public BufferSettingsControl()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			App.BrowseTo(App.LoggingPathBase);
		}
	}
}
