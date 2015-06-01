using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IrcSays.Configuration;

namespace IrcSays.Ui
{
	public class ChatBoxBase : Control
	{
		public static readonly DependencyProperty BufferLinesProperty = DependencyProperty.Register("BufferLines",
			typeof (int), typeof (ChatBoxBase));

		public int BufferLines
		{
			get { return (int) GetValue(BufferLinesProperty); }
			set { SetValue(BufferLinesProperty, value); }
		}

		public static readonly DependencyProperty MinimumCopyLengthProperty = DependencyProperty.Register("MinimumCopyLength",
			typeof (int), typeof (ChatBoxBase));

		public int MinimumCopyLength
		{
			get { return (int) GetValue(MinimumCopyLengthProperty); }
			set { SetValue(MinimumCopyLengthProperty, value); }
		}

		public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register("Palette",
			typeof (IDictionary<string, Brush>), typeof (ChatBoxBase));

		public IDictionary<string, Brush> Palette
		{
			get { return (ChatPalette) GetValue(PaletteProperty); }
			set { SetValue(PaletteProperty, value); }
		}

		public static readonly DependencyProperty ShowTimestampProperty = DependencyProperty.Register("ShowTimestamp",
			typeof (bool), typeof (ChatBoxBase));

		public bool ShowTimestamp
		{
			get { return (bool) GetValue(ShowTimestampProperty); }
			set { SetValue(ShowTimestampProperty, value); }
		}

		public static readonly DependencyProperty TimestampFormatProperty = DependencyProperty.Register("TimestampFormat",
			typeof (string), typeof (ChatBoxBase));

		public string TimestampFormat
		{
			get { return (string) GetValue(TimestampFormatProperty); }
			set { SetValue(TimestampFormatProperty, value); }
		}

		public static readonly DependencyProperty UseTabularViewProperty = DependencyProperty.Register("UseTabularView",
			typeof (bool), typeof (ChatBoxBase));

		public bool UseTabularView
		{
			get { return (bool) GetValue(UseTabularViewProperty); }
			set { SetValue(UseTabularViewProperty, value); }
		}

		public static readonly DependencyProperty ColorizeNicknamesProperty = DependencyProperty.Register("ColorizeNicknames",
			typeof (bool), typeof (ChatBoxBase));

		public bool ColorizeNicknames
		{
			get { return (bool) GetValue(ColorizeNicknamesProperty); }
			set { SetValue(ColorizeNicknamesProperty, value); }
		}

		public static readonly DependencyProperty NicknameColorSeedProperty = DependencyProperty.Register("NicknameColorSeed",
			typeof (int), typeof (ChatBoxBase));

		public int NicknameColorSeed
		{
			get { return (int) GetValue(NicknameColorSeedProperty); }
			set { SetValue(NicknameColorSeedProperty, value); }
		}

		public static readonly DependencyProperty NewMarkerColorProperty = DependencyProperty.Register("NewMarkerColor",
			typeof (Color), typeof (ChatBoxBase));

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

		public static readonly DependencyProperty OldMarkerColorProperty = DependencyProperty.Register("OldMarkerColor",
			typeof (Color), typeof (ChatBoxBase));

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

		public static readonly DependencyProperty AttentionColorProperty = DependencyProperty.Register("AttentionColor",
			typeof (Color), typeof (ChatBoxBase));

		public Color AttentionColor
		{
			get { return (Color) GetValue(AttentionColorProperty); }
			set { SetValue(AttentionColorProperty, value); }
		}

		public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register("HighlightColor",
			typeof (Color), typeof (ChatBoxBase));

		public Color HighlightColor
		{
			get { return (Color) GetValue(HighlightColorProperty); }
			set { SetValue(HighlightColorProperty, value); }
		}

		public static readonly DependencyProperty DividerBrushProperty = DependencyProperty.Register("DividerBrush",
			typeof (Brush), typeof (ChatBoxBase));

		public Brush DividerBrush
		{
			get { return (Brush) GetValue(DividerBrushProperty); }
			set { SetValue(DividerBrushProperty, value); }
		}

		public static readonly DependencyProperty SelectedLinkProperty = DependencyProperty.Register("SelectedLink",
			typeof (string), typeof (ChatBoxBase));

		public string SelectedLink
		{
			get { return (string) GetValue(SelectedLinkProperty); }
			set { SetValue(SelectedLinkProperty, value); }
		}

		public static readonly DependencyProperty AutoSizeColumnProperty = DependencyProperty.Register("AutoSizeColumn",
			typeof (bool), typeof (ChatBoxBase));

		public bool AutoSizeColumn
		{
			get { return (bool) GetValue(AutoSizeColumnProperty); }
			set { SetValue(AutoSizeColumnProperty, value); }
		}

		public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth",
			typeof (double), typeof (ChatBoxBase));

		public double ColumnWidth
		{
			get { return (double) GetValue(ColumnWidthProperty); }
			set { SetValue(ColumnWidthProperty, value); }
		}
	}
}