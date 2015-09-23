using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace IrcSays.Ui
{
	public partial class ChatPresenter : ChatBoxBase, IScrollInfo
	{
		private ScrollViewer _viewer;

		public ChatPresenter()
		{
			Loaded += HandleLoad;
			Unloaded += HandleUnload;
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
			_blocks = new LinkedList<DisplayBlock>();
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
				e.Property == UseTabularViewProperty ||
				e.Property == ColorizeNicknamesProperty ||
				e.Property == NewMarkerColorProperty ||
				e.Property == NicknameColorSeedProperty ||
				e.Property == DividerBrushProperty ||
				e.Property == BackgroundProperty ||
				e.Property == HighlightColorProperty)
			{
				InvalidateAll(true);
			}

			base.OnPropertyChanged(e);
		}

		public void PurgeMessages(string nick, bool removeAll)
		{
			foreach (var block in _blocks.Where(b => b.NickText == nick).ToArray())
			{
				_blocks.Remove(block);
			}

			if (removeAll)
			{
				foreach (var block in _blocks
					.Where(b => b.Source.IsDirectedMessage &&
						b.Source.Text.StartsWith(nick,StringComparison.OrdinalIgnoreCase))
						.ToArray())
				{
					_blocks.Remove(block);
				}
			}

			InvalidateAll(false);
		}
	}
}