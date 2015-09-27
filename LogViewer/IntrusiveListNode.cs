namespace Std.Ui.Logging
{
	public abstract class IntrusiveListNode<TEntry>
		where TEntry : IntrusiveListNode<TEntry>
	{
		private IntrusiveListNode<TEntry> _next;
		private IntrusiveListNode<TEntry> _prev;

		internal void SetNext(TEntry value)
		{
			_next = value;
		}

		internal void SetPrevious(TEntry value)
		{
			_prev = value;
		}
		internal IntrusiveListNode(IntrusiveLinkedList<TEntry> list)
		{
			List = list;
		}

		protected IntrusiveListNode()
		{
		}

		public IntrusiveLinkedList<TEntry> List { get; internal set; }

		public TEntry Next => (TEntry) (_next == null || _next == List.First ? null : _next);

		public TEntry Previous => (TEntry) (_prev == null || this == List.First ? null : _prev);

		internal void Invalidate()
		{
			List = null;
			_next = null;
			_prev = null;
		}
	}
}