using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace IrcSays.Ui
{
	public class DisplayBlock
	{
		public long DrawCounter { get; set; }

		public ChatLine Source { get; set; }
		public Brush Foreground { get; set; }

		public string TimeString { get; set; }
		public string FormattedNick { get; set; }

		public string NickText { get; set; }

		public TextLine Time { get; set; }
		public TextLine Nick { get; set; }
		public TextLine[] Text { get; set; }

		public int CharStart { get; set; }
		public int CharEnd { get; set; }
		public double Y { get; set; }
		public double NickX { get; set; }
		public double TextX { get; set; }
		public double Height { get; set; }
	}
}