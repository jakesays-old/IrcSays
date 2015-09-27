// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Std.Ui.Logging
{
	[DebuggerTypeProxy(typeof (IntrusiveListDebugView<>))]
	[DebuggerDisplay("Count = {Count}")]
	public class IntrusiveLinkedList<TEntry> : ICollection<TEntry>, ICollection, IReadOnlyCollection<TEntry>
		where TEntry : IntrusiveListNode<TEntry>
	{
		private object _syncRoot;
		private int _count;
		// This IntrusiveLinkedList is a doubly-Linked circular list.
		private TEntry _head;
		internal int Version { get; set; }

		public IntrusiveLinkedList()
		{
		}

		public IntrusiveLinkedList(IEnumerable<TEntry> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			foreach (var item in collection)
			{
				AddLast(item);
			}
		}

		public TEntry First => _head;

		public TEntry Last => First?.Previous;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (array.Rank != 1)
			{
				throw new ArgumentException("Array rank != 1");
			}

			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("Non zero lower bound specified");
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of range");
			}

			if (array.Length - index < Count)
			{
				throw new ArgumentException("Insufficient space");
			}

			var tArray = array as TEntry[];
			if (tArray != null)
			{
				CopyTo(tArray, index);
			}
			else
			{
				// No need to use reflection to verify that the types are compatible because it isn't 100% correct and we can rely 
				// on the runtime validation during the cast that happens below (i.e. we will get an ArrayTypeMismatchException).
				var objects = array as object[];
				if (objects == null)
				{
					throw new ArgumentException("Invalid array type");
				}
				var node = First;
				try
				{
					if (node != null)
					{
						do
						{
							objects[index++] = node;
							node = node.Next;
						} while (node != First);
					}
				}
				catch (ArrayTypeMismatchException)
				{
					throw new ArgumentException("Invalid array type");
				}
			}
		}

		public int Count
		{
			get { return _count; }
		}

		bool ICollection<TEntry>.IsReadOnly
		{
			get { return false; }
		}

		void ICollection<TEntry>.Add(TEntry value)
		{
			AddLast(value);
		}

		public void Clear()
		{
			var current = First;
			while (current != null)
			{
				var temp = current;
				current = current.Next; // use Next the instead of "next", otherwise it will loop forever
				temp.Invalidate();
			}

			_head = null;
			_count = 0;
			Version++;
		}

		public bool Contains(TEntry value)
		{
			return value.List == this;
		}

		public void CopyTo(TEntry[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (index < 0 ||
				index > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of range");
			}

			if (array.Length - index < Count)
			{
				throw new ArgumentException("Insufficient space");
			}

			var node = First;
			if (node != null)
			{
				do
				{
					array[index++] = node;
					node = node.Next;
				} while (node != First);
			}
		}

		IEnumerator<TEntry> IEnumerable<TEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Remove(TEntry value)
		{
			if (value.List == this)
			{
				InternalRemoveNode(value);
				return true;
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void AddAfter(TEntry node, TEntry newNode)
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			InternalInsertNodeBefore(node.Next, newNode);
			newNode.List = this;
		}

		public void AddBefore(TEntry node, TEntry newNode)
		{
			ValidateNode(node);
			ValidateNewNode(newNode);
			InternalInsertNodeBefore(node, newNode);
			newNode.List = this;
			if (node == First)
			{
				_head = newNode;
			}
		}

		public void AddFirst(TEntry node)
		{
			ValidateNewNode(node);

			if (First == null)
			{
				InternalInsertNodeToEmptyList(node);
			}
			else
			{
				InternalInsertNodeBefore(First, node);
				_head = node;
			}
			node.List = this;
		}

		public void AddLast(TEntry node)
		{
			ValidateNewNode(node);

			if (First == null)
			{
				InternalInsertNodeToEmptyList(node);
			}
			else
			{
				InternalInsertNodeBefore(First, node);
			}
			node.List = this;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public void Remove(IntrusiveListNode<TEntry> node)
		{
			ValidateNode(node);
			InternalRemoveNode(node);
		}

		public void RemoveFirst()
		{
			if (First == null)
			{
				throw new InvalidOperationException("List is empty");
			}
			InternalRemoveNode(First);
		}

		public void RemoveLast()
		{
			if (First == null)
			{
				throw new InvalidOperationException("List is empty");
			}
			InternalRemoveNode(First.Previous);
		}

		private void InternalInsertNodeBefore(TEntry node, TEntry newNode)
		{
			newNode.SetNext(node);

			newNode.SetPrevious(node.Previous);
			node.Previous.SetNext(newNode);
			node.SetPrevious(newNode);

			Version++;
			_count = Count + 1;
		}

		private void InternalInsertNodeToEmptyList(TEntry newNode)
		{
			Debug.Assert(First == null && Count == 0, "LinkedList must be empty when this method is called!");
			newNode.SetNext(newNode);
			newNode.SetPrevious(newNode);
			_head = newNode;
			Version++;
			_count = Count + 1;
		}

		internal void InternalRemoveNode(IntrusiveListNode<TEntry> node)
		{
			Debug.Assert(node.List == this, "Deleting the node from another list!");
			Debug.Assert(First != null, "This method shouldn't be called on empty list!");
			if (node.Next == node)
			{
				Debug.Assert(Count == 1 && First == node, "this should only be true for a list with only one node");
				_head = null;
			}
			else
			{
				node.Next.SetPrevious(node.Previous);
				node.Previous.SetNext(node.Next);

				if (First == node)
				{
					_head = node.Next;
				}
			}
			node.Invalidate();
			_count = Count - 1;
			Version++;
		}

		internal void ValidateNewNode(IntrusiveListNode<TEntry> node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (node.List != null)
			{
				throw new InvalidOperationException("List entry is already attached to a list.");
			}
		}


		internal void ValidateNode(IntrusiveListNode<TEntry> node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			if (node.List != this)
			{
				throw new InvalidOperationException("List entry attached to a different list");
			}
		}

		public struct Enumerator : IEnumerator<TEntry>, IEnumerator
		{
			private readonly IntrusiveLinkedList<TEntry> _list;
			private TEntry _node;
			private readonly int _version;
			private int _index;

			internal Enumerator(IntrusiveLinkedList<TEntry> list)
			{
				_list = list;
				_version = list.Version;
				_node = list.First;
				Current = default(TEntry);
				_index = 0;
			}

			public TEntry Current { get; private set; }

			object IEnumerator.Current
			{
				get
				{
					if (_index == 0 ||
						(_index == _list.Count + 1))
					{
						throw new InvalidOperationException("Invalid operation - enumeration cannot continue");
					}

					return Current;
				}
			}

			public bool MoveNext()
			{
				if (_version != _list.Version)
				{
					throw new InvalidOperationException("Invalid operation - version mismatch");
				}

				if (_node == null)
				{
					_index = _list.Count + 1;
					return false;
				}

				++_index;
				Current = _node;
				_node = _node.Next;
				if (_node == _list.First)
				{
					_node = null;
				}
				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _list.Version)
				{
					throw new InvalidOperationException("Invalid operation - version mismatch");
				}

				Current = default(TEntry);
				_node = _list.First;
				_index = 0;
			}

			public void Dispose()
			{
			}
		}
	}
}