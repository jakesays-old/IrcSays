using System;

namespace Std.Ui.Logging
{
	[Flags]
	public enum LogEntryMarker
	{
		None = 0,
		NewMarker = 1,
		OldMarker = 2,
		Attention = 4
	}
}
