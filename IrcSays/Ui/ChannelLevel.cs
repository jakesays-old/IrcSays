using System;

namespace IrcSays.Ui
{
	[Flags]
	public enum ChannelLevel
	{
		Normal = 0,
		Voice = 1,
		HalfOp = 2,
		Op = 4
	}
}