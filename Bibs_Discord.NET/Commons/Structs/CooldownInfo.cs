namespace Bibs_Discord_dotNET.Commons.Structs
{
	/// Info for a command cooldown
	public struct CooldownInfo
	{
		/// The user ID that this cooldown applies to
		public ulong UserId { get; }
		/// The hash code for what command this applies to
		public int CommandHashCode { get; }

		public CooldownInfo(ulong userId, int commandHashCode)
		{
			UserId = userId;
			CommandHashCode = commandHashCode;
		}
	}
}
