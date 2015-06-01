using System;

namespace IrcSays.Communication.Network
{
	[Serializable]
	public class SocksException : Exception
	{
		public SocksException(string message)
			: base(message)
		{
		}
	}
}