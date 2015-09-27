using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Std.Ui.Logging
{
	public partial class LogView : Control, IScrollInfo
	{
		private ScrollViewer _viewer;

		private IDisplayBlockFormatter _blockFormatter;
		private ISearchProvider _searchProvider;

		private const int TextProcessingBatchSize = 50;

		public IntrusiveLinkedList<DisplayBlock> Blocks { get; private set; }
		private double _lineHeight;
		public DisplayBlock BottomBlock { get; private set; }
		private DisplayBlock _curBlock;
		private int _curLine;
		private bool _isProcessingText;
		private Typeface _typeface;

		public LogView()
		{
			_typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
			Blocks = new IntrusiveLinkedList<DisplayBlock>();
			DefaultStyleKey = typeof(LogView);
			Loaded += HandleLoad;
			Unloaded += HandleUnload;
		}

		public void Initialize(IDisplayBlockFormatter blockFormatter, ISearchProvider searchProvider)
		{
			_blockFormatter = blockFormatter;
			_blockFormatter.Attach(this);
			_searchProvider = searchProvider;
			_searchProvider.Attach(this);
		}

		private void HandleUnload(object sender, RoutedEventArgs e)
		{
			_isSelecting = false;
			_isDragging = false;
		}

		private void HandleLoad(object sender, RoutedEventArgs e)
		{
			if (_isAutoScrolling)
			{
				ScrollToEnd();
			}

			if (_selectBrush == null)
			{
				var c = HighlightColor;
				c.A = 102;
				_selectBrush = new SolidColorBrush(c);
			}
		}

		public void Clear()
		{
			Blocks = new IntrusiveLinkedList<DisplayBlock>();
			_curBlock = null;
			_bufferLines = 0;
			_isAutoScrolling = true;
			InvalidateScrollInfo();
			InvalidateVisual();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == FontFamilyProperty ||
				e.Property == FontSizeProperty ||
				e.Property == FontStyleProperty ||
				e.Property == FontWeightProperty ||
				e.Property == PaletteProperty ||
				e.Property == ShowTimestampProperty ||
				e.Property == TimestampFormatProperty ||
				e.Property == JustifyNameColumnProperty ||
				e.Property == ColorizeNamesProperty ||
				e.Property == NewMarkerColorProperty ||
				e.Property == NameColorizerSeedProperty ||
				e.Property == DividerBrushProperty ||
				e.Property == BackgroundProperty ||
				e.Property == HighlightColorProperty)
			{
				InvalidateAll(true);
			}

			base.OnPropertyChanged(e);
		}

		public void PurgeLogEntries(string entryName, bool removeAll)
		{
			foreach (var block in Blocks.Where(b => b.NameText == entryName).ToArray())
			{
				Blocks.Remove(block);
			}

			if (removeAll)
			{
				foreach (var block in Blocks
					.Where(b => b.Entry.IsDirectedMessage &&
						b.Entry.Text.StartsWith(entryName, StringComparison.OrdinalIgnoreCase))
						.ToArray())
				{
					Blocks.Remove(block);
				}
			}

			InvalidateAll(false);
		}
	}
}