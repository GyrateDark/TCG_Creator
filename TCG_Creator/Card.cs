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
    public class Card : ObservableObject
    {
        private List<Card_Region> _regions;
        private int _id = -1;
        private string _name;
        private List<int> _deckIds;
        private int _parentCardId = -1;

        private bool _templateCard;

        public Card()
        {
            _regions = new List<Card_Region>();
            _deckIds = new List<int>();
            _name = "<BLANK>";
        }

        #region Properties

        public List<Card_Region> Regions
        {
            get { return _regions; }
            set
            {
                if (value != _regions)
                {
                    _regions = value;
                    OnPropertyChanged("Regions");
                }
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public List<int> DeckIds
        {
            get { return _deckIds; }
            set
            {
                if (value != _deckIds)
                {
                    _deckIds = value;
                    OnPropertyChanged("DeckIds");
                }
            }
        }

        public int ParentCard
        {
            get { return _parentCardId; }
            set
            {
                if (value != _parentCardId)
                {
                    _parentCardId = value;
                    OnPropertyChanged("ParentCard");
                }
            }
        }

        public bool IsTemplateCard
        {
            get { return _templateCard; }
            set
            {
                if (value != _templateCard)
                {
                    _templateCard = value;
                    OnPropertyChanged("IsTemplateCard");
                }
            }
        }


        #endregion // Properties

        public DrawingGroup Render_Card(Rect location, ref Card_Collection allCardsRef)
        {
            DrawingGroup card_drawing = new DrawingGroup();

            if (ParentCard != -1)
            {
                Card parent_card = allCardsRef.Find_Card_In_Collection(ParentCard);

                card_drawing.Children.Add(parent_card.Render_Card(location, ref allCardsRef));
            }

            

            foreach (Card_Region i in Regions)
            {
                Rect draw_location = new Rect
                {
                    X = location.X + i.ideal_location.X * location.Width,
                    Y = location.Y + i.ideal_location.Y * location.Height,
                    Width = location.Width * i.ideal_location.Width,
                    Height = location.Height * i.ideal_location.Height
                };

                card_drawing.Children.Add(i.Draw_Region(draw_location));
            }

            return card_drawing;
        }
    }
}
