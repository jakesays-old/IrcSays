using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Std.Ui.Logging;

namespace IrcSays.Ui.Logging
{
	internal class IrcDisplayBlockFormatter : IDisplayBlockFormatter
	{
		private const float MinNameBrightness = .2f;
		private const float NameBrightnessBand = .2f;

		private LogView _view;
		public Brush BackgroundBrush { get; set; }
		public Color BackgroundColor { get; set; }
		public Brush ForegroundBrush { get; set; }
		public bool ColorizeNames { get; set; }
		public bool JustifyNameColumn { get; set; }
		public double NameColumnWidth { get; set; }
		public LogEntryPalette Palette { get; set; }
		public double SeparatorPadding { get; set; }
		public Typeface Typeface { get; set; }
		public double FontSize { get; set; }
		public bool ShowTimestamp { get; set; }
		public string TimestampFormat { get; set; }
		public int NameColorizerSeed { get; set; }

		public void Attach(LogView view)
		{
			_view = view;
		}

		public string FormatName(string name)
		{
			if (!JustifyNameColumn)
			{
				if (name == null)
				{
					name = "* ";
				}
				else
				{
					name = $"<{name}> ";
				}
			}
			return name ?? "*";
		}

		public string FormatTimestamp(DateTime? timestamp)
		{
			if (ShowTimestamp && timestamp.HasValue)
			{
				return timestamp.Value.ToString(TimestampFormat + " ");
			}
			return "";
		}

		public void FormatBlock(DisplayBlock displayBlock, bool autoSize, bool highlightName = false)
		{
			displayBlock.Foreground = Palette[displayBlock.Entry.ColorKey];

			var formatter = new LogEntryRenderer(Typeface, FontSize, ForegroundBrush, Palette);
			displayBlock.Time = formatter.RenderEntry(displayBlock.TimeString, null, _view.ActualWidth,
				displayBlock.Foreground, BackgroundBrush,
				TextWrapping.NoWrap).FirstOrDefault();
			displayBlock.NameX = displayBlock.Time?.WidthIncludingTrailingWhitespace ?? 0.0;

			var blockForegroundBrush = displayBlock.Foreground;
			if (ColorizeNames && displayBlock.Entry.NameColorKey != 0)
			{
				blockForegroundBrush = MakeNameColor(displayBlock.Entry.NameColorKey);
			}

			var nickFg = blockForegroundBrush;
			var nickBg = BackgroundBrush;
			var forceNickBackground = false;
			if (highlightName)
			{
				nickFg = BackgroundBrush;
				nickBg = blockForegroundBrush;
				forceNickBackground = true;
			}
			displayBlock.Name = formatter.RenderEntry(displayBlock.FormattedName, null,
				_view.ActualWidth - displayBlock.NameX, nickFg, nickBg,
				TextWrapping.NoWrap, forceNickBackground).First();
			displayBlock.TextX = displayBlock.NameX + displayBlock.Name.WidthIncludingTrailingWhitespace;

			if (autoSize && displayBlock.TextX > NameColumnWidth)
			{
				NameColumnWidth = displayBlock.TextX;
			}

			if (JustifyNameColumn)
			{
				displayBlock.TextX = NameColumnWidth + SeparatorPadding * 2.0 + 1.0;
				displayBlock.NameX = NameColumnWidth - displayBlock.Name.WidthIncludingTrailingWhitespace;
			}

			displayBlock.Text = formatter.RenderEntry(displayBlock.Entry.Text, displayBlock.Entry,
				_view.ActualWidth - displayBlock.TextX, displayBlock.Foreground,
				BackgroundBrush, TextWrapping.Wrap).ToArray();
			displayBlock.Height = displayBlock.Text.Sum(t => t.Height);
		}

		private Brush MakeNameColor(int hashCode)
		{
			var rand = new Random(hashCode * (NameColorizerSeed + 1));
			var bgv = Math.Max(Math.Max(BackgroundColor.R, BackgroundColor.G), BackgroundColor.B) / 255f;

			var v = (float) rand.NextDouble() * NameBrightnessBand + (bgv < 0.5f ? (1f - NameBrightnessBand) : MinNameBrightness);
			var h = 360f * (float) rand.NextDouble();
			var s = .4f + (.6f * (float) rand.NextDouble());
			return new SolidColorBrush(new HsvColor(1f, h, s, v).ToColor());
		}
	}
}