namespace Std.Ui.Logging
{
	public struct LogEntrySpan
	{
		public int Start { get; set; }
		public int End { get; set; }
		public SpanFormatOptions FormatOptions { get; set; }
		public byte Foreground { get; set; }
		public byte Background { get; set; }

		public bool IsBold => FormatOptions.IsSet(SpanFormatOptions.Bold);
		public bool IsReverse => FormatOptions.IsSet(SpanFormatOptions.Reverse);
		public bool IsUnderline => FormatOptions.IsSet(SpanFormatOptions.Underline);
		public bool IsForeground => FormatOptions.IsSet(SpanFormatOptions.Foreground);
		public bool IsBackground => FormatOptions.IsSet(SpanFormatOptions.Background);
	}
}