/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace IrcSays.ColorPicker
{
	[TemplatePart(Name = PART_AvailableColors, Type = typeof (ListBox))]
	[TemplatePart(Name = PART_StandardColors, Type = typeof (ListBox))]
	[TemplatePart(Name = PART_RecentColors, Type = typeof (ListBox))]
	[TemplatePart(Name = PART_ColorPickerToggleButton, Type = typeof (ToggleButton))]
	[TemplatePart(Name = PART_ColorPickerPalettePopup, Type = typeof (Popup))]
	[TemplatePart(Name = PART_ColorModeButton, Type = typeof (Button))]
	public class ColorPicker : Control
	{
		private const string PART_AvailableColors = "PART_AvailableColors";
		private const string PART_StandardColors = "PART_StandardColors";
		private const string PART_RecentColors = "PART_RecentColors";
		private const string PART_ColorPickerToggleButton = "PART_ColorPickerToggleButton";
		private const string PART_ColorPickerPalettePopup = "PART_ColorPickerPalettePopup";
		private const string PART_ColorModeButton = "PART_ColorModeButton";


		private ListBox _availableColors;
		private ListBox _standardColors;
		private ListBox _recentColors;
		private ToggleButton _toggleButton;
		private Popup _popup;
		private Button _colorModeButton;
		private bool _selectionChanged;


		public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty.Register("AvailableColors",
			typeof (ObservableCollection<ColorItem>), typeof (ColorPicker), new UIPropertyMetadata(CreateAvailableColors()));

		public ObservableCollection<ColorItem> AvailableColors
		{
			get { return (ObservableCollection<ColorItem>) GetValue(AvailableColorsProperty); }
			set { SetValue(AvailableColorsProperty, value); }
		}


		public static readonly DependencyProperty AvailableColorsSortingModeProperty =
			DependencyProperty.Register("AvailableColorsSortingMode", typeof (ColorSortingMode), typeof (ColorPicker),
				new UIPropertyMetadata(ColorSortingMode.Alphabetical, OnAvailableColorsSortingModeChanged));

		public ColorSortingMode AvailableColorsSortingMode
		{
			get { return (ColorSortingMode) GetValue(AvailableColorsSortingModeProperty); }
			set { SetValue(AvailableColorsSortingModeProperty, value); }
		}

		private static void OnAvailableColorsSortingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var colorPicker = (ColorPicker) d;
			if (colorPicker != null)
			{
				colorPicker.OnAvailableColorsSortingModeChanged((ColorSortingMode) e.OldValue, (ColorSortingMode) e.NewValue);
			}
		}

		private void OnAvailableColorsSortingModeChanged(ColorSortingMode oldValue, ColorSortingMode newValue)
		{
			var lcv = (ListCollectionView) (CollectionViewSource.GetDefaultView(AvailableColors));
			if (lcv != null)
			{
				lcv.CustomSort = (AvailableColorsSortingMode == ColorSortingMode.HueSaturationBrightness)
					? new ColorSorter()
					: null;
			}
		}


		public static readonly DependencyProperty AvailableColorsHeaderProperty =
			DependencyProperty.Register("AvailableColorsHeader", typeof (string), typeof (ColorPicker),
				new UIPropertyMetadata("Available Colors"));

		public string AvailableColorsHeader
		{
			get { return (string) GetValue(AvailableColorsHeaderProperty); }
			set { SetValue(AvailableColorsHeaderProperty, value); }
		}


		public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register("ButtonStyle",
			typeof (Style), typeof (ColorPicker));

		public Style ButtonStyle
		{
			get { return (Style) GetValue(ButtonStyleProperty); }
			set { SetValue(ButtonStyleProperty, value); }
		}


		public static readonly DependencyProperty DisplayColorAndNameProperty =
			DependencyProperty.Register("DisplayColorAndName", typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(false));

		public bool DisplayColorAndName
		{
			get { return (bool) GetValue(DisplayColorAndNameProperty); }
			set { SetValue(DisplayColorAndNameProperty, value); }
		}


		public static readonly DependencyProperty ColorModeProperty = DependencyProperty.Register("ColorMode",
			typeof (ColorMode), typeof (ColorPicker), new UIPropertyMetadata(ColorMode.ColorPalette));

		public ColorMode ColorMode
		{
			get { return (ColorMode) GetValue(ColorModeProperty); }
			set { SetValue(ColorModeProperty, value); }
		}


		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof (bool),
			typeof (ColorPicker), new UIPropertyMetadata(false));

		public bool IsOpen
		{
			get { return (bool) GetValue(IsOpenProperty); }
			set { SetValue(IsOpenProperty, value); }
		}


		public static readonly DependencyProperty RecentColorsProperty = DependencyProperty.Register("RecentColors",
			typeof (ObservableCollection<ColorItem>), typeof (ColorPicker), new UIPropertyMetadata(null));

		public ObservableCollection<ColorItem> RecentColors
		{
			get { return (ObservableCollection<ColorItem>) GetValue(RecentColorsProperty); }
			set { SetValue(RecentColorsProperty, value); }
		}


		public static readonly DependencyProperty RecentColorsHeaderProperty =
			DependencyProperty.Register("RecentColorsHeader", typeof (string), typeof (ColorPicker),
				new UIPropertyMetadata("Recent Colors"));

		public string RecentColorsHeader
		{
			get { return (string) GetValue(RecentColorsHeaderProperty); }
			set { SetValue(RecentColorsHeaderProperty, value); }
		}


		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor",
			typeof (Color), typeof (ColorPicker),
			new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnSelectedColorPropertyChanged));

		public Color SelectedColor
		{
			get { return (Color) GetValue(SelectedColorProperty); }
			set { SetValue(SelectedColorProperty, value); }
		}

		private static void OnSelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var colorPicker = (ColorPicker) d;
			if (colorPicker != null)
			{
				colorPicker.OnSelectedColorChanged((Color) e.OldValue, (Color) e.NewValue);
			}
		}

		private void OnSelectedColorChanged(Color oldValue, Color newValue)
		{
			SelectedColorText = GetFormatedColorString(newValue);

			var args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
			args.RoutedEvent = SelectedColorChangedEvent;
			RaiseEvent(args);
		}


		public static readonly DependencyProperty SelectedColorTextProperty = DependencyProperty.Register(
			"SelectedColorText", typeof (string), typeof (ColorPicker), new UIPropertyMetadata("Black"));

		public string SelectedColorText
		{
			get { return (string) GetValue(SelectedColorTextProperty); }
			protected set { SetValue(SelectedColorTextProperty, value); }
		}


		public static readonly DependencyProperty ShowAdvancedButtonProperty =
			DependencyProperty.Register("ShowAdvancedButton", typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(true));

		public bool ShowAdvancedButton
		{
			get { return (bool) GetValue(ShowAdvancedButtonProperty); }
			set { SetValue(ShowAdvancedButtonProperty, value); }
		}


		public static readonly DependencyProperty ShowAvailableColorsProperty =
			DependencyProperty.Register("ShowAvailableColors", typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(true));

		public bool ShowAvailableColors
		{
			get { return (bool) GetValue(ShowAvailableColorsProperty); }
			set { SetValue(ShowAvailableColorsProperty, value); }
		}


		public static readonly DependencyProperty ShowRecentColorsProperty = DependencyProperty.Register("ShowRecentColors",
			typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(false));

		public bool ShowRecentColors
		{
			get { return (bool) GetValue(ShowRecentColorsProperty); }
			set { SetValue(ShowRecentColorsProperty, value); }
		}


		public static readonly DependencyProperty ShowStandardColorsProperty =
			DependencyProperty.Register("ShowStandardColors", typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(true));

		public bool ShowStandardColors
		{
			get { return (bool) GetValue(ShowStandardColorsProperty); }
			set { SetValue(ShowStandardColorsProperty, value); }
		}


		public static readonly DependencyProperty ShowDropDownButtonProperty =
			DependencyProperty.Register("ShowDropDownButton", typeof (bool), typeof (ColorPicker), new UIPropertyMetadata(true));

		public bool ShowDropDownButton
		{
			get { return (bool) GetValue(ShowDropDownButtonProperty); }
			set { SetValue(ShowDropDownButtonProperty, value); }
		}


		public static readonly DependencyProperty StandardColorsProperty = DependencyProperty.Register("StandardColors",
			typeof (ObservableCollection<ColorItem>), typeof (ColorPicker), new UIPropertyMetadata(CreateStandardColors()));

		public ObservableCollection<ColorItem> StandardColors
		{
			get { return (ObservableCollection<ColorItem>) GetValue(StandardColorsProperty); }
			set { SetValue(StandardColorsProperty, value); }
		}


		public static readonly DependencyProperty StandardColorsHeaderProperty =
			DependencyProperty.Register("StandardColorsHeader", typeof (string), typeof (ColorPicker),
				new UIPropertyMetadata("Standard Colors"));

		public string StandardColorsHeader
		{
			get { return (string) GetValue(StandardColorsHeaderProperty); }
			set { SetValue(StandardColorsHeaderProperty, value); }
		}


		public static readonly DependencyProperty UsingAlphaChannelProperty = DependencyProperty.Register(
			"UsingAlphaChannel", typeof (bool), typeof (ColorPicker),
			new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnUsingAlphaChannelPropertyChanged));

		public bool UsingAlphaChannel
		{
			get { return (bool) GetValue(UsingAlphaChannelProperty); }
			set { SetValue(UsingAlphaChannelProperty, value); }
		}

		private static void OnUsingAlphaChannelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var colorPicker = (ColorPicker) d;
			if (colorPicker != null)
			{
				colorPicker.OnUsingAlphaChannelChanged();
			}
		}

		private void OnUsingAlphaChannelChanged()
		{
			SelectedColorText = GetFormatedColorString(SelectedColor);
		}


		static ColorPicker()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (ColorPicker), new FrameworkPropertyMetadata(typeof (ColorPicker)));
		}

		public ColorPicker()
		{
			RecentColors = new ObservableCollection<ColorItem>();
			Keyboard.AddKeyDownHandler(this, OnKeyDown);
			Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
		}


		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_availableColors != null)
			{
				_availableColors.SelectionChanged -= Color_SelectionChanged;
			}

			_availableColors = GetTemplateChild(PART_AvailableColors) as ListBox;
			if (_availableColors != null)
			{
				_availableColors.SelectionChanged += Color_SelectionChanged;
			}

			if (_standardColors != null)
			{
				_standardColors.SelectionChanged -= Color_SelectionChanged;
			}

			_standardColors = GetTemplateChild(PART_StandardColors) as ListBox;
			if (_standardColors != null)
			{
				_standardColors.SelectionChanged += Color_SelectionChanged;
			}

			if (_recentColors != null)
			{
				_recentColors.SelectionChanged -= Color_SelectionChanged;
			}

			_recentColors = GetTemplateChild(PART_RecentColors) as ListBox;
			if (_recentColors != null)
			{
				_recentColors.SelectionChanged += Color_SelectionChanged;
			}

			if (_popup != null)
			{
				_popup.Opened -= Popup_Opened;
			}

			_popup = GetTemplateChild(PART_ColorPickerPalettePopup) as Popup;
			if (_popup != null)
			{
				_popup.Opened += Popup_Opened;
			}

			_toggleButton = Template.FindName(PART_ColorPickerToggleButton, this) as ToggleButton;

			if (_colorModeButton != null)
			{
				_colorModeButton.Click -= ColorModeButton_Clicked;
			}

			_colorModeButton = Template.FindName(PART_ColorModeButton, this) as Button;

			if (_colorModeButton != null)
			{
				_colorModeButton.Click += ColorModeButton_Clicked;
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			// Close ColorPicker on MouseUp to prevent action of mouseUp on controls behind the ColorPicker.
			if (_selectionChanged)
			{
				CloseColorPicker(true);
				_selectionChanged = false;
			}
		}


		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (!IsOpen)
			{
				if (KeyboardUtilities.IsKeyModifyingPopupState(e))
				{
					IsOpen = true;
					// Focus will be on ListBoxItem in Popup_Opened().
					e.Handled = true;
				}
			}
			else
			{
				if (KeyboardUtilities.IsKeyModifyingPopupState(e))
				{
					CloseColorPicker(true);
					e.Handled = true;
				}
				else if (e.Key == Key.Escape)
				{
					CloseColorPicker(true);
					e.Handled = true;
				}
			}
		}

		private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
		{
			CloseColorPicker(false);
		}

		private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var lb = (ListBox) sender;

			if (e.AddedItems.Count > 0)
			{
				var colorItem = (ColorItem) e.AddedItems[0];
				SelectedColor = colorItem.Color;
				UpdateRecentColors(colorItem);
				_selectionChanged = true;
				lb.SelectedIndex = -1; //for now I don't care about keeping track of the selected color
			}
		}

		private void Popup_Opened(object sender, EventArgs e)
		{
			if ((_availableColors != null) && ShowAvailableColors)
			{
				FocusOnListBoxItem(_availableColors);
			}
			else if ((_standardColors != null) && ShowStandardColors)
			{
				FocusOnListBoxItem(_standardColors);
			}
			else if ((_recentColors != null) && ShowRecentColors)
			{
				FocusOnListBoxItem(_recentColors);
			}
		}

		private void FocusOnListBoxItem(ListBox listBox)
		{
			var listBoxItem = (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem);
			if ((listBoxItem == null) &&
				(listBox.Items.Count > 0))
			{
				listBoxItem = (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[0]);
			}
			if (listBoxItem != null)
			{
				listBoxItem.Focus();
			}
		}

		private void ColorModeButton_Clicked(object sender, RoutedEventArgs e)
		{
			ColorMode = (ColorMode == ColorMode.ColorPalette) ? ColorMode.ColorCanvas : ColorMode.ColorPalette;
		}


		public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectedColorChanged", RoutingStrategy.Bubble, typeof (RoutedPropertyChangedEventHandler<Color>),
			typeof (ColorPicker));

		public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
		{
			add { AddHandler(SelectedColorChangedEvent, value); }
			remove { RemoveHandler(SelectedColorChangedEvent, value); }
		}


		private void CloseColorPicker(bool isFocusOnColorPicker)
		{
			if (IsOpen)
			{
				IsOpen = false;
			}
			ReleaseMouseCapture();

			if (isFocusOnColorPicker && (_toggleButton != null))
			{
				_toggleButton.Focus();
			}
			UpdateRecentColors(new ColorItem(SelectedColor, SelectedColorText));
		}

		private void UpdateRecentColors(ColorItem colorItem)
		{
			if (!RecentColors.Contains(colorItem))
			{
				RecentColors.Add(colorItem);
			}

			if (RecentColors.Count > 10) //don't allow more than ten, maybe make a property that can be set by the user.
			{
				RecentColors.RemoveAt(0);
			}
		}

		private string GetFormatedColorString(Color colorToFormat)
		{
			return ColorUtilities.FormatColorString(colorToFormat.GetColorName(), UsingAlphaChannel);
		}

		private static ObservableCollection<ColorItem> CreateStandardColors()
		{
			var standardColors = new ObservableCollection<ColorItem>
			{
				new ColorItem(Colors.Transparent, "Transparent"),
				new ColorItem(Colors.White, "White"),
				new ColorItem(Colors.Gray, "Gray"),
				new ColorItem(Colors.Black, "Black"),
				new ColorItem(Colors.Red, "Red"),
				new ColorItem(Colors.Green, "Green"),
				new ColorItem(Colors.Blue, "Blue"),
				new ColorItem(Colors.Yellow, "Yellow"),
				new ColorItem(Colors.Orange, "Orange"),
				new ColorItem(Colors.Purple, "Purple")
			};
			return standardColors;
		}

		private static ObservableCollection<ColorItem> CreateAvailableColors()
		{
			var standardColors = new ObservableCollection<ColorItem>();

			foreach (var item in ColorUtilities.KnownColors)
			{
				if (!String.Equals(item.Key, "Transparent"))
				{
					var colorItem = new ColorItem(item.Value, item.Key);
					if (!standardColors.Contains(colorItem))
					{
						standardColors.Add(colorItem);
					}
				}
			}

			return standardColors;
		}
	}
}