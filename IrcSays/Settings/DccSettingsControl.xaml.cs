using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Navigation;
using IrcSays.Application;
using IrcSays.Ui;

namespace IrcSays.Settings
{
	public partial class DccSettingsControl : UserControl
	{
		public DccSettingsControl()
		{
			InitializeComponent();
		}

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			var parentWindow = new WindowInteropHelper(Window.GetWindow(this)).Handle;

			var path = FileSystemObjectSelector.SelectFolder(parentWindow,
				"Select the download location for receiving files.",
				App.Settings.Current.Dcc.DownloadFolder);
			if (!string.IsNullOrEmpty(path))
			{
				App.Settings.Current.Dcc.DownloadFolder = path;
				txtDownloadFolder.Text = path;
			}
		}
	}
}
