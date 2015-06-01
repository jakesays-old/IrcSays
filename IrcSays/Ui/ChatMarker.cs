using System;

namespace IrcSays.Ui
{
	[Flags]
	public enum ChatMarker
	{
		None = 0,
		NewMarker = 1,
		OldMarker = 2,
		Attention = 4
	}
}
