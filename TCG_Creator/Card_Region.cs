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
        }

        // 0-1 location of region on  a card of width 1 and height 1
        public Rect ideal_location;
        public string description;
        public int id;

        public string text;
        public Serializable_Typeface text_typeface;
        public bool decrease_text_size_to_fit = true;

        public Serializable_Brush text_brush;

        public ImageSource std_background_image;
        public IMAGE_OPTIONS background_image_filltype = IMAGE_OPTIONS.None;

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
                FormattedText formatted_text = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, text_typeface, 32, text_brush);

                formatted_text.MaxTextWidth = draw_location.Width;
                formatted_text.MaxTextHeight = draw_location.Height;

                Pen text_pen = new Pen(Brushes.Transparent, 0);

                GeometryDrawing text_drawing = new GeometryDrawing(text_brush, text_pen, formatted_text.BuildGeometry(draw_location.Location));

                reg_img.Children.Add(text_drawing);
            }

            return reg_img;
        }
    }
}
