// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using IrcSays.Utility;

namespace IrcSays.Preferences
{
	public abstract class PropertyService : IPropertyService
	{
		private Properties _properties;

		protected PropertyService()
		{
		}

		protected void Initialize(Properties properties)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			_properties = properties;
		}

		public TValue Get<TValue>(string key, TValue defaultValue)
		{
			return _properties.Get(key, defaultValue);
		}

		public TValue Get<TValue>(string key, Func<TValue> defaultFactory)
		{
			return _properties.Get(key, defaultFactory);
		}

		public Properties NestedProperties(string key)
		{
			return _properties.NestedProperties(key);
		}

		public void SetNestedProperties(string key, Properties nestedProperties)
		{
			_properties.SetNestedProperties(key, nestedProperties);
		}

		public bool Contains(string key)
		{
			return _properties.Contains(key);
		}

		public T Set<T>(string key, T value)
		{
			return _properties.Set(key, value);
		}

		public IReadOnlyList<T> GetList<T>(string key)
		{
			return _properties.GetList<T>(key);
		}

		public void SetList<T>(string key, IEnumerable<T> value)
		{
			_properties.SetList(key, value);
		}

		public void Remove(string key)
		{
			_properties.Remove(key);
		}

		public event PropertyChangedEventHandler PropertyChanged
		{
			add { _properties.PropertyChanged += value; }
			remove { _properties.PropertyChanged -= value; }
		}

		public abstract bool IsNew { get; }

		public Properties RootContainer
		{
			get { return _properties; }
		}

		public virtual void Save()
		{
		}

		public virtual Properties LoadExtraProperties(string key)
		{
			return new Properties();
		}

		public virtual void SaveExtraProperties(string key, Properties p)
		{
		}
	}
}