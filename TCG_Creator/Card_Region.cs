using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Xml.Serialization;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using System.Net;
using System.IO;

namespace TCG_Creator
{
    

    public class Card_Region : ObservableObject
    {
        public Card_Region()
        {
            id = -1;
        }
        public Card_Region(ref int currentCardRegionId)
        {
            id = currentCardRegionId;
            ++currentCardRegionId;
        }

        public Card_Region(Card_Region reg)
        {
            ideal_location = reg.ideal_location;
            id = reg.id;
            _desiredProperties = reg._desiredProperties.Clone();
        }

        // 0-1 location of region on  a card of width 1 and height 1
        public Rect ideal_location;
        public string description = "";
        public int id;
        
        public bool decrease_text_size_to_fit = true;
        public bool InheritRegionBeforeCard { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        private Inherittable_Properties _desiredProperties = new Inherittable_Properties();
        private Inherittable_Properties _renderProperties = new Inherittable_Properties();

        // Call SetRenderInherittableProperties() First
        public DrawingGroup Draw_Region(Rect draw_location, double PPI)
        {
            DrawingGroup reg_img = new DrawingGroup();

            if (_renderProperties.ImageProperties.BackgroundImageFillType != IMAGE_OPTIONS.None && _renderProperties.ImageProperties.BackgroundImageLocationType != IMAGE_LOCATION_TYPE.None)
            {
                try
                {
                    BitmapImage background;

                    if (_renderProperties.ImageProperties.BackgroundImageLocationType == IMAGE_LOCATION_TYPE.Absolute)
                    {
                        background = new BitmapImage(new Uri(_renderProperties.ImageProperties.BackgroundImageLocation, UriKind.Absolute));
                    }
                    else if (_renderProperties.ImageProperties.BackgroundImageLocationType == IMAGE_LOCATION_TYPE.Relative)
                    {
                        background = new BitmapImage(new Uri(_renderProperties.ImageProperties.BackgroundImageLocation, UriKind.Relative));
                    }
                    else
                    {
                        background = URLToBitmap(_renderProperties.ImageProperties.BackgroundImageLocation);
                    }

                    ImageBrush image_brush = new ImageBrush(background);
                    if (_renderProperties.ImageProperties.BackgroundImageFillType == IMAGE_OPTIONS.Fill)
                        image_brush.Stretch = Stretch.Fill;
                    else if (_renderProperties.ImageProperties.BackgroundImageFillType == IMAGE_OPTIONS.Letterbox)
                        image_brush.Stretch = Stretch.Uniform;
                    else if (_renderProperties.ImageProperties.BackgroundImageFillType == IMAGE_OPTIONS.Unified_Fill)
                        image_brush.Stretch = Stretch.UniformToFill;

                    GeometryDrawing image_rec_drawing = new GeometryDrawing(image_brush, new Pen(Brushes.Transparent, 0), new RectangleGeometry(draw_location));

                    reg_img.Children.Add(image_rec_drawing);
                }
                catch
                {
                    MessageBox.Show("Invalid background image for region "+id.ToString());
                }
            }

            if (_renderProperties.StringContainer.strings.Count >= 1)
            {
                FormattedText formatText = _renderProperties.StringContainer.ConvertToFormattedText(PPI);

                formatText.MaxTextWidth = draw_location.Width;
                //formatText.MaxTextHeight = draw_location.Height;

                Size formatTextBounds = new Size(formatText.Width, formatText.Height);
                Rect textDrawLocation = draw_location;

                if (_renderProperties.StringProperties.FontFamily == "#CrashLanding BB")
                {
                    formatTextBounds.Height *= 1.25;
                }

                if (_renderProperties.StringProperties.TextHorizontalAlignment == HorizontalAlignment.Center)
                {
                    textDrawLocation.X += (textDrawLocation.Width - formatTextBounds.Width) / 2;
                }
                else if (_renderProperties.StringProperties.TextHorizontalAlignment == HorizontalAlignment.Right)
                {
                    textDrawLocation.X += (textDrawLocation.Width - formatTextBounds.Width);
                }

                if (_renderProperties.StringProperties.TextVerticalAlignment == VerticalAlignment.Bottom)
                {
                    textDrawLocation.Y += (textDrawLocation.Height - formatTextBounds.Height);
                }
                else if (_renderProperties.StringProperties.TextVerticalAlignment == VerticalAlignment.Center)
                {
                    textDrawLocation.Y += (textDrawLocation.Height - formatTextBounds.Height) / 2;
                }
                
                DrawingContext text_drawing = reg_img.Open();

                formatText.MaxTextWidth = draw_location.Width;
                //formatText.MaxTextHeight = draw_location.Height;

                text_drawing.DrawText(formatText, textDrawLocation.Location);

                text_drawing.Close();


                if (_renderProperties.StringProperties.StrokeOn)
                {
                    GeometryDrawing textStrokeDrawing = new GeometryDrawing(Brushes.Transparent, new Pen(new SolidColorBrush(_renderProperties.StringProperties.TextStrokeColor), _renderProperties.StringProperties.StrokeThickness*PPI/300), formatText.BuildGeometry(textDrawLocation.Location));

                    reg_img.Children.Add(textStrokeDrawing);
                }
            }

            return reg_img;
        }

        public Inherittable_Properties RenderInherittableProperties
        {
            get
            {
                return _renderProperties.Clone();
            }
        }
        public Inherittable_Properties DesiredInherittedProperties
        {
            get { return _desiredProperties; }
            set
            {
                if (_desiredProperties != value)
                {
                    _desiredProperties = value;
                }
            }
        }

        public void SetRenderInherittableProperties(List<Inherittable_Properties> RegionProperties, Inherittable_Properties DeckProperties)
        {
            _renderProperties = _desiredProperties.Clone();

            foreach (Inherittable_Properties i in RegionProperties)
            {
                _renderProperties = _renderProperties.GetInherittedPropertiesMerging(i, DeckProperties);
            }
            
        }
        /*
        public FlowDocument ConvertFromStringContainerToFlowDocument(String_Properties cardRenderProperties)
        {
            Paragraph oneLine = new Paragraph();
            FlowDocument result = new FlowDocument();
            String_Properties properties = new String_Properties();

            foreach (String_Drawing i in strings.strings)
            {
                oneLine = new Paragraph(new Run(i.Text));

                properties = strings.CombinedStringProperties(i, cardRenderProperties);

                oneLine.FontFamily = new FontFamily(properties.FontFamily);
                oneLine.FontSize = properties.FontSize;
                oneLine.FontStyle = properties.SFontStyle;
                oneLine.FontWeight = properties.SFontWeight;

                oneLine.Foreground = i.properties.TextBrush;

                result.Blocks.Add(oneLine);
            }

            return result;
        }

        public void ConvertFromFlowDocumentToStringContainer(FlowDocument doc)
        {
            if (strings == null)
            {
                strings = new String_Container();
            }
            strings.strings.Clear();
            if (doc != null)
            {
                foreach (Block i in doc.Blocks)
                {
                    String_Drawing reconstructedString = new String_Drawing();
                    String_Properties properties = new String_Properties();

                    properties.FontFamily = i.FontFamily.ToString();
                    properties.FontSize = i.FontSize;
                    properties.SFontStyle = i.FontStyle;
                    properties.SFontWeight = i.FontWeight;

                    reconstructedString.properties.TextBrush = i.Foreground;

                    reconstructedString.Text = new TextRange(i.ContentStart, i.ContentEnd).Text;

                    strings.strings.Add(reconstructedString);
                }

                strings.TxtAlign = doc.TextAlignment;

            }
        }
        */

        private BitmapImage URLToBitmap(string URL)
        {
            var image = new BitmapImage();
            int BytesToRead = 100;

            WebRequest request = WebRequest.Create(new Uri(URL, UriKind.Absolute));
            request.Timeout = -1;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            BinaryReader reader = new BinaryReader(responseStream);
            MemoryStream memoryStream = new MemoryStream();

            byte[] bytebuffer = new byte[BytesToRead];
            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);

            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }
        
        public IList<Color> GetUsedColors()
        {
            return _renderProperties.GetUsedColors();
        }
        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                string result = "";

                if (IsLocked)
                {
                    result += "🔒 ";
                }

                if (id == 0)
                {
                    return result + "Base Card";
                }
                else if (id == -1)
                {
                    return result + "No Regions";
                }
                else
                {
                    if (description != "")
                    {
                        return result + description;
                    }
                    else
                    {
                        return result + "<No Description>";
                    }
                }
            }
        }
        [XmlIgnore]
        private bool _isMouseOver = false;
        [XmlIgnore]
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }
            set
            {
                if (value != _isMouseOver)
                {
                    _isMouseOver = value;
                    OnPropertyChanged("IsMouseOver");
                }
            }
        }
    }
}
