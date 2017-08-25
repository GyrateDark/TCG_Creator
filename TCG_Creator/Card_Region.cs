using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
        public Rect location;
        public string description;
        public int id;

        public string text;
        public Typeface text_typeface;
        public bool decrease_text_size_to_fit = true;

        public ImageSource std_background_image;
        public IMAGE_OPTIONS background_image_filltype = IMAGE_OPTIONS.None;

        public void draw_region(ref DrawingGroup base_img)
        {
            if (background_image_filltype != IMAGE_OPTIONS.None)
            {
                ImageBrush image_brush = new ImageBrush(std_background_image);
                if (background_image_filltype == IMAGE_OPTIONS.Fill)
                    image_brush.Stretch = Stretch.Fill;
                else if (background_image_filltype == IMAGE_OPTIONS.Letterbox)
                    image_brush.Stretch = Stretch.Uniform;
                else if (background_image_filltype == IMAGE_OPTIONS.Unified_Fill)
                    image_brush.Stretch = Stretch.UniformToFill;

                BitmapImage background = image_brush;
                
                ImageDrawing drawing = new ImageDrawing(, location);
            }

            

            drawing


            //background_img_brush.
        }
    }
}
