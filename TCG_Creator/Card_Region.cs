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
    

    public class Card_Region
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

        private Inherittable_Properties _desiredProperties = new Inherittable_Properties();
        private Inherittable_Properties _renderProperties = new Inherittable_Properties();

        // Call SetRenderInherittableProperties() First
        public DrawingGroup Draw_Region(Rect draw_location)
        {
            DrawingGroup reg_img = new DrawingGroup();

            if (_renderProperties.ImageProperties.BackgroundImageFillType != IMAGE_OPTIONS.None && _renderProperties.ImageProperties.BackgroundImageLocationType != IMAGE_LOCATION_TYPE.None)
            {
                BitmapImage background;

                if (_renderProperties.ImageProperties.BackgroundImageLocationType == IMAGE_LOCATION_TYPE.Absolute || _renderProperties.ImageProperties.BackgroundImageLocationType == IMAGE_LOCATION_TYPE.Relative)
                {
                    background = new BitmapImage(new Uri(_renderProperties.ImageProperties.BackgroundImageLocation));
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

            if (_renderProperties.StringContainer.strings.Count >= 1)
            {
                FormattedText formatText = _renderProperties.StringContainer.ConvertToFormattedText();

                formatText.MaxTextWidth = draw_location.Width;
                formatText.MaxTextHeight = draw_location.Height;
                
                DrawingContext text_drawing = reg_img.Open();

                text_drawing.DrawText(formatText, draw_location.Location);
                text_drawing.Close();
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

        public void SetRenderInherittableProperties(List<Inherittable_Properties> Properties)
        {
            _renderProperties = _desiredProperties.Clone();

            if (Properties != null)
            {
                foreach (Inherittable_Properties i in Properties)
                {
                    _renderProperties = _renderProperties.GetInherittedPropertiesMerging(i);
                }
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
    }
}
