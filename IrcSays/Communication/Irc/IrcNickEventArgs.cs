namespace IrcSays.Communication.Irc
{
	/// <summary>
	///     Provides event arguments describing a nickname change event.
	/// </summary>
	public sealed class IrcNickEventArgs : IrcEventArgs
	{
		/// <summary>
		///     Gets the user's old nickname.
		/// </summary>
		public string OldNickname { get; private set; }

		/// <summary>
		///     Gets the user's new nickname.
		/// </summary>
		public string NewNickname { get; private set; }

		internal IrcNickEventArgs(IrcMessage message)
			: base(message)
		{
			var peer = message.From as IrcPeer;
			OldNickname = peer != null ? peer.Nickname : null;
			NewNickname = message.Parameters.Count > 0 ? message.Parameters[0] : null;
		}
	}
}