using System;

namespace IrcSays.Communication.Network
{
	/// <summary>
	///     Encapsulates a STUN-related error.
	/// </summary>
	[Serializable]
	public class StunException : Exception
	{
		public StunException()
		{
		}

		public StunException(string message)
			: base(message)
		{
		}
	}
}