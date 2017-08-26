using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

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
        // 0-1 location of region on  a card of width 1 and height 1
        public Rect ideal_location;
        public string description;
        public int id;

        public string text;
        public Typeface text_typeface;
        public bool decrease_text_size_to_fit = true;

        public Brush text_brush;

        public ImageSource std_background_image;
        public IMAGE_OPTIONS background_image_filltype = IMAGE_OPTIONS.None;

        public DrawingGroup draw_region(Rect draw_location)
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
                GlyphRun text_glyph_run = new GlyphRun();

                GlyphTypeface auto_typeface;
                bool auto_convert_typeface = text_typeface.TryGetGlyphTypeface(out auto_typeface);

                if (auto_convert_typeface)
                {
                    text_glyph_run.GlyphTypeface = auto_typeface;
                }
                else
                {
                    text_glyph_run.Characters = text.ToCharArray();
                    text_glyph_run.DeviceFontName = text_typeface.FontFamily.Source;
                }

                GlyphRunDrawing text_drawing = new GlyphRunDrawing(text_brush, text_glyph_run);

                reg_img.Children.Add(text_drawing);
            }

            return reg_img;
        }
    }
}
