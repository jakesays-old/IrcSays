using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IrcSays.Ui
{
	public class ChatLine : IChatSpanProvider
	{
		public DateTime Time { get; private set; }
		public string ColorKey { get; private set; }
		public int NickHashCode { get; private set; }
		public string Nick { get; private set; }
		public string RawText { get; private set; }
		public string Text { get; private set; }
		public ChatMarker Marker { get; set; }
		public ChatSpan[] Spans { get; private set; }
		public ChatLink[] Links { get; private set; }

		public ChatLine(string colorKey, DateTime time, int nickHashCode, string nick, string text, ChatMarker decoration)
		{
			ColorKey = colorKey;
			Time = time;
			NickHashCode = nickHashCode;
			Nick = nick;
			Marker = decoration;
			RawText = text;
			Process(text);
		}

		public ChatLine(string colorKey, int nickHashCode, string nick, string text, ChatMarker decoration)
			: this(colorKey, DateTime.Now, nickHashCode, nick, text, decoration)
		{
		}

		public ChatSpan GetSpan(int idx)
		{
			return Spans.Where(s => idx >= s.Start && idx < s.End).FirstOrDefault();
		}

		private void ParseLine(string line)
		{
			var lastPos = line.Length - 1;
			var text = new StringBuilder();
			var spans = new List<ChatSpan>();
			var currentSpan = new ChatSpan();

			const int normalState = 0;
			const int fgColorState = 1;
			const int bgColorState = 2;

			var currentState = normalState;
			var colorCharCount = 0;
			var currentColor = 0;

			for(var index = 0; index <= lastPos; index++)
			{
				var ch = (int) line[index];
				if (currentState == normalState)
				{
					if (!TextFormatCodes.IsFormatChar(ch))
					{
						text.Append((char) ch);
						continue;
					}

					currentSpan.End = index;
					spans.Add(currentSpan);
					currentSpan.Start = index;

					switch (ch)
					{
						case TextFormatCodes.Color:
							currentState = fgColorState;
							colorCharCount = 0;
							currentColor = 0;
							break;
						case TextFormatCodes.Bold:
							currentSpan.Flags ^= ChatSpanFlags.Bold;
							break;
						case TextFormatCodes.Reset:
							currentSpan.Flags = ChatSpanFlags.None;
							break;
						case TextFormatCodes.Reverse:
							currentSpan.Flags ^= ChatSpanFlags.Reverse;
							break;
						case TextFormatCodes.Underline:
							currentSpan.Flags ^= ChatSpanFlags.Underline;
							break;
					}

					continue;
				}

				if (!char.IsDigit((char) ch) ||
					colorCharCount >= 2)
				{
					if (currentState == fgColorState)
					{
						if (colorCharCount == 0)
						{
							//no colors specified
							currentSpan.Flags &= ~ChatSpanFlags.Foreground;
							currentSpan.Flags &= ~ChatSpanFlags.Background;
							currentState = normalState;
						}
						else
						{
							currentSpan.Flags |= ChatSpanFlags.Foreground;
							currentSpan.Foreground = (byte) Math.Min(currentColor, 15);
							colorCharCount = 0;
							currentState = ch == ','
								? bgColorState
								: normalState;
						}
					}
					else if (currentState == bgColorState)
					{
						currentState = normalState;
						currentSpan.Flags |= ChatSpanFlags.Foreground;
						currentSpan.Foreground = (byte) Math.Min(currentColor, 15);
					}

					if (currentState == normalState)
					{
						text.Append((char) ch);
					}
					continue;
				}

				currentColor += ('9' - ch) * (++colorCharCount);
			}

			if (currentState != normalState)
			{
				//indicates the last char in line was the color formatting code without color parameters
				currentSpan.Flags &= ~ChatSpanFlags.Foreground;
				currentSpan.Flags &= ~ChatSpanFlags.Background;
			}

			currentSpan.End = lastPos;
			spans.Add(currentSpan);
		}

		public void Process(string raw)
		{
			var text = new StringBuilder();
			var spans = new List<ChatSpan>();
			var span = new ChatSpan();

			var last = raw.Length - 1;
			var idx = 0;
			for (var i = 0; i < raw.Length; i++)
			{
				var ichar = (int) raw[i];
				if (TextFormatCodes.IsFormatChar(ichar))
				{
					span.End = idx;
					spans.Add(span);
					span.Start = idx;
				}
				switch (ichar)
				{
					case TextFormatCodes.Bold:
						span.Flags ^= ChatSpanFlags.Bold;
						break;
					case TextFormatCodes.Color:
						if (i == last ||
							(raw[i + 1] > '9' || raw[i + 1] < '0'))
						{
							span.Flags &= ~ChatSpanFlags.Foreground;
							span.Flags &= ~ChatSpanFlags.Background;
							break;
						}
						span.Flags |= ChatSpanFlags.Foreground;
						var c = raw[++i] - '0';
						if (i < last &&
							(
								(c == 0 && raw[i + 1] >= '0' && raw[i + 1] <= '9') ||
								(c == 1 && raw[i + 1] >= '0' && raw[i + 1] <= '5')
								))
						{
							c *= 10;
							c += raw[++i] - '0';
						}
						span.Foreground = (byte) Math.Min(15, c);
						if (i == last ||
							i + 1 == last ||
							raw[i + 1] != ',' ||
							raw[i + 2] < '0' ||
							raw[i + 2] > '9')
						{
							break;
						}
						span.Flags |= ChatSpanFlags.Background;
						++i;
						c = raw[++i] - '0';
						if (i < last &&
							(
								(c == 0 && raw[i + 1] >= '0' && raw[i + 1] <= '9') ||
								(c == 1 && raw[i + 1] >= '0' && raw[i + 1] <= '5')
								))
						{
							c *= 10;
							c += raw[++i] - '0';
						}
						span.Background = (byte) Math.Min(15, c);
						break;
					case TextFormatCodes.Reset:
						span.Flags = ChatSpanFlags.None;
						break;
					case TextFormatCodes.Reverse:
						span.Flags ^= ChatSpanFlags.Reverse;
						break;
					case TextFormatCodes.Underline:
						span.Flags ^= ChatSpanFlags.Underline;
						break;
					default:
						text.Append(raw[i]);
						idx++;
						break;
				}
			}
			span.End = idx;
			spans.Add(span);
			Text = text.ToString();
			Spans = spans.Where(s => s.End > s.Start).ToArray();
			Links = (from Match m in Constants.UrlRegex.Matches(Text)
				select new ChatLink
				{
					Start = m.Index, 
					End = m.Index + m.Length
				}).ToArray();
		}
	}
}