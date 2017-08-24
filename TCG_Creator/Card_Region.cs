using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TCG_Creator
{
    public enum IMAGE_OPTIONS
    {
        Crop,
        Fill,
        Stretch
    }

    public class Card_Region
    {
        public Rectangle location;
        public string description;
        public int id;

        public string text;
        public Typeface text_typeface;
        public bool decrease_text_size_to_fit = true;

        public ImageSource background_image;
        public IMAGE_OPTIONS background_image_filltype;

        public void draw_region(ref BitmapImage img, Rectangle location)
        {
            ImageBrush background_img_brush = new ImageBrush(background_image);
            
            //background_img_brush.
        }
    }
}
