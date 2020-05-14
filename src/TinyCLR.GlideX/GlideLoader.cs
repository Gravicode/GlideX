////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using TinyCLR.GlideX.Properties;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;
using GHI.GlideX.Display;

namespace GHI.GlideX
{
    /// <summary>
    /// The GlideLoader class parses XML into components.
    /// </summary>
    public static class GlideLoader
    {
        private static Font _font = Resources.GetFont(Resources.FontResources.droid_reg10);
        private static DisplayObject _defaultDisplayObject = new DisplayObject();
        private static Window _window = null;
        private static Canvas _canvas = null;
        /// <summary>
        /// Loads a Window from an XML string.
        /// </summary>
        /// <param name="xmlStr">XML string.</param>
        /// <returns>Window object.</returns>
        public static Window LoadWindow(string xmlStr)
        {
            StringBuilder sb = new StringBuilder(xmlStr);
            sb = sb.Replace("\n", String.Empty);
            sb = sb.Replace("\r", String.Empty);
            sb = sb.Replace("\t", String.Empty);
            return LoadWindow(UTF8Encoding.UTF8.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Loads a Window from XML bytes.
        /// </summary>
        /// <param name="xmlBytes">XML bytes.</param>
        /// <returns>Window object.</returns>
        public static Window LoadWindow(byte[] xmlBytes)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.IgnoreProcessingInstructions = true;
            xmlReaderSettings.IgnoreWhitespace = true;
            
            MemoryStream stream = new MemoryStream(xmlBytes);

            XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);


            if(!reader.ReadToDescendant("Glide"))
                throw new ArgumentException("Glide not detected.");

            stream.Seek(0, SeekOrigin.Begin);
            reader = XmlReader.Create(stream, xmlReaderSettings);

            if (!reader.ReadToDescendant("Window"))
                throw new ArgumentException("XML does not contain a Window element.");

            _window = ParseWindow(reader);

            UIElement component;
            _canvas = new Canvas();

            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Window"))
            {
                component = GetComponent(reader);

                if (component != null)
                {
                    _canvas.Children.Add(component);
                }
                else
                    throw new Exception(reader.Name + " is not a valid UI component.");
            }
            _window.Child = _canvas;

            reader.Close();

            _window.Invalidate();

            return _window;
        }

        /// <summary>
        /// Parses a UI component from the XML.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>UIElement</returns>
        private static UIElement GetComponent(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "Button":
                    return LoadButton(reader);

                case "CheckBox":
                    return LoadCheckBox(reader);

                case "Dropdown":
                    return LoadDropdown(reader);

                case "DataGrid":
                    //not supported
                    return null;
                    //return LoadDataGrid(reader);

                case "Image":
                    return LoadImage(reader);

                case "PasswordBox":
                    return LoadPasswordBox(reader);

                case "ProgressBar":
                    return LoadProgressBar(reader);

                case "RadioButton":
                    return LoadRadioButton(reader);

                case "Slider":
                    //not supported
                    return null;
                    //return LoadSlider(reader);

                case "TextBlock":
                    return LoadTextBlock(reader);

                case "TextBox":
                    return LoadTextBox(reader);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Parses the Window XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Window</returns>
        private static Window ParseWindow(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));

            if (GlideX.FitToScreen && (width < GlideX.LCD.Width || height < GlideX.LCD.Height))
            {
                width = GlideX.LCD.Width;
                height = GlideX.LCD.Height;
            }

            var backColor = GlideUtils.Convert.ToColor(reader.GetAttribute("BackColor"));

            var window = new Window() { 
                            
                Height = height,
                Width = width
            };
            
            window.Background = new SolidColorBrush(backColor);
            //window.BackColor = backColor;
            window.Visibility = Visibility.Visible;

            return window;
        }

        /// <summary>
        /// Parses the Button XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Button object.</returns>
        private static Button LoadButton(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            ushort alpha = Convert.ToUInt16(reader.GetAttribute("Alpha"));

            string text = reader.GetAttribute("Text");
            int i = text.IndexOf("\\n");
            while (i > -1)
            {
                text = text.Substring(0, i) + "\n" + text.Substring(i + 2, text.Length - (i + 2));
                i = text.IndexOf("\\n");
            }

            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            var fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            var disabledFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("DisabledFontColor"));
            var tintColor = GlideUtils.Convert.ToColor(reader.GetAttribute("TintColor"));
            ushort tintAmount = Convert.ToUInt16(reader.GetAttribute("TintAmount"));
            var txt = new Text(font, text)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                ForeColor = fontColor,
                
            };
            var button = new Button()
            {   
                Child = txt,
                Width = width,
                Height = height,
                Foreground = new SolidColorBrush(tintColor),
                Alpha = alpha
            };
            
            Canvas.SetLeft(button, x);
            Canvas.SetTop(button, y);
            /*
            Button button = new Button(name, alpha, x, y, width, height);
            button.Text = text;
            button.Font = font;
            button.FontColor = fontColor;
            button.DisabledFontColor = disabledFontColor;
            button.TintColor = tintColor;
            button.TintAmount = tintAmount;
            */
            return button;
        }

        /// <summary>
        /// Parses the CheckBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>CheckBox object.</returns>
        private static CheckBox LoadCheckBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            bool _checked = (reader.GetAttribute("Checked") == bool.TrueString) ? true : false;

            CheckBox checkBox = new CheckBox();
            //name, alpha, x, y);
            checkBox.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            checkBox.IsEnabled = enabled;
            checkBox.IsChecked = _checked;
            checkBox.Name = name;

            Canvas.SetLeft(checkBox, x);
            Canvas.SetTop(checkBox, y);
            return checkBox;
        }

        /// <summary>
        /// Parses the Dropdown XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Dropdown object.</returns>
        private static Dropdown LoadDropdown(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            var fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            var txt = new Text(font, text)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                ForeColor = fontColor,

            };
            Dropdown dropdown = new Dropdown();// name, alpha, x, y, width, height);
            dropdown.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            dropdown.IsEnabled = enabled;
            dropdown.Child = txt;
            //dropdown.Text = text;
            dropdown.Font = font;
            //dropdown.FontColor = fontColor;

            if (!reader.IsEmptyElement)
            {
                dropdown.Options = new ArrayList();
                while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Dropdown"))
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Option")
                    {
                        // Apparently if you readstring before getattribute you'll lose position and it cannot find the attribute.
                        string value = reader.GetAttribute("Value");
                        object[] item = new object[2] { reader.ReadString(), value };
                        dropdown.Options.Add(item);
                    }
                }
            }
            Canvas.SetLeft(dropdown, x);
            Canvas.SetTop(dropdown, y);
            return dropdown;
        }
        /*
        /// <summary>
        /// Parses the DataGrid XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Datagrid object.</returns>
        private static DataGrid LoadDataGrid(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            int rowHeight = Convert.ToInt32(reader.GetAttribute("RowHeight"));
            int rowCount = Convert.ToInt32(reader.GetAttribute("RowCount"));

            bool draggable = (reader.GetAttribute("Draggable") == bool.TrueString) ? true : false;
            bool tappableCells = (reader.GetAttribute("TappableCells") == bool.TrueString) ? true : false;
            bool sortableHeaders = (reader.GetAttribute("SortableHeaders") == bool.TrueString) ? true : false;
            bool showHeaders = (reader.GetAttribute("ShowHeaders") == bool.TrueString) ? true : false;
            bool showScrollbar = (reader.GetAttribute("ShowScrollbar") == bool.TrueString) ? true : false;
            int scrollbarWidth = Convert.ToInt32(reader.GetAttribute("ScrollbarWidth"));

            Color headersBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("HeadersBackColor"));
            Color headersFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("HeadersFontColor"));
            Color itemsBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsBackColor"));
            Color itemsAltBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsAltBackColor"));
            Color itemsFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ItemsFontColor"));
            Color selectedItemBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedItemBackColor"));
            Color selectedItemFontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedItemFontColor"));
            Color gridColor = GlideUtils.Convert.ToColor(reader.GetAttribute("GridColor"));
            Color scrollbarBackColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ScrollbarBackColor"));
            Color scrollbarScrubberColor = GlideUtils.Convert.ToColor(reader.GetAttribute("ScrollbarScrubberColor"));

            DataGrid dataGrid = new DataGrid(name, alpha, x, y, width, rowHeight, rowCount);
            dataGrid.Visible = visible;
            dataGrid.Enabled = enabled;
            dataGrid.Font = font;

            dataGrid.Draggable = draggable;
            dataGrid.TappableCells = tappableCells;
            dataGrid.SortableHeaders = sortableHeaders;
            dataGrid.ShowHeaders = showHeaders;
            dataGrid.ShowScrollbar = showScrollbar;
            dataGrid.ScrollbarWidth = scrollbarWidth;

            dataGrid.HeadersBackColor = headersBackColor;
            dataGrid.HeadersFontColor = headersFontColor;
            dataGrid.ItemsBackColor = itemsBackColor;
            dataGrid.ItemsAltBackColor = itemsAltBackColor;
            dataGrid.ItemsFontColor = itemsFontColor;
            dataGrid.SelectedItemBackColor = selectedItemBackColor;
            dataGrid.SelectedItemFontColor = selectedItemFontColor;
            dataGrid.GridColor = gridColor;
            dataGrid.ShowScrollbar = showScrollbar;
            dataGrid.ScrollbarWidth = scrollbarWidth;

            return dataGrid;
        }
        */
        /// <summary>
        /// Parses the Image XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Image object.</returns>
        private static GHIElectronics.TinyCLR.UI.Controls.Image LoadImage(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            var image = new GHIElectronics.TinyCLR.UI.Controls.Image();// name, alpha, x, y, width, height);
            
            image.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            image.IsEnabled = enabled;
            image.Width = width;
            image.Height = height;

            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);

            return image;
        }

        /// <summary>
        /// Parses the PasswordBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>PasswordBox object.</returns>
        private static TextBox LoadPasswordBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            uint textAlign = GlideUtils.Convert.ToAlignment(reader.GetAttribute("TextAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            var fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            var passwordBox = new TextBox
            {
                Font = font,
                Text = text,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(fontColor),
                
                
            };
            //PasswordBox passwordBox = new PasswordBox(name, alpha, x, y, width, height);
            passwordBox.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            passwordBox.IsEnabled = enabled;
            passwordBox.TextAlign = FromAlignment(textAlign);
            passwordBox.Font = font;
            //passwordBox.FontColor = fontColor;

            Canvas.SetLeft(passwordBox, x);
            Canvas.SetTop(passwordBox, y);
            return passwordBox;
        }
        static TextAlignment FromAlignment(uint Alignment)
        {
            var format = new TextAlignment();
            switch (Alignment)
            {
                case TinyCLR.GlideX.BitmapHelper.DT_AlignmentCenter:
                    format = TextAlignment.Center; 
                    break;
                case TinyCLR.GlideX.BitmapHelper.DT_AlignmentLeft:
                    format = TextAlignment.Left;
                    break;
                case TinyCLR.GlideX.BitmapHelper.DT_AlignmentRight:
                    format = TextAlignment.Right;
                    break;
                default: throw new ArgumentException();
            }
            return format;
        }
        /// <summary>
        /// Parses the ProgressBar XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>ProgressBar object.</returns>
        private static ProgressBar LoadProgressBar(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            Direction direction;
            switch (reader.GetAttribute("Direction"))
            {
                case "Up":
                    direction = Direction.Up;
                    break;
                case "Left":
                    direction = Direction.Left;
                    break;
                case "Down":
                    direction = Direction.Down;
                    break;
                case "Right":
                default:
                    direction = Direction.Right;
                    break;
            }
            int maxValue = Convert.ToInt32(reader.GetAttribute("MaxValue"));
            int value = Convert.ToInt32(reader.GetAttribute("Value"));

            ProgressBar progressBar = new ProgressBar();// name, alpha, x, y, width, height);
            progressBar.Width = width;
            progressBar.Height = height;
            progressBar.Alpha = alpha;
            progressBar.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            progressBar.IsEnabled = enabled;
            progressBar.Direction = direction;
            progressBar.MaxValue = maxValue;
            progressBar.Value = value;

            Canvas.SetLeft(progressBar, x);
            Canvas.SetTop(progressBar, y);
            return progressBar;
        }

        /// <summary>
        /// Parses the RadioButton XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>RadioButton object.</returns>
        private static RadioButton LoadRadioButton(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            ushort alpha = Convert.ToUInt16(reader.GetAttribute("Alpha"));

            string value = reader.GetAttribute("Value");
            bool _checked = (reader.GetAttribute("Checked") == bool.TrueString) ? true : false;
            string groupName = reader.GetAttribute("GroupName");
            bool showBackground = (reader.GetAttribute("ShowBackground") == bool.TrueString) ? true : false;
            var color = GlideUtils.Convert.ToColor(reader.GetAttribute("Color"));
            var outlineColor = GlideUtils.Convert.ToColor(reader.GetAttribute("OutlineColor"));
            var selectedColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedColor"));
            var selectedOutlineColor = GlideUtils.Convert.ToColor(reader.GetAttribute("SelectedOutlineColor"));

            RadioButton radioButton = new RadioButton();// name, alpha, x, y, width, height);
            radioButton.Name = name;
            radioButton.Width = width;
            radioButton.Height = height;
            radioButton.Alpha = alpha;
            radioButton.Value = value;
            radioButton.Checked = _checked;
            radioButton.GroupName = groupName;
            //radioButton.showBackground = showBackground;
            radioButton.Foreground =  new SolidColorBrush( color);
            radioButton.OutlineUnselectColor = outlineColor;
            radioButton.SelectedColor = selectedColor;
            radioButton.SelectedOutlineColor = selectedOutlineColor;
            radioButton.GroupName = groupName;

            Canvas.SetLeft(radioButton, x);
            Canvas.SetTop(radioButton, y);

            //RadioButtonManager.AddButton(radioButton);

            return radioButton;
        }
        /*
        /// <summary>
        /// Parses the Slider XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>Slider object.</returns>
        private static Slider LoadSlider(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string direction = reader.GetAttribute("Direction");
            int snapInterval = Convert.ToInt32(reader.GetAttribute("SnapInterval"));
            int tickInterval = Convert.ToInt32(reader.GetAttribute("TickInterval"));
            Color tickColor = GlideUtils.Convert.ToColor(reader.GetAttribute("TickColor"));
            int knobSize = Convert.ToInt32(reader.GetAttribute("KnobSize"));
            double minimum = Convert.ToDouble(reader.GetAttribute("Minimum"));
            double maximum = Convert.ToDouble(reader.GetAttribute("Maximum"));
            double value = Convert.ToDouble(reader.GetAttribute("Value"));

            Slider slider = new Slider(name, alpha, x, y, width, height);
            slider.Visible = visible;
            slider.Enabled = enabled;
            slider.Direction = direction;
            slider.SnapInterval = snapInterval;
            slider.TickInterval = tickInterval;
            slider.TickColor = tickColor;
            slider.KnobSize = knobSize;
            slider.Minimum = minimum;
            slider.Maximum = maximum;
            slider.Value = value;

            return slider;
        }
        */
        /// <summary>
        /// Parses the TextBlock XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>TextBlock object.</returns>
        private static Text LoadTextBlock(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            var textAlign = GlideUtils.Convert.ToHorizontalAlign(reader.GetAttribute("TextAlign"));
            VerticalAlignment textVerticalAlign = GlideUtils.Convert.ToVerticalAlign(reader.GetAttribute("TextVerticalAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            var fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            var backColor = GlideUtils.Convert.ToColor(reader.GetAttribute("BackColor"));
            bool showBackColor = (reader.GetAttribute("ShowBackColor") == bool.TrueString) ? true : false;
            var textBlock = new Text()
            {
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            //;(name, alpha, x, y, width, height);
            textBlock.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            textBlock.IsEnabled = enabled;
            textBlock.TextContent = text;
            textBlock.TextAlignment = textAlign;
            textBlock.VerticalAlignment = textVerticalAlign;
            textBlock.Font = font;
            textBlock.ForeColor =  fontColor;
            //textBlock.Background = showBackColor ?  new SolidColorBrush( backColor) : new SolidColorBrush(Colors.Transparent);
            
            //textBlock.ShowBackColor = showBackColor;

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            return textBlock;
        }

        /// <summary>
        /// Parses the TextBox XML into a UI component.
        /// </summary>
        /// <param name="reader">XML reader object.</param>
        /// <returns>TextBox object.</returns>
        private static TextBox LoadTextBox(XmlReader reader)
        {
            string name = reader.GetAttribute("Name");
            ushort alpha = (reader.GetAttribute("Alpha") != null) ? Convert.ToUInt16(reader.GetAttribute("Alpha")) : _defaultDisplayObject.Alpha;
            int x = Convert.ToInt32(reader.GetAttribute("X"));
            int y = Convert.ToInt32(reader.GetAttribute("Y"));
            int width = Convert.ToInt32(reader.GetAttribute("Width"));
            int height = Convert.ToInt32(reader.GetAttribute("Height"));
            bool visible = (reader.GetAttribute("Visible") != null) ? (reader.GetAttribute("Visible") == bool.TrueString) : _defaultDisplayObject.Visible;
            bool enabled = (reader.GetAttribute("Enabled") != null) ? (reader.GetAttribute("Enabled") == bool.TrueString) : _defaultDisplayObject.Enabled;

            string text = reader.GetAttribute("Text");
            uint textAlign = GlideUtils.Convert.ToAlignment(reader.GetAttribute("TextAlign"));
            Font font = GlideUtils.Convert.ToFont(reader.GetAttribute("Font"));
            var fontColor = GlideUtils.Convert.ToColor(reader.GetAttribute("FontColor"));
            var textBox = new TextBox()
            {
                
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            //TextBox textBox = new TextBox(name, alpha, x, y, width, height);
            textBox.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            textBox.IsEnabled = enabled;
            textBox.Text = text;
            textBox.TextAlign = FromAlignment(textAlign);
            textBox.Font = font;
            textBox.Foreground = new SolidColorBrush( fontColor);

            Canvas.SetLeft(textBox, x);
            Canvas.SetTop(textBox, y);
            return textBox;
        }
    }
}
