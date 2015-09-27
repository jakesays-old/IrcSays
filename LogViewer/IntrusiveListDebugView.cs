// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Std.Ui.Logging
{
	internal sealed class IntrusiveListDebugView<TEntry>
	{
		private readonly ICollection<TEntry> _collection;

		public IntrusiveListDebugView(ICollection<TEntry> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			_collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public TEntry[] Items
		{
			get
			{
				var items = new TEntry[_collection.Count];
				_collection.CopyTo(items, 0);
				return items;
			}
		}
	}
}