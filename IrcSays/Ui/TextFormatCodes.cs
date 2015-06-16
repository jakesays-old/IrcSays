namespace IrcSays.Ui
{
	public static class FormattingCodes
	{
		public const int Bold = 2;
		public const int Color = 3;
		public const int Reset = 15;
		public const int Reverse = 22;
		public const int Underline = 31;

		public static bool IsFormatChar(int ch)
		{
			if (ch == Bold ||
				ch == Color ||
				ch == Reset ||
				ch == Reverse ||
				ch == Underline)
			{
				return true;
			}

			return false;
		}
	}
}