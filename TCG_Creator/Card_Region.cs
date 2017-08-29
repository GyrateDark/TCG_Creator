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

namespace TCG_Creator
{
    public enum IMAGE_OPTIONS
    {
        Fill,
        Letterbox,
        Unified_Fill,
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
            std_background_image = reg.std_background_image;
            background_image_filltype = reg.background_image_filltype;
            inheritted = reg.inheritted;
        }

        // 0-1 location of region on  a card of width 1 and height 1
        public Rect ideal_location;
        public string description;
        public int id;
        
        public String_Container strings = null;
        public bool decrease_text_size_to_fit = true;

        public ImageSource std_background_image;
        public IMAGE_OPTIONS background_image_filltype = IMAGE_OPTIONS.None;

        public bool inheritted = false;

        public DrawingGroup Draw_Region(Rect draw_location)
        {
            DrawingGroup reg_img = new DrawingGroup();

            if (background_image_filltype != IMAGE_OPTIONS.None)
            {
                ImageBrush image_brush = new ImageBrush(std_background_image);
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

            result |= std_background_image == null;
            std_background_image = null;

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
            strings.strings.Clear();
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

            return;
        }
    }
}
