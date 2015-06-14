using System;
using System.Runtime.InteropServices;

namespace IrcSays.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FLASHINFO
	{
		public int cbSize;
		public IntPtr hWnd;
		public int dwFlags;
		public uint uCount;
		public int dwTimeout;
	}
}