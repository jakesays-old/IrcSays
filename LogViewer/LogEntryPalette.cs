using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;

namespace Std.Ui.Logging
{
	public class LogEntryPalette : IDictionary<string, Brush>
	{
		private readonly Dictionary<string, Brush> _brushes;
		private readonly Brush _defaultBrush;

		public LogEntryPalette(Brush defaultBrush)
		{
			_brushes = new Dictionary<string, Brush>();
			_defaultBrush = defaultBrush;
		}

		public void Add(string key, Brush brush)
		{
			_brushes.Add(key, brush);
		}

		public Brush this[string key]
		{
			get
			{
				if (_brushes.ContainsKey(key))
				{
					return _brushes[key];
				}
				return _defaultBrush;
			}
			set { _brushes[key] = value; }
		}

		public bool ContainsKey(string key)
		{
			return _brushes.ContainsKey(key);
		}

		public ICollection<string> Keys => _brushes.Keys;

		public bool Remove(string key)
		{
			return _brushes.Remove(key);
		}

		public bool TryGetValue(string key, out Brush value)
		{
			return _brushes.TryGetValue(key, out value);
		}

		public ICollection<Brush> Values => _brushes.Values;

		public void Add(KeyValuePair<string, Brush> item)
		{
			_brushes.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_brushes.Clear();
		}

		public bool Contains(KeyValuePair<string, Brush> item)
		{
			return _brushes.ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<string, Brush>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count => _brushes.Count;

		public bool IsReadOnly => false;

		public bool Remove(KeyValuePair<string, Brush> item)
		{
			return _brushes.Remove(item.Key);
		}

		public IEnumerator<KeyValuePair<string, Brush>> GetEnumerator()
		{
			return _brushes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _brushes.GetEnumerator();
		}
	}
}