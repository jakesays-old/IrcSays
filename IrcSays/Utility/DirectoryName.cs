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
using System.ComponentModel;
using System.IO;

namespace IrcSays.Utility
{
	/// <summary>
	///     Represents a path to a directory.
	///     The equality operator is overloaded to compare for path equality (case insensitive, normalizing paths with '..\')
	/// </summary>
	[TypeConverter(typeof (DirectoryNameConverter))]
	public sealed class DirectoryName : PathName
	{
		public DirectoryName(string path)
			: base(path)
		{
		}

		public static explicit operator DirectoryName(string dir)
		{
			return new DirectoryName(dir);
		}

		/// <summary>
		///     Creates a DirectoryName instance from the string.
		///     It is valid to pass null or an empty string to this method (in that case, a null reference will be returned).
		/// </summary>
		public static DirectoryName Create(string directoryName)
		{
			if (string.IsNullOrEmpty(directoryName))
			{
				return null;
			}
			return new DirectoryName(directoryName);
		}

		/// <summary>
		///     Combines this directory name with a relative path.
		/// </summary>
		public DirectoryName Combine(DirectoryName relativePath)
		{
			if (relativePath == null)
			{
				return null;
			}
			return Create(Path.Combine(NormalizedPath, relativePath));
		}

		/// <summary>
		///     Combines this directory name with a relative path.
		/// </summary>
		public FileName Combine(FileName relativePath)
		{
			if (relativePath == null)
			{
				return null;
			}
			return FileName.Create(Path.Combine(NormalizedPath, relativePath));
		}

		/// <summary>
		///     Combines this directory name with a relative path.
		/// </summary>
		public FileName CombineFile(string relativeFileName)
		{
			if (relativeFileName == null)
			{
				return null;
			}
			return FileName.Create(Path.Combine(NormalizedPath, relativeFileName));
		}

		/// <summary>
		///     Combines this directory name with a relative path.
		/// </summary>
		public DirectoryName CombineDirectory(string relativeDirectoryName)
		{
			if (relativeDirectoryName == null)
			{
				return null;
			}
			return Create(Path.Combine(NormalizedPath, relativeDirectoryName));
		}

		/// <summary>
		///     Converts the specified absolute path into a relative path (relative to <c>this</c>).
		/// </summary>
		public DirectoryName GetRelativePath(DirectoryName path)
		{
			if (path == null)
			{
				return null;
			}
			return Create(FileUtility.GetRelativePath(NormalizedPath, path));
		}

		/// <summary>
		///     Converts the specified absolute path into a relative path (relative to <c>this</c>).
		/// </summary>
		public FileName GetRelativePath(FileName path)
		{
			if (path == null)
			{
				return null;
			}
			return FileName.Create(FileUtility.GetRelativePath(NormalizedPath, path));
		}

		/// <summary>
		///     Gets the directory name as a string, including a trailing backslash.
		/// </summary>
		public string ToStringWithTrailingBackslash()
		{
			if (NormalizedPath.EndsWith("\\", StringComparison.Ordinal))
			{
				return NormalizedPath; // trailing backslash exists in normalized version for root of drives ("C:\")
			}
			return NormalizedPath + "\\";
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as DirectoryName);
		}

		public bool Equals(DirectoryName other)
		{
			if (other != null)
			{
				return string.Equals(NormalizedPath, other.NormalizedPath, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(NormalizedPath);
		}

		public static bool operator ==(DirectoryName left, DirectoryName right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}
			if (ReferenceEquals(left, null) ||
				ReferenceEquals(right, null))
			{
				return false;
			}
			return left.Equals(right);
		}

		public static bool operator !=(DirectoryName left, DirectoryName right)
		{
			return !(left == right);
		}

		[Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
		public static bool operator ==(DirectoryName left, string right)
		{
			return (string) left == right;
		}

		[Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
		public static bool operator !=(DirectoryName left, string right)
		{
			return (string) left != right;
		}

		[Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
		public static bool operator ==(string left, DirectoryName right)
		{
			return left == (string) right;
		}

		[Obsolete("Warning: comparing DirectoryName with string results in case-sensitive comparison")]
		public static bool operator !=(string left, DirectoryName right)
		{
			return left != (string) right;
		}
	}
}