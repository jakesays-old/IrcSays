using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Std.Ui.Logging;

namespace IrcSays.Ui.Logging
{
	public class IrcEntryFormatter : ILogEntryFormatter
	{
		private static readonly Regex _directedMessageDetector =
			new Regex(@"^[^\s:]+:\s+",
				RegexOptions.Compiled |
				RegexOptions.IgnoreCase |
				RegexOptions.CultureInvariant);

		public void FormatEntry(LogEntry entry, string line)
		{
			var text = new StringBuilder();
			var spans = new List<LogEntrySpan>();
			var span = new LogEntrySpan();

			var last = line.Length - 1;
			var idx = 0;
			for (var i = 0; i < line.Length; i++)
			{
				var ichar = (int) line[i];
				if (TextFormatCodes.IsFormatChar(ichar))
				{
					span.End = idx;
					spans.Add(span);
					span.Start = idx;
				}
				switch (ichar)
				{
					case TextFormatCodes.Bold:
						span.FormatOptions ^= SpanFormatOptions.Bold;
						break;
					case TextFormatCodes.Color:
						if (i == last ||
							(line[i + 1] > '9' || line[i + 1] < '0'))
						{
							span.FormatOptions &= ~SpanFormatOptions.Foreground;
							span.FormatOptions &= ~SpanFormatOptions.Background;
							break;
						}
						span.FormatOptions |= SpanFormatOptions.Foreground;
						var c = line[++i] - '0';
						if (i < last &&
							(
								(c == 0 && line[i + 1] >= '0' && line[i + 1] <= '9') ||
								(c == 1 && line[i + 1] >= '0' && line[i + 1] <= '5')
								))
						{
							c *= 10;
							c += line[++i] - '0';
						}
						span.Foreground = (byte) Math.Min(15, c);
						if (i == last ||
							i + 1 == last ||
							line[i + 1] != ',' ||
							line[i + 2] < '0' ||
							line[i + 2] > '9')
						{
							break;
						}
						span.FormatOptions |= SpanFormatOptions.Background;
						++i;
						c = line[++i] - '0';
						if (i < last &&
							(
								(c == 0 && line[i + 1] >= '0' && line[i + 1] <= '9') ||
								(c == 1 && line[i + 1] >= '0' && line[i + 1] <= '5')
								))
						{
							c *= 10;
							c += line[++i] - '0';
						}
						span.Background = (byte) Math.Min(15, c);
						break;
					case TextFormatCodes.Reset:
						span.FormatOptions = SpanFormatOptions.None;
						break;
					case TextFormatCodes.Reverse:
						span.FormatOptions ^= SpanFormatOptions.Reverse;
						break;
					case TextFormatCodes.Underline:
						span.FormatOptions ^= SpanFormatOptions.Underline;
						break;
					default:
						text.Append(line[i]);
						idx++;
						break;
				}
			}
			span.End = idx;
			spans.Add(span);
			entry.Text = text.ToString();

			entry.IsDirectedMessage = _directedMessageDetector.IsMatch(entry.Text);

			entry.Spans = spans.Where(s => s.End > s.Start).ToArray();
			entry.Links = (from Match m in Constants.UrlRegex.Matches(entry.Text)
					 select new UrlPosition
					 {
						 Start = m.Index,
						 End = m.Index + m.Length
					 }).ToArray();
		}
	}
}