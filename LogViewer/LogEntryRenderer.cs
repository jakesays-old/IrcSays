using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Std.Ui.Logging
{
	public class LogEntryRenderer : TextSource, ILogEntryRenderer
	{
		private string _text;
		private CustomTextRunProperties _runProperties;
		private CustomParagraphProperties _paraProperties;
		private readonly TextFormatter _formatter;
		private LogEntry _entry;
		private Brush _background;
		private readonly LogEntryPalette _palette;

		private class CustomTextRunProperties : TextRunProperties
		{
			private readonly Typeface _typeface;
			private readonly double _fontSize;
			private readonly Brush _foreground;
			private readonly Brush _background;
			private readonly TextDecorationCollection _decorations;

			public override double FontHintingEmSize => _fontSize;

			public override TextDecorationCollection TextDecorations => _decorations;

			public override TextEffectCollection TextEffects => null;

			public override CultureInfo CultureInfo => CultureInfo.InvariantCulture;

			public override Typeface Typeface => _typeface;

			public override double FontRenderingEmSize => _fontSize;

			public override Brush BackgroundBrush => _background;

			public override Brush ForegroundBrush => _foreground;

			public CustomTextRunProperties(Typeface typeface, double fontSize, 
				Brush foreground, Brush background, bool underline)
			{
				_typeface = typeface;
				_fontSize = fontSize;
				_foreground = foreground;
				_background = background;
				if (underline)
				{
					_decorations = new TextDecorationCollection(1);
					_decorations.Add(System.Windows.TextDecorations.Underline);
				}
			}
		}

		private class CustomParagraphProperties : TextParagraphProperties
		{
			public override FlowDirection FlowDirection => FlowDirection.LeftToRight;

			public override TextAlignment TextAlignment => TextAlignment.Left;

			public override double LineHeight => 0.0;

			public override bool FirstLineInParagraph => false;

			public override TextWrapping TextWrapping { get; }

			public override TextMarkerProperties TextMarkerProperties => null;

			public override TextRunProperties DefaultTextRunProperties { get; }

			public override double Indent => 0.0;

			public CustomParagraphProperties(TextRunProperties defaultTextRunProperties)
			{
				DefaultTextRunProperties = defaultTextRunProperties;
				TextWrapping = TextWrapping.Wrap;
			}
		}


		public LogEntryRenderer(Typeface typeface, double fontSize, Brush foreground, 
			LogEntryPalette palette)
		{
			_runProperties = new CustomTextRunProperties(typeface, fontSize, foreground, Brushes.Transparent, false);
			_paraProperties = new CustomParagraphProperties(_runProperties);
			_formatter = TextFormatter.Create(TextFormattingMode.Display);
			_palette = palette;
		}

		public IEnumerable<TextLine> RenderEntry(string text, LogEntry entry, double width, 
			Brush foreground,
			Brush background,
			TextWrapping textWrapping,
			bool forceBackground = false)
		{
			_text = text;
			_entry = entry;
			_background = background;
			_runProperties = new CustomTextRunProperties(
				_runProperties.Typeface, 
				_runProperties.FontRenderingEmSize, 
				foreground,
				forceBackground
					? background
					: Brushes.Transparent,
				false);
			_paraProperties = new CustomParagraphProperties(_runProperties);
			if (width < 0)
			{
				width = 0;
				text = "";
			}

			var idx = 0;
			while (idx < _text.Length)
			{
				var line = _formatter.FormatLine(this, idx, width, _paraProperties, null);
				idx += line.Length;
				yield return line;
			}
		}

		private FontWeight GetSpanWeight(LogEntrySpan span)
		{
			return span.IsBold ? FontWeights.Bold : FontWeights.Normal;
		}

		private Brush GetSpanForeground(LogEntrySpan span)
		{
			if (span.IsReverse)
			{
				return _background;
			}

			if (span.IsForeground)
			{
				return _palette["Color" + span.Foreground];
			}

			return _runProperties.ForegroundBrush;
        }

		private Brush GetSpanBackground(LogEntrySpan span)
		{
			if (span.IsReverse)
			{
				return _runProperties.ForegroundBrush;
			}

			if (span.IsBackground)
			{
				return _palette["Color" + span.Background];
			}
			return _runProperties.BackgroundBrush;
		}

		private Typeface MakeTypeface(LogEntrySpan span)
		{
			var face = new Typeface(_runProperties.Typeface.FontFamily,
				_runProperties.Typeface.Style,
				GetSpanWeight(span),
				_runProperties.Typeface.Stretch);

			return face;
		}

		public override TextRun GetTextRun(int idx)
		{
			if (idx >= _text.Length)
			{
				return new TextEndOfLine(1);
			}
			var props = _runProperties;
			var end = _text.Length;
			if (_entry.Spans != null)
			{
				var span = _entry.Spans[idx];
				if (span.FormatOptions != SpanFormatOptions.None)
				{
					props = new CustomTextRunProperties(
						MakeTypeface(span),
						_runProperties.FontRenderingEmSize,
						GetSpanForeground(span),
						GetSpanBackground(span),
						span.IsUnderline);
				}
				end = span.End;
			}
			return new TextCharacters(_text, idx, end - idx, props);
		}

		public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
		{
			throw new NotImplementedException();
		}

		public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
		{
			throw new NotImplementedException();
		}
	}
}