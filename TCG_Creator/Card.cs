using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TCG_Creator
{
    public class Card
    {
        public List<Card_Region> regions;
        public int id;
        public string name;

        public DrawingGroup render_card(Rect location)
        {
            DrawingGroup card_drawing = new DrawingGroup();

            foreach (Card_Region i in regions)
            {
                Rect draw_location = new Rect();

                draw_location.X = location.X + i.ideal_location.X * location.Width;
                draw_location.Y = location.Y + i.ideal_location.Y * location.Height;
                draw_location.Width = location.Width * i.ideal_location.Width;
                draw_location.Height = location.Height * i.ideal_location.Height;

                card_drawing.Children.Add(i.draw_region(draw_location));
            }

            return card_drawing;
        }
    }
}
