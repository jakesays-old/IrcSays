namespace IrcSays.Ui
{
	public struct ChatSpan
	{
		public int Start;
		public int End;
		public ChatSpanFlags Flags;
		public byte Foreground;
		public byte Background;
	}
}