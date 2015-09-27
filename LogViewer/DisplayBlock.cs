using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Std.Ui.Logging
{
	public class DisplayBlock : IntrusiveListNode<DisplayBlock>
	{
		public long DrawCounter { get; set; }

		public LogEntry Entry { get; set; }
		public Brush Foreground { get; set; }

		public string TimeString { get; set; }
		public string FormattedName { get; set; }

		public string NameText { get; set; }

		public TextLine Time { get; set; }
		public TextLine Name { get; set; }
		public TextLine[] Text { get; set; }

		public int CharStart { get; set; }
		public int CharEnd { get; set; }
		public double Y { get; set; }
		public double NameX { get; set; }
		public double TextX { get; set; }
		public double Height { get; set; }
	}
}