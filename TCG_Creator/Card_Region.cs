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
        
        public FormattedText text;
        public bool decrease_text_size_to_fit = true;

        public Brush text_brush;

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

            if (text != null)
            {
                text.MaxTextWidth = draw_location.Width;
                text.MaxTextHeight = draw_location.Height;

                Pen text_pen = new Pen(Brushes.White, 0);

                GeometryDrawing text_drawing = new GeometryDrawing(text_brush, text_pen, text.BuildGeometry(draw_location.Location));
                
                reg_img.Children.Add(text_drawing);
            }

            return reg_img;
        }

        public bool Should_Inherit_Region()
        {
            bool result = false;

            if (text != null)
            {
                result |= text.Text != "";
                text = null;
            }

            result |= std_background_image == null;
            std_background_image = null;

            return result;
        }

        public FlowDocument ConvertFromFormattedTextToFlowDocument()
        {
            FlowDocument result = new FlowDocument();

            Paragraph graph = new Paragraph();

            string tmp = text.ToString();

            return result;
        }

        public void ConvertFromFlowDocumentToFormattedText(FlowDocument doc)
        {
            return;
        }
    }
}
