using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using IrcSays.Communication.Irc;

namespace IrcSays.Ui
{
	public class NicknameList : KeyedCollection<string, NicknameItem>, INotifyCollectionChanged
	{
		public NicknameList()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void AddRange(IEnumerable<string> nicks)
		{
			foreach (var item in nicks.Select(n => new NicknameItem(n)))
			{
				Add(item);
			}
		}

		public void Add(string nick)
		{
			Add(new NicknameItem(nick));
		}

		public void ChangeNick(string oldNick, string newNick)
		{
			var item = this[oldNick];
			if (item != null &&
				!Contains(newNick))
			{
				var idx = IndexOf(item);
				ChangeItemKey(item, newNick);
				item.Nickname = newNick;
				RefreshItem(idx);
			}
		}

		public void ProcessMode(IrcChannelMode mode)
		{
			var mask = ChannelLevel.Normal;
			switch (mode.Mode)
			{
				case 'o':
					mask = ChannelLevel.Op;
					break;
				case 'h':
					mask = ChannelLevel.HalfOp;
					break;
				case 'v':
					mask = ChannelLevel.Voice;
					break;
			}

			if (mask != ChannelLevel.Normal &&
				Contains(mode.Parameter))
			{
				var item = this[mode.Parameter];
				if (item != null)
				{
					item.Level = mode.Set 
						? item.Level | mask 
						: item.Level & ~mask;
					var idx = IndexOf(item);
					RefreshItem(idx);
				}
			}
		}

		protected override string GetKeyForItem(NicknameItem item)
		{
			return item.Nickname;
		}

		protected override void SetItem(int index, NicknameItem item)
		{
			var oldItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
		}

		protected override void InsertItem(int index, NicknameItem item)
		{
			base.InsertItem(index, item);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void RemoveItem(int index)
		{
			var item = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
		}

		private void RefreshItem(int idx)
		{
			var item = this[idx];
			SetItem(idx, new NicknameItem(""));
			SetItem(idx, item);
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var handler = CollectionChanged;
			if (handler != null)
			{
				handler(this, args);
			}
		}
	}
}