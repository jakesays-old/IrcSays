using System;
using System.Linq;

namespace Std.Ui.Logging
{
	public class LogEntry
	{
		public DateTime Time { get; set; }
		public string ColorKey { get; set; }
		public int NameColorKey { get; set; }
		public string Name { get; set; }
		public string RawText { get; set; }
		public string Text { get; set; }
		public LogEntryMarker Marker { get; set; }
		public LogEntrySpan[] Spans { get; set; }
		public UrlPosition[] Links { get; set; }
		
		public bool IsDirectedMessage { get; set; }

		public LogEntry(string colorKey, DateTime time, int nameColorKey, 
			string name, string text, LogEntryMarker decoration)
		{
			ColorKey = colorKey;
			Time = time;
			NameColorKey = nameColorKey;
			Name = name;
			Marker = decoration;
			RawText = text;
		}

		public LogEntry(string colorKey, int nickHashCode, string nick, string text, LogEntryMarker decoration)
			: this(colorKey, DateTime.Now, nickHashCode, nick, text, decoration)
		{
		}

		public LogEntrySpan GetSpan(int idx)
		{
			return Spans.Where(s => idx >= s.Start && idx < s.End).FirstOrDefault();
		}
	}
}