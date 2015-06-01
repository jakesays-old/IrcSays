using System;

namespace IrcSays.Communication.Dcc
{
	/// <summary>
	///     Provides event arguments for a DCC chat message event.
	/// </summary>
	public class DccChatEventArgs : EventArgs
	{
		public string Text { get; private set; }

		internal DccChatEventArgs(string text)
		{
			Text = text;
		}
	}
}