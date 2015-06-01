using System;
using System.Collections.Generic;
using System.Text;

namespace IrcSays.Communication.Dcc
{
	/// <summary>
	///     A class responsible for sending and receiving textual messages through a direct connection to another host.
	/// </summary>
	public class DccChat : DccOperation
	{
		private readonly List<byte> _input = new List<byte>(512);

		/// <summary>
		///     Fires when a line of text has been received from the remote host.
		/// </summary>
		public EventHandler<DccChatEventArgs> MessageReceived;

		/// <summary>
		///     Enqueue a line of text to be sent to the remote host.
		/// </summary>
		/// <param name="text">The line of text to send.</param>
		public void QueueMessage(string text)
		{
			var data = Encoding.UTF8.GetBytes(text + "\u000d\u000a");
			QueueWrite(data, 0, data.Length);
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			for (var i = 0; i < count; i++)
			{
				switch (buffer[i])
				{
					case 0xa:
						var input = Encoding.UTF8.GetString(_input.ToArray());
						_input.Clear();
						RaiseEvent(MessageReceived, new DccChatEventArgs(input));
						break;
					case 0xd:
						break;
					default:
						_input.Add(buffer[i]);
						break;
				}
			}
		}
	}
}