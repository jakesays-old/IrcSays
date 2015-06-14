using System;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace IrcSays.Utility
{
	/// <summary>
	/// Various convenience extension methods for generic lists
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Add an item to a list if the item is not null
		/// </summary>
		/// <typeparam name="TItem">List item type</typeparam>
		/// <param name="list">List to add the item to</param>
		/// <param name="item">The item to be added</param>
		public static void AddIfNotNull<TItem>(this IList<TItem> list, TItem item)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}

			if (item != null)
			{
				list.Add(item);
			}
		}
		
		/// <summary>
		/// Determines if the list is null or empty.
		/// </summary>
		/// <typeparam name="TItem">The list item type</typeparam>
		/// <param name="list">List to test for nullity</param>
		/// <returns>true if the list is null or has no entries, false otherwise.</returns>
		[ContractAnnotation("list:null => true")]
		public static bool IsNullOrEmpty<TItem>(this List<TItem> list)
 		{
 			return list == null || list.Count == 0;
 		}

		[ContractAnnotation("list:null => false")]
		public static bool NotNullOrEmpty<TItem>(this List<TItem> list)
		{
			return list != null && list.Count > 0;
		}

		[ContractAnnotation("list:null => false")]
		public static bool NotNullOrEmpty<TItem>(this IReadOnlyList<TItem> list)
		{
			return list != null && list.Count > 0;
		}

		/// <summary>
		/// Determines if the list is null or empty.
		/// </summary>
		/// <typeparam name="TItem">The list item type</typeparam>
		/// <param name="list">List to test for nullity</param>
		/// <returns>true if the list is null or has no entries, false otherwise.</returns>
		[ContractAnnotation("list:null => true")]
		public static bool IsNullOrEmpty<TItem>(this IReadOnlyList<TItem> list)
		{
			return list == null || list.Count == 0;
		}
	}
}