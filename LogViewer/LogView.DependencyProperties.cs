using System.Windows;
using System.Windows.Media;

namespace Std.Ui.Logging
{
	public partial class LogView
	{
		public static readonly DependencyProperty BufferLinesProperty = 
			DependencyProperty.Register(nameof(BufferLines),
			typeof (int), typeof (LogView));

		public static readonly DependencyProperty MinimumCopyLengthProperty =
			DependencyProperty.Register(nameof(MinimumCopyLength),
				typeof (int), typeof (LogView));

		public static readonly DependencyProperty PaletteProperty =
			DependencyProperty.Register(nameof(Palette),
			typeof (LogEntryPalette), typeof (LogView));

		public static readonly DependencyProperty ShowTimestampProperty = 
			DependencyProperty.Register(nameof(ShowTimestamp),
			typeof (bool), typeof (LogView));

		public static readonly DependencyProperty TimestampFormatProperty =
			DependencyProperty.Register(nameof(TimestampFormat),
				typeof (string), typeof (LogView));

		public static readonly DependencyProperty JustifyNameColumnProperty = 
			DependencyProperty.Register(nameof(JustifyNameColumn),
			typeof (bool), typeof (LogView));

		public static readonly DependencyProperty ColorizeNamesProperty =
			DependencyProperty.Register(nameof(ColorizeNames),
				typeof (bool), typeof (LogView));

		public static readonly DependencyProperty NameColorizerSeedProperty =
			DependencyProperty.Register(nameof(NameColorizerSeed),
				typeof (int), typeof (LogView));

		public static readonly DependencyProperty NewMarkerColorProperty =
			DependencyProperty.Register(nameof(NewMarkerColor),
				typeof (Color), typeof (LogView));

		public static readonly DependencyProperty OldMarkerColorProperty =
			DependencyProperty.Register(nameof(OldMarkerColor),
			typeof (Color), typeof (LogView));

		public static readonly DependencyProperty AttentionColorProperty = 
			DependencyProperty.Register(nameof(AttentionColor),
			typeof (Color), typeof (LogView));

		public static readonly DependencyProperty HighlightColorProperty = 
			DependencyProperty.Register(nameof(HighlightColor),
			typeof (Color), typeof (LogView));

		public static readonly DependencyProperty DividerBrushProperty =
			DependencyProperty.Register(nameof(DividerBrush),
			typeof (Brush), typeof (LogView));

		public static readonly DependencyProperty SelectedLinkProperty = 
			DependencyProperty.Register(nameof(SelectedLink),
			typeof (string), typeof (LogView));

		public static readonly DependencyProperty AutoSizeColumnProperty = 
			DependencyProperty.Register(nameof(AutoSizeColumn),
			typeof (bool), typeof (LogView));

		public static readonly DependencyProperty NameColumnWidthProperty =
			DependencyProperty.Register(nameof(NameColumnWidth),
			typeof (double), typeof (LogView));

		public int BufferLines
		{
			get { return (int) GetValue(BufferLinesProperty); }
			set { SetValue(BufferLinesProperty, value); }
		}

		public int MinimumCopyLength
		{
			get { return (int) GetValue(MinimumCopyLengthProperty); }
			set { SetValue(MinimumCopyLengthProperty, value); }
		}

		public LogEntryPalette Palette
		{
			get { return (LogEntryPalette) GetValue(PaletteProperty); }
			set
			{
				_blockFormatter.Palette = value;
				SetValue(PaletteProperty, value);
			}
		}

		public bool ShowTimestamp
		{
			get { return (bool) GetValue(ShowTimestampProperty); }
			set
			{
				_blockFormatter.ShowTimestamp = value;
				SetValue(ShowTimestampProperty, value);
			}
		}

		public string TimestampFormat
		{
			get { return (string) GetValue(TimestampFormatProperty); }
			set
			{
				_blockFormatter.TimestampFormat = value;
				SetValue(TimestampFormatProperty, value);
			}
		}

		public bool JustifyNameColumn
		{
			get { return (bool) GetValue(JustifyNameColumnProperty); }
			set
			{
				_blockFormatter.JustifyNameColumn = value;
				SetValue(JustifyNameColumnProperty, value);
			}
		}

		public bool ColorizeNames
		{
			get { return (bool) GetValue(ColorizeNamesProperty); }
			set
			{
				_blockFormatter.ColorizeNames = value;
				SetValue(ColorizeNamesProperty, value);
			}
		}

		public int NameColorizerSeed
		{
			get { return (int) GetValue(NameColorizerSeedProperty); }
			set
			{
				_blockFormatter.NameColorizerSeed = value;
				SetValue(NameColorizerSeedProperty, value);
			}
		}

		public Color NewMarkerColor
		{
			get { return (Color) GetValue(NewMarkerColorProperty); }
			set { SetValue(NewMarkerColorProperty, value); }
		}

		protected Color NewMarkerTransparentColor
		{
			get
			{
				var c = NewMarkerColor;
				return Color.FromArgb(0, c.R, c.G, c.B);
			}
		}

		public Color OldMarkerColor
		{
			get { return (Color) GetValue(OldMarkerColorProperty); }
			set { SetValue(OldMarkerColorProperty, value); }
		}

		protected Color OldMarkerTransparentColor
		{
			get
			{
				var c = OldMarkerColor;
				return Color.FromArgb(0, c.R, c.G, c.B);
			}
		}

		public Color AttentionColor
		{
			get { return (Color) GetValue(AttentionColorProperty); }
			set { SetValue(AttentionColorProperty, value); }
		}

		public Color HighlightColor
		{
			get { return (Color) GetValue(HighlightColorProperty); }
			set { SetValue(HighlightColorProperty, value); }
		}

		public Brush DividerBrush
		{
			get { return (Brush) GetValue(DividerBrushProperty); }
			set { SetValue(DividerBrushProperty, value); }
		}

		public string SelectedLink
		{
			get { return (string) GetValue(SelectedLinkProperty); }
			set { SetValue(SelectedLinkProperty, value); }
		}

		public bool AutoSizeColumn
		{
			get { return (bool) GetValue(AutoSizeColumnProperty); }
			set { SetValue(AutoSizeColumnProperty, value); }
		}

		public double NameColumnWidth
		{
			get { return (double) GetValue(NameColumnWidthProperty); }
			set
			{
				_blockFormatter.NameColumnWidth = value;
				SetValue(NameColumnWidthProperty, value);
			}
		}
	}
}