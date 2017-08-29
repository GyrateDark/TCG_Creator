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
    public enum IMAGE_OPTIONS
    {
        Fill,
        Letterbox,
        Unified_Fill,
        None
    }

    public enum IMAGE_LOCATION_TYPE
    {
        Absolute,
        Online,
        Relative,
        None
    }

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
            background_location = reg.background_location;
            background_location_type = reg.background_location_type;
            background_image_filltype = reg.background_image_filltype;
            inheritted = reg.inheritted;
        }

        // 0-1 location of region on  a card of width 1 and height 1
        public Rect ideal_location;
        public string description;
        public int id;
        
        public String_Container strings = null;
        public bool decrease_text_size_to_fit = true;

        public string background_location;
        public IMAGE_LOCATION_TYPE background_location_type = IMAGE_LOCATION_TYPE.None;
        public IMAGE_OPTIONS background_image_filltype = IMAGE_OPTIONS.None;

        public bool inheritted = false;

        public DrawingGroup Draw_Region(Rect draw_location)
        {
            DrawingGroup reg_img = new DrawingGroup();

            if (background_image_filltype != IMAGE_OPTIONS.None && background_location_type != IMAGE_LOCATION_TYPE.None)
            {
                BitmapImage background;

                if (background_location_type == IMAGE_LOCATION_TYPE.Absolute || background_location_type == IMAGE_LOCATION_TYPE.Relative)
                {
                    background = new BitmapImage(new Uri(background_location));
                }
                else
                {
                    background = URLToBitmap(background_location);
                }

                ImageBrush image_brush = new ImageBrush(background);
                if (background_image_filltype == IMAGE_OPTIONS.Fill)
                    image_brush.Stretch = Stretch.Fill;
                else if (background_image_filltype == IMAGE_OPTIONS.Letterbox)
                    image_brush.Stretch = Stretch.Uniform;
                else if (background_image_filltype == IMAGE_OPTIONS.Unified_Fill)
                    image_brush.Stretch = Stretch.UniformToFill;

                GeometryDrawing image_rec_drawing = new GeometryDrawing(image_brush, new Pen(Brushes.Transparent, 0), new RectangleGeometry(draw_location));

                reg_img.Children.Add(image_rec_drawing);
            }

            if (strings != null)
            {
                FormattedText formatText = strings.ConvertToFormattedText();

                formatText.MaxTextWidth = draw_location.Width;
                formatText.MaxTextHeight = draw_location.Height;
                
                DrawingContext text_drawing = reg_img.Open();

                text_drawing.DrawText(formatText, draw_location.Location);
                text_drawing.Close();
            }

            return reg_img;
        }

        public bool Should_Inherit_Region()
        {
            bool result = false;

            if (strings != null)
            {
                if (strings.strings.Count >= 0)
                {
                    result |= strings.strings[0].Text.Length >= 1;
                    strings = null;
                }
            }

            result |= background_location_type == IMAGE_LOCATION_TYPE.None;
            background_location = "";
            background_location_type = IMAGE_LOCATION_TYPE.None;
            background_image_filltype = IMAGE_OPTIONS.None;

            return result;
        }

        public FlowDocument ConvertFromStringContainerToFlowDocument()
        {
            Paragraph oneLine = new Paragraph();
            FlowDocument result = new FlowDocument();
            
            foreach (String_Drawing i in strings.strings)
            {
                oneLine = new Paragraph(new Run(i.Text));

                oneLine.FontFamily = new FontFamily(i.FontFamily);
                oneLine.FontSize = i.FontSize;
                oneLine.FontStyle = i.StrFontStyle;
                oneLine.FontWeight = i.StrFontWeight;

                oneLine.Foreground = i.TextBrush;

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

                    reconstructedString.FontFamily = i.FontFamily.ToString();
                    reconstructedString.FontSize = i.FontSize;
                    reconstructedString.StrFontStyle = i.FontStyle;
                    reconstructedString.StrFontWeight = i.FontWeight;

                    reconstructedString.TextBrush = i.Foreground;

                    reconstructedString.Text = new TextRange(i.ContentStart, i.ContentEnd).Text;

                    strings.strings.Add(reconstructedString);
                }

                strings.TxtAlign = doc.TextAlignment;

            }
        }

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
    }
}
