using System.Windows;
using IrcSays.Ui;

namespace IrcSays.FileControl
{
	public partial class FileControl : ChatPage
	{
		public static readonly DependencyProperty DescriptionProperty =
			DependencyProperty.Register("Description", typeof (string), typeof (FileControl));

		public string Description
		{
			get { return (string) GetValue(DescriptionProperty); }
			set { SetValue(DescriptionProperty, value); }
		}

		public static readonly DependencyProperty FileSizeProperty =
			DependencyProperty.Register("FileSize", typeof (long), typeof (FileControl));

		public long FileSize
		{
			get { return (long) GetValue(FileSizeProperty); }
			set { SetValue(FileSizeProperty, value); }
		}

		public static readonly DependencyProperty BytesTransferredProperty =
			DependencyProperty.Register("BytesTransferred", typeof (long), typeof (FileControl));

		public long BytesTransferred
		{
			get { return (long) GetValue(BytesTransferredProperty); }
			set { SetValue(BytesTransferredProperty, value); }
		}

		public static readonly DependencyProperty SpeedProperty =
			DependencyProperty.Register("Speed", typeof (long), typeof (FileControl));

		public long Speed
		{
			get { return (long) GetValue(SpeedProperty); }
			set { SetValue(SpeedProperty, value); }
		}

		public static readonly DependencyProperty EstimatedTimeProperty =
			DependencyProperty.Register("EstimatedTime", typeof (int), typeof (FileControl));

		public int EstimatedTime
		{
			get { return (int) GetValue(EstimatedTimeProperty); }
			set { SetValue(EstimatedTimeProperty, value); }
		}

		public static readonly DependencyProperty StatusProperty =
			DependencyProperty.Register("Status", typeof (FileStatus), typeof (FileControl));

		public FileStatus Status
		{
			get { return (FileStatus) GetValue(StatusProperty); }
			set { SetValue(StatusProperty, value); }
		}

		public static readonly DependencyProperty StatusTextProperty =
			DependencyProperty.Register("StatusText", typeof (string), typeof (FileControl));

		public string StatusText
		{
			get { return (string) GetValue(StatusTextProperty); }
			set { SetValue(StatusTextProperty, value); }
		}

		public static readonly DependencyProperty IsDangerousProperty =
			DependencyProperty.Register("IsDangerous", typeof (bool), typeof (FileControl));

		public bool IsDangerous
		{
			get { return (bool) GetValue(IsDangerousProperty); }
			set { SetValue(IsDangerousProperty, value); }
		}

		public static readonly DependencyProperty DccMethodProperty =
			DependencyProperty.Register("DccMethod", typeof (DccMethod), typeof (FileControl));

		public DccMethod DccMethod
		{
			get { return (DccMethod) GetValue(DccMethodProperty); }
			set { SetValue(DccMethodProperty, value); }
		}
	}
}