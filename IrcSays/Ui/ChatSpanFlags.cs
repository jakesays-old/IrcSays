using System;

namespace IrcSays.Ui
{
	[Flags]
	public enum ChatSpanFlags
	{
		None,
		Bold = 1,
		Reverse = 2,
		Underline = 4,
		Foreground = 8,
		Background = 16
	}
}