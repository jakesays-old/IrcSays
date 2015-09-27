using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Std.Ui.Logging
{
	public interface ILogEntryRenderer
	{
		TextRun GetTextRun(int idx);
		IEnumerable<TextLine> RenderEntry(string text, 
			LogEntry entry, 
			double width, 
			Brush foreground, 
			Brush background, 
			TextWrapping textWrapping, 
			bool forceBackground = false);
	}
}