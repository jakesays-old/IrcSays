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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace IrcSays.Utility
{
	internal static class ExtensionMethods
	{
		/// <summary>
		///     Converts a recursive data structure into a flat list.
		/// </summary>
		/// <param name="input">The root elements of the recursive data structure.</param>
		/// <param name="recursion">The function that gets the children of an element.</param>
		/// <returns>Iterator that enumerates the tree structure in preorder.</returns>
		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> input, Func<T, IEnumerable<T>> recursion)
		{
			var stack = new Stack<IEnumerator<T>>();
			try
			{
				stack.Push(input.GetEnumerator());
				while (stack.Count > 0)
				{
					while (stack.Peek().MoveNext())
					{
						var element = stack.Peek().Current;
						yield return element;
						var children = recursion(element);
						if (children != null)
						{
							stack.Push(children.GetEnumerator());
						}
					}
					stack.Pop().Dispose();
				}
			}
			finally
			{
				while (stack.Count > 0)
				{
					stack.Pop().Dispose();
				}
			}
		}
	}

	public delegate void FileOperationDelegate();

	public delegate void NamedFileOperationDelegate(FileName fileName);

	/// <summary>
	///     A utility class related to file utilities.
	/// </summary>
	public static partial class FileUtility
	{
		private static readonly char[] _separators = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		private const string FileNameRegEx = @"^([a-zA-Z]:)?[^:]+$";

		public static string ApplicationRootPath { get; set; }

		private static string GetPathFromRegistry(string key, string valueName)
		{
			using (var installRootKey = Registry.LocalMachine.OpenSubKey(key))
			{
				if (installRootKey != null)
				{
					var o = installRootKey.GetValue(valueName);
					if (o != null)
					{
						var r = o.ToString();
						if (!string.IsNullOrEmpty(r))
						{
							return r;
						}
					}
				}
			}
			return null;
		}

		private static string GetPathFromRegistryX86(string key, string valueName)
		{
			using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			{
				using (var installRootKey = baseKey.OpenSubKey(key))
				{
					if (installRootKey != null)
					{
						var o = installRootKey.GetValue(valueName);
						if (o != null)
						{
							var r = o.ToString();
							if (!string.IsNullOrEmpty(r))
							{
								return r;
							}
						}
					}
				}
			}
			return null;
		}

		#region InstallRoot Properties

		private static string _netFrameworkInstallRoot = null;

		/// <summary>
		///     Gets the installation root of the .NET Framework (@"C:\Windows\Microsoft.NET\Framework\")
		/// </summary>
		public static string NetFrameworkInstallRoot
		{
			get
			{
				if (_netFrameworkInstallRoot == null)
				{
					_netFrameworkInstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\.NETFramework", "InstallRoot") ?? string.Empty;
				}
				return _netFrameworkInstallRoot;
			}
		}

		private static string _netSdk20InstallRoot = null;

		/// <summary>
		///     Location of the .NET 2.0 SDK install root.
		/// </summary>
		public static string NetSdk20InstallRoot
		{
			get
			{
				if (_netSdk20InstallRoot == null)
				{
					_netSdk20InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\.NETFramework", "sdkInstallRootv2.0") ??
										string.Empty;
				}
				return _netSdk20InstallRoot;
			}
		}

		private static string windowsSdk60InstallRoot = null;

		/// <summary>
		///     Location of the .NET 3.0 SDK (Windows SDK 6.0) install root.
		/// </summary>
		public static string WindowsSdk60InstallRoot
		{
			get
			{
				if (windowsSdk60InstallRoot == null)
				{
					windowsSdk60InstallRoot =
						GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.0", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk60InstallRoot;
			}
		}

		private static string _windowsSdk60AInstallRoot = null;

		/// <summary>
		///     Location of the Windows SDK Components in Visual Studio 2008 (.NET 3.5; Windows SDK 6.0a).
		/// </summary>
		public static string WindowsSdk60aInstallRoot
		{
			get
			{
				if (_windowsSdk60AInstallRoot == null)
				{
					_windowsSdk60AInstallRoot =
						GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.0a", "InstallationFolder") ?? string.Empty;
				}
				return _windowsSdk60AInstallRoot;
			}
		}

		private static string _windowsSdk61InstallRoot = null;

		/// <summary>
		///     Location of the .NET 3.5 SDK (Windows SDK 6.1) install root.
		/// </summary>
		public static string WindowsSdk61InstallRoot
		{
			get
			{
				if (_windowsSdk61InstallRoot == null)
				{
					_windowsSdk61InstallRoot =
						GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.1", "InstallationFolder") ?? string.Empty;
				}
				return _windowsSdk61InstallRoot;
			}
		}

		private static string _windowsSdk70InstallRoot = null;

		/// <summary>
		///     Location of the .NET 3.5 SP1 SDK (Windows SDK 7.0) install root.
		/// </summary>
		public static string WindowsSdk70InstallRoot
		{
			get
			{
				if (_windowsSdk70InstallRoot == null)
				{
					_windowsSdk70InstallRoot =
						GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0", "InstallationFolder") ?? string.Empty;
				}
				return _windowsSdk70InstallRoot;
			}
		}

		private static string _windowsSdk71InstallRoot = null;

		/// <summary>
		///     Location of the .NET 4.0 SDK (Windows SDK 7.1) install root.
		/// </summary>
		public static string WindowsSdk71InstallRoot
		{
			get
			{
				if (_windowsSdk71InstallRoot == null)
				{
					_windowsSdk71InstallRoot =
						GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1", "InstallationFolder") ?? string.Empty;
				}
				return _windowsSdk71InstallRoot;
			}
		}

		private static string _windowsSdk80InstallRoot = null;

		/// <summary>
		///     Location of the .NET 4.5 SDK (Windows SDK 8.0) install root.
		/// </summary>
		public static string WindowsSdk80NetFxTools
		{
			get
			{
				if (_windowsSdk80InstallRoot == null)
				{
					_windowsSdk80InstallRoot =
						GetPathFromRegistryX86(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0A\WinSDK-NetFx40Tools",
							"InstallationFolder") ?? string.Empty;
				}
				return _windowsSdk80InstallRoot;
			}
		}

		#endregion

		public static bool IsUrl(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return path.IndexOf("://", StringComparison.Ordinal) > 0;
		}

		public static bool IsEqualFileName(FileName fileName1, FileName fileName2)
		{
			return fileName1 == fileName2;
		}

		public static string GetCommonBaseDirectory(string dir1, string dir2)
		{
			if (dir1 == null ||
				dir2 == null)
			{
				return null;
			}
			if (IsUrl(dir1) ||
				IsUrl(dir2))
			{
				return null;
			}

			dir1 = NormalizePath(dir1);
			dir2 = NormalizePath(dir2);

			var aPath = dir1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var bPath = dir2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var result = new StringBuilder();
			var indx = 0;
			for (; indx < Math.Min(bPath.Length, aPath.Length); ++indx)
			{
				if (bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase))
				{
					if (result.Length > 0)
					{
						result.Append(Path.DirectorySeparatorChar);
					}
					result.Append(aPath[indx]);
				}
				else
				{
					break;
				}
			}
			if (indx == 0)
			{
				return null;
			}
			return result.ToString();
		}

		/// <summary>
		///     Searches all the .net sdk bin folders and return the path of the
		///     exe from the latest sdk.
		/// </summary>
		/// <param name="exeName">The EXE to search for.</param>
		/// <returns>The path of the executable, or null if the exe is not found.</returns>
		public static string GetSdkPath(string exeName)
		{
			string execPath;
			if (!string.IsNullOrEmpty(WindowsSdk80NetFxTools))
			{
				execPath = Path.Combine(WindowsSdk80NetFxTools, exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(WindowsSdk71InstallRoot))
			{
				execPath = Path.Combine(WindowsSdk71InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(WindowsSdk70InstallRoot))
			{
				execPath = Path.Combine(WindowsSdk70InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(WindowsSdk61InstallRoot))
			{
				execPath = Path.Combine(WindowsSdk61InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(WindowsSdk60aInstallRoot))
			{
				execPath = Path.Combine(WindowsSdk60aInstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(WindowsSdk60InstallRoot))
			{
				execPath = Path.Combine(WindowsSdk60InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			if (!string.IsNullOrEmpty(NetSdk20InstallRoot))
			{
				execPath = Path.Combine(NetSdk20InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath))
				{
					return execPath;
				}
			}
			return null;
		}

		/// <summary>
		///     Converts a given absolute path and a given base path to a path that leads
		///     from the base path to the absoulte path. (as a relative path)
		/// </summary>
		public static string GetRelativePath(string baseDirectoryPath, string absPath)
		{
			if (string.IsNullOrEmpty(baseDirectoryPath))
			{
				return absPath;
			}
			if (IsUrl(absPath) ||
				IsUrl(baseDirectoryPath))
			{
				return absPath;
			}

			baseDirectoryPath = NormalizePath(baseDirectoryPath);
			absPath = NormalizePath(absPath);

			var bPath = baseDirectoryPath != "." ? baseDirectoryPath.Split(_separators) : new string[0];
			var aPath = absPath != "." ? absPath.Split(_separators) : new string[0];
			var indx = 0;
			for (; indx < Math.Min(bPath.Length, aPath.Length); ++indx)
			{
				if (!bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
			}

			if (indx == 0 &&
				(Path.IsPathRooted(baseDirectoryPath) || Path.IsPathRooted(absPath)))
			{
				return absPath;
			}

			if (indx == bPath.Length &&
				indx == aPath.Length)
			{
				return ".";
			}
			var erg = new StringBuilder();
			for (var i = indx; i < bPath.Length; ++i)
			{
				erg.Append("..");
				erg.Append(Path.DirectorySeparatorChar);
			}
			erg.Append(String.Join(Path.DirectorySeparatorChar.ToString(), aPath, indx, aPath.Length - indx));
			if (erg[erg.Length - 1] == Path.DirectorySeparatorChar)
			{
				erg.Length -= 1;
			}
			return erg.ToString();
		}

		/// <summary>
		///     Combines baseDirectoryPath with relPath and normalizes the resulting path.
		/// </summary>
		public static string GetAbsolutePath(string baseDirectoryPath, string relPath)
		{
			return NormalizePath(Path.Combine(baseDirectoryPath, relPath));
		}

		public static string RenameBaseDirectory(string fileName, string oldDirectory, string newDirectory)
		{
			fileName = NormalizePath(fileName);
			oldDirectory = NormalizePath(oldDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			newDirectory = NormalizePath(newDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			if (IsBaseDirectory(oldDirectory, fileName))
			{
				if (fileName.Length == oldDirectory.Length)
				{
					return newDirectory;
				}
				return Path.Combine(newDirectory, fileName.Substring(oldDirectory.Length + 1));
			}
			return fileName;
		}

		public static void DeepCopy(string sourceDirectory, string destinationDirectory, bool overwrite)
		{
			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}
			foreach (var fileName in Directory.GetFiles(sourceDirectory))
			{
				File.Copy(fileName, Path.Combine(destinationDirectory, Path.GetFileName(fileName)), overwrite);
			}
			foreach (var directoryName in Directory.GetDirectories(sourceDirectory))
			{
				DeepCopy(directoryName, Path.Combine(destinationDirectory, Path.GetFileName(directoryName)), overwrite);
			}
		}

		[Obsolete("Use SD.FileSystem.GetFiles() instead")]
		public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories,
			bool ignoreHidden)
		{
			return
				SearchDirectoryInternal(directory, filemask, searchSubdirectories, ignoreHidden)
					.Select(file => file.ToString())
					.ToList();
		}

		[Obsolete("Use SD.FileSystem.GetFiles() instead")]
		public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories)
		{
			return SearchDirectory(directory, filemask, searchSubdirectories, true);
		}

		[Obsolete("Use SD.FileSystem.GetFiles() instead")]
		public static List<string> SearchDirectory(string directory, string filemask)
		{
			return SearchDirectory(directory, filemask, true, true);
		}

		public static IEnumerable<FileName> LazySearchDirectory(string directory, string filemask,
			bool searchSubdirectories = true, bool ignoreHidden = true)
		{
			return SearchDirectoryInternal(directory, filemask, searchSubdirectories, ignoreHidden);
		}

		/// <summary>
		///     Finds all files which are valid to the mask <paramref name="filemask" /> in the path
		///     <paramref name="directory" /> and all subdirectories
		///     (if <paramref name="searchSubdirectories" /> is true).
		///     If <paramref name="ignoreHidden" /> is true, hidden files and folders are ignored.
		/// </summary>
		private static IEnumerable<FileName> SearchDirectoryInternal(string directory, string filemask,
			bool searchSubdirectories, bool ignoreHidden)
		{
			// If Directory.GetFiles() searches the 8.3 name as well as the full name so if the filemask is
			// "*.xpt" it will return "Template.xpt~"
			var isExtMatch = filemask != null && Regex.IsMatch(filemask, @"^\*\.[\w\d_]{3}$");
			string ext = null;
			if (isExtMatch)
			{
				ext = filemask.Substring(1);
			}
			IEnumerable<string> dir = new[] {directory};

			if (searchSubdirectories)
			{
				dir = dir.Flatten(
					d =>
					{
						try
						{
							if (ignoreHidden)
							{
								return Directory.EnumerateDirectories(d).Where(IsNotHidden);
							}
							else
							{
								return Directory.EnumerateDirectories(d);
							}
						}
						catch (UnauthorizedAccessException)
						{
							return new string[0];
						}
					});
			}
			foreach (var d in dir)
			{
				IEnumerable<string> files;
				try
				{
					files = Directory.EnumerateFiles(d, filemask);
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
				foreach (var f in files)
				{
					if (ext != null &&
						!f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
					{
						continue; // file extension didn't match
					}
					if (!ignoreHidden ||
						IsNotHidden(f))
					{
						yield return new FileName(f);
					}
				}
			}
		}

		private static bool IsNotHidden(string dir)
		{
			try
			{
				return (File.GetAttributes(dir) & FileAttributes.Hidden) != FileAttributes.Hidden;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		// This is an arbitrary limitation built into the .NET Framework.
		// Windows supports paths up to 32k length.
		public static readonly int MaxPathLength = 260;

		static FileUtility()
		{
			ApplicationRootPath = AppDomain.CurrentDomain.BaseDirectory;
		}

		/// <summary>
		///     This method checks if a path (full or relative) is valid.
		/// </summary>
		public static bool IsValidPath(string fileName)
		{
			// Fixme: 260 is the hardcoded maximal length for a path on my Windows XP system
			//        I can't find a .NET property or method for determining this variable.

			if (string.IsNullOrEmpty(fileName) ||
				fileName.Length >= MaxPathLength)
			{
				return false;
			}

			// platform independend : check for invalid path chars

			if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			{
				return false;
			}
			if (fileName.IndexOf('?') >= 0 ||
				fileName.IndexOf('*') >= 0)
			{
				return false;
			}

			if (!Regex.IsMatch(fileName, FileNameRegEx))
			{
				return false;
			}

			if (fileName[fileName.Length - 1] == ' ')
			{
				return false;
			}

			if (fileName[fileName.Length - 1] == '.')
			{
				return false;
			}

			// platform dependend : Check for invalid file names (DOS)
			// this routine checks for follwing bad file names :
			// CON, PRN, AUX, NUL, COM1-9 and LPT1-9

			var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			if (nameWithoutExtension != null)
			{
				nameWithoutExtension = nameWithoutExtension.ToUpperInvariant();
			}

			if (nameWithoutExtension == "CON" ||
				nameWithoutExtension == "PRN" ||
				nameWithoutExtension == "AUX" ||
				nameWithoutExtension == "NUL")
			{
				return false;
			}

			var ch = nameWithoutExtension.Length == 4 ? nameWithoutExtension[3] : '\0';

			return !((nameWithoutExtension.StartsWith("COM", StringComparison.Ordinal) ||
					nameWithoutExtension.StartsWith("LPT", StringComparison.Ordinal)) &&
					Char.IsDigit(ch));
		}

		/// <summary>
		///     Checks that a single directory name (not the full path) is valid.
		/// </summary>
		public static bool IsValidDirectoryEntryName(string name)
		{
			if (!IsValidPath(name))
			{
				return false;
			}
			if (
				name.IndexOfAny(new char[] {Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar, Path.VolumeSeparatorChar}) >=
				0)
			{
				return false;
			}
			if (name.Trim(' ').Length == 0)
			{
				return false;
			}
			return true;
		}

		public static bool TestFileExists(string filename)
		{
			if (!File.Exists(filename))
			{
//				var messageService = ServiceSingleton.GetRequiredService<IMessageService>();
//				messageService.ShowWarning(StringParser.Parse("${res:Fileutility.CantFindFileError}", new StringTagPair("FILE", filename)));
				return false;
			}
			return true;
		}

		public static bool IsDirectory(string filename)
		{
			if (!Directory.Exists(filename))
			{
				return false;
			}
			var attr = File.GetAttributes(filename);
			return (attr & FileAttributes.Directory) != 0;
		}

		//TODO This code is Windows specific
		private static bool MatchN(string src, int srcidx, string pattern, int patidx)
		{
			var patlen = pattern.Length;
			var srclen = src.Length;
			char next_char;

			for (;;)
			{
				if (patidx == patlen)
				{
					return (srcidx == srclen);
				}
				next_char = pattern[patidx++];
				if (next_char == '?')
				{
					if (srcidx == src.Length)
					{
						return false;
					}
					srcidx++;
				}
				else if (next_char != '*')
				{
					if ((srcidx == src.Length) ||
						(src[srcidx] != next_char))
					{
						return false;
					}
					srcidx++;
				}
				else
				{
					if (patidx == pattern.Length)
					{
						return true;
					}
					while (srcidx < srclen)
					{
						if (MatchN(src, srcidx, pattern, patidx))
						{
							return true;
						}
						srcidx++;
					}
					return false;
				}
			}
		}

		private static bool Match(string src, string pattern)
		{
			if (pattern[0] == '*')
			{
				// common case optimization
				var i = pattern.Length;
				var j = src.Length;
				while (--i > 0)
				{
					if (pattern[i] == '*')
					{
						return MatchN(src, 0, pattern, 0);
					}
					if (j-- == 0)
					{
						return false;
					}
					if ((pattern[i] != src[j]) &&
						(pattern[i] != '?'))
					{
						return false;
					}
				}
				return true;
			}
			return MatchN(src, 0, pattern, 0);
		}

		public static bool MatchesPattern(string filename, string pattern)
		{
			filename = filename.ToUpperInvariant();
			pattern = pattern.ToUpperInvariant();
			var patterns = pattern.Split(';');
			foreach (var p in patterns)
			{
				if (Match(filename, p))
				{
					return true;
				}
			}
			return false;
		}
	}
}