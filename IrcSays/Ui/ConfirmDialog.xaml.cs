using System.Windows;

namespace IrcSays.Ui
{
	public partial class ConfirmDialog : Window
	{
		public ConfirmDialog(string title, string message, bool showDontAskAgainCheckbox)
		{
			InitializeComponent();

			this.Title = title;
			txtMessage.Text = message;
			if (!showDontAskAgainCheckbox)
			{
				chkDontAskAgain.Visibility = Visibility.Collapsed;
			}
		}

		public bool IsDontAskAgainChecked { get { return chkDontAskAgain.IsChecked.Value; } }

		private void btnYes_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}

		private void btnNo_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			chkDontAskAgain.IsChecked = false;
			this.Close();
		}
	}
}
