using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace IrcSays.Utility
{
	public static partial class StringExtensions
	{
		private static readonly string[] _emptyStrings = new string[0];

		/// <summary>
		/// Constant empty string array.
		/// </summary>
		public static string[] EmptyStrings
		{
			get { return _emptyStrings; }
		}

		[StringFormatMethod("value")]
		public static string FormatWith(this string value, object arg)
		{
			return string.Format(value, arg);
		}

		[StringFormatMethod("value")]
		public static string FormatWith(this string value, object arg1, object arg2)
		{
			return string.Format(value, arg1, arg2);
		}

		[StringFormatMethod("value")]
		public static string FormatWith(this string value, object arg1, object arg2, object arg3)
		{
			return string.Format(value, arg1, arg2, arg3);
		}

		[StringFormatMethod("value")]
		public static string FormatWith(this string value, params object[] args)
		{
			return string.Format(value, args);
		}

		[StringFormatMethod("value")]
		public static string SafeFormatWith(this string value, params object[] args)
		{
			if (value == null)
			{
				return null;
			}

			return string.Format(value, args);
		}

		[ContractAnnotation("s:null => true")]
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		[ContractAnnotation("s:null => false")]
		public static bool NotNullOrEmpty(this string value)
		{
			return !string.IsNullOrEmpty(value);
		}

		public static string ValueOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value) ? "" : value;
		}

		/// <summary>
		/// Returns the passed in string if it is not null or empty.
		/// </summary>
		/// <param name="value">Target string</param>
		/// <returns>value if length > 0, null otherwise</returns>
		public static string ValueOrNull(this string value)
		{
			return string.IsNullOrEmpty(value) ? null : value;
		}

		/// <summary>
		/// Return the last <param name="charCount">characters</param>
		/// </summary>
		/// <param name="value">The target string</param>
		/// <param name="charCount">Count of characters to return at the end of the string.</param>
		/// <returns>Last charCount chars of value, or value if the length of value is less than charCount</returns>
		public static string Last(this string value, int charCount)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			var len = value.Length;
			if (len <= charCount)
			{
				return value;
			}

			return value.Substring(len - charCount, charCount);
		}

		/// <summary>
		/// Returns the value of a string if one is present,
		/// or a default value if the string is <c>null</c> or empty.
		/// </summary>
		/// <param name="value">The target string.</param>
		/// <param name="defaultValue">The default value to return.</param>
		/// <returns><paramref name="value"/>if value is not null and value.Length > 0,
		/// <paramref name="defaultValue"/> otherwise.</returns>
		public static string ValueOrDefault(this string value, string defaultValue)
		{
			return string.IsNullOrEmpty(value) ? defaultValue : value;
		}

		public static string SafeTrim(this string value)
		{
			if (value != null)
			{
				return value.Trim();
			}

			return null;
		}

		public static string SafeToUpperInvariant(this string value)
		{
			if (value == null)
			{
				return null;
			}

			return value.ToUpperInvariant();
		}

		public static string SafeToLowerInvariant(this string value)
		{
			if (value == null)
			{
				return null;
			}

			return value.ToLowerInvariant();
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string Substring(this string value, string leftDelimiter, string rightDelimiter)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			var startPos = 0;
			var endPos = 0;

			if (!leftDelimiter.IsNullOrEmpty())
			{
				startPos = value.IndexOf(leftDelimiter, StringComparison.InvariantCulture);
				if (startPos == -1)
				{
					return null;
				}

				startPos += leftDelimiter.Length;
			}

			if (!rightDelimiter.IsNullOrEmpty())
			{
				endPos = value.IndexOf(rightDelimiter, StringComparison.InvariantCulture);
				if (endPos == -1)
				{
					return null;
				}
			}

			if (endPos == 0)
			{
				endPos = value.Length;
			}

			var substr = value.Substring(startPos, endPos - startPos);

			return substr;
		}

		public static bool EqualsInvariant(this string lhs, string rhs)
		{
			return string.Equals(lhs, rhs, StringComparison.InvariantCulture);
		}

		public static bool EqualsInvariantIgnoreCase(this string lhs, string rhs)
		{
			return string.Equals(lhs, rhs, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Chops, or truncates, a string at maxLength.
		/// </summary>
		/// <param name="src">String to be chopped</param>
		/// <param name="maxLength">Maximum length of string</param>
		/// <returns>Chopped string.</returns>
		public static string Chop(this string src, int maxLength)
		{
			if (src == null)
			{
				return null;
			}

			if (maxLength < 0)
			{
				throw new ArgumentOutOfRangeException("maxLength");
			}

			if (src.Length == 0 ||
				src.Length == maxLength)
			{
				return src;
			}

			return src.Substring(0, Math.Min(src.Length, maxLength));
		}

		/// <summary>
		/// Word wraps the given text to fit within the specified width.
		/// </summary>
		/// <param name="text">Text to be word wrapped</param>
		/// <param name="width">Width, in characters, to which the text
		/// should be word wrapped</param>
		/// <returns>The modified text</returns>
		public static string WordWrap(string text, int width)
		{
			//lifted from: http://www.blackbeltcoder.com/Articles/strings/implementing-word-wrap-in-email-messages
			//and improved

			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException("width");
			}

			int pos, next;
			var sb = new StringBuilder();

			// Parse each line of text
			for (pos = 0; pos < text.Length; pos = next)
			{
				// Find end of line
				var eol = text.IndexOf(Environment.NewLine, pos);
				if (eol == -1)
				{
					next = eol = text.Length;
				}
				else
				{
					next = eol + Environment.NewLine.Length;
				}

				// Copy this line of text, breaking into smaller lines as needed
				if (eol > pos)
				{
					do
					{
						var len = eol - pos;
						if (len > width)
						{
							len = BreakLine(text, pos, width);
						}
						sb.Append(text, pos, len);
						sb.Append(Environment.NewLine);

						// Trim whitespace following break
						pos += len;
						while (pos < eol && Char.IsWhiteSpace(text[pos]))
						{
							pos++;
						}
					} while (eol > pos);
				}
				else
				{
					sb.Append(Environment.NewLine); // Empty line
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Locates position to break the given line so as to avoid
		/// breaking words.
		/// </summary>
		/// <param name="text">String that contains line of text</param>
		/// <param name="pos">Index where line of text starts</param>
		/// <param name="max">Maximum line length</param>
		/// <returns>The modified line length</returns>
		private static int BreakLine(string text, int pos, int max)
		{
			// Find last whitespace in line
			int i = max;
			while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
			{
				i--;
			}

			// If no whitespace found, break at maximum length
			if (i < 0)
			{
				return max;
			}

			// Find start of whitespace
			while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
			{
				i--;
			}

			// Return length of text before whitespace
			return i + 1;
		}

		/// <summary>
		/// Breaks a string of text in to lines of a maximum width.
		/// Breaks happen at word boundaries.
		/// </summary>
		/// <param name="text">Line of text to be broken up.</param>
		/// <param name="width">Maximum length of a line.</param>
		/// <returns>List of lines.</returns>
		public static List<string> BreakIntoLines(string text, int width)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException("width");
			}

			var pos = 0;
			var eol = text.Length;
			var lines = new List<string>();

			do
			{
				var len = eol - pos;
				if (len > width)
				{
					len = BreakLine(text, pos, width);
				}
				var line = text.Substring(pos, len);
				lines.Add(line);

				// Trim whitespace following break
				pos += len;
				while (pos < eol && Char.IsWhiteSpace(text[pos]))
				{
					pos++;
				}
			} while (eol > pos);

			return lines;
		}

		public static string ToPascalCase(this string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (source.Length == 0)
			{
				return source;
			}

			var buffer = source.ToCharArray();
			var first = false;
			var needle = 0;
			for (var idx = 0; idx < buffer.Length; idx++)
			{
				var c = buffer[idx];
				if (Char.IsLetterOrDigit(c))
				{
					if (first || needle == 0)
					{
						buffer[needle] = Char.ToUpperInvariant(c);
					}
					else
					{
						buffer[needle] = Char.ToLowerInvariant(c);
					}
					first = false;
					needle += 1;
				}
				else if (char.IsWhiteSpace(c) ||
					char.IsPunctuation(c))
				{
					first = true;
				}
				else
				{
					buffer[needle] = c;
					needle += 1;
				}
			}

			return new string(buffer, 0, needle);
		}
	}
}
