using System;
using System.Windows.Media;

namespace Std.Ui.Logging
{
	public interface IDisplayBlockFormatter
	{
		Brush ForegroundBrush { get; set; }
		Brush BackgroundBrush { get; set; }
		Color BackgroundColor { get; set; }
		bool ColorizeNames { get; set; }
		int NameColorizerSeed { get; set; }
		bool JustifyNameColumn { get; set; }
		double NameColumnWidth { get; set; }
		LogEntryPalette Palette { get; set; }
		double SeparatorPadding { get; set; }
		Typeface Typeface { get; set; }
		double FontSize { get; set; }
		bool ShowTimestamp { get; set; }
		string TimestampFormat { get; set; }

		void Attach(LogView view);

        string FormatName(string name);
		string FormatTimestamp(DateTime? timestamp);

		void FormatBlock(DisplayBlock displayBlock, bool autoSize, bool highlightName = false);
	}
}