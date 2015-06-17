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
		private readonly Properties _properties;

		/// <summary>
		///     Initializes the service for unit-testing (reset properties to an empty property container).
		///     Use <c>SD.InitializeForUnitTests()</c> instead, that initializes the property service and more.
		/// </summary>
		protected PropertyService()
		{
			_properties = new Properties();
		}

		protected PropertyService(Properties properties)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			_properties = properties;
		}

		/// <inheritdoc cref="Properties.Get{T}(string, T)" />
		public T Get<T>(string key, T defaultValue)
		{
			return _properties.Get(key, defaultValue);
		}

		/// <inheritdoc cref="Properties.NestedProperties" />
		public Properties NestedProperties(string key)
		{
			return _properties.NestedProperties(key);
		}

		/// <inheritdoc cref="Properties.SetNestedProperties" />
		public void SetNestedProperties(string key, Properties nestedProperties)
		{
			_properties.SetNestedProperties(key, nestedProperties);
		}

		/// <inheritdoc cref="Properties.Contains" />
		public bool Contains(string key)
		{
			return _properties.Contains(key);
		}

		/// <inheritdoc cref="Properties.Set{T}(string, T)" />
		public void Set<T>(string key, T value)
		{
			_properties.Set(key, value);
		}

		/// <inheritdoc cref="Properties.GetList" />
		public IReadOnlyList<T> GetList<T>(string key)
		{
			return _properties.GetList<T>(key);
		}

		/// <inheritdoc cref="Properties.SetList" />
		public void SetList<T>(string key, IEnumerable<T> value)
		{
			_properties.SetList(key, value);
		}

		/// <inheritdoc cref="Properties.Remove" />
		public void Remove(string key)
		{
			_properties.Remove(key);
		}

		public event PropertyChangedEventHandler PropertyChanged
		{
			add { _properties.PropertyChanged += value; }
			remove { _properties.PropertyChanged -= value; }
		}

		public Properties MainPropertiesContainer
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