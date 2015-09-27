using System;

namespace Std.Ui.Logging
{
	[Flags]
	public enum SpanFormatOptions
	{
		None,
		Bold = 1,
		Reverse = 2,
		Underline = 4,
		Foreground = 8,
		Background = 16
	}

	public static class SpanFormatOptionsExtensions
	{
		public static bool IsSet(this SpanFormatOptions value, SpanFormatOptions candidate)
		{
			if ((value & candidate) == candidate)
			{
				return true;
			}

			return false;
		}

		public static bool IsClear(this SpanFormatOptions value, SpanFormatOptions candidate)
		{
			if ((value & candidate) == candidate)
			{
				return false;
			}

			return true;
		}

		public static SpanFormatOptions Set(this SpanFormatOptions value, SpanFormatOptions setValue)
		{
			var result = value | setValue;
			return result;
		}

		public static SpanFormatOptions Clear(this SpanFormatOptions value, SpanFormatOptions clearValue)
		{
			var result = value & ~clearValue;
			return result;
		}
	}
}