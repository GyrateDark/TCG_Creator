using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace TCG_Creator
{
    public class Card
    {
        private List<Card_Region> _regions;
        private int _id = -1;
        private string _name;
        private List<int> _deckIds;
        private int _parentCardId = -1;

        private bool _templateCard;

        private Size _physicalSize = new Size(2.5, 3.5);

        [XmlIgnore]
        public bool IsBaseTemplate { get; set; } = false;

        public double PhysicalSizeHeight
        {
            get { return _physicalSize.Height; }
            set
            {
                if (_physicalSize.Height != value)
                {
                    _physicalSize.Height = value;
                }
            }
        }
        public double PhysicalSizeWidth
        {
            get { return _physicalSize.Width; }
            set
            {
                if (_physicalSize.Width != value)
                {
                    _physicalSize.Width = value;
                }
            }
        }

        public IList<Color> GetUsedColors
        {
            get
            {
                IList<Color> result = new List<Color>();

                foreach (Card_Region i in _regions)
                {
                    IList<Color> regColors = i.GetUsedColors();
                    foreach(Color j in regColors)
                    {
                        if (!result.Contains(j))
                        {
                            result.Add(j);
                        }
                    }
                }

                return result;
            }
        }

        public Card()
        {
            _regions = new List<Card_Region>();
            _deckIds = new List<int>();
            _name = "<BLANK>";
        }

        public Card(Card card)
        {
            _physicalSize = card._physicalSize;
            _regions = new List<Card_Region>(card.Regions.Count);

            for(int i = 0; i < card.Regions.Count; ++i)
            {
                Card_Region tmp = new Card_Region();

                tmp.id = card.Regions[i].id;
                tmp.description = card.Regions[i].description;

                tmp.ideal_location = card.Regions[i].ideal_location;

                _regions.Add(tmp);
            }

            if (card.Regions.Count == 0)
            {
                Card_Region tmp = new Card_Region();

                tmp.id = 0;
                tmp.description = "";

                tmp.ideal_location = new Rect(0, 0, 1, 1);

                _regions.Add(tmp);
            }

            if (card.Id == -2)
            {
                _regions[0].DesiredInherittedProperties.SetAllInheritValues(false);
            }

            _deckIds = new List<int>(card.DeckIds);
            _name = card.Name;
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
                }
            }
        }
        public string DisplayName
        {
            get
            {
                string result = "";

                if (IsBaseTemplate)
                {
                    result += "B";
                }
                if (IsTemplateCard)
                {
                    result += "T ";
                }

                result += Name;

                return result;
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
                }
            }
        }

        public Inherittable_Properties GetRenderInherittableProperties()
        {
            return _regions[0].RenderInherittableProperties;
        }
        #endregion // Properties

        public void CalcInherittableProperties(ref Card_Collection allCardsRef, Inherittable_Properties deckProperties)
        {
            if (_regions.Count >= 1)
            {
                // Outer List is one per region
                // Inner List is per region
                List<Inherittable_Properties>[] properties = new List<Inherittable_Properties>[_regions.Count];

                for (int i = 0; i < _regions.Count; ++i)
                {
                    properties[i] = new List<Inherittable_Properties>();
                }

                if (ParentCard != -1)
                {
                    Card parentCard = allCardsRef.Find_Card_In_Collection(ParentCard);

                    parentCard.CalcInherittableProperties(ref allCardsRef, deckProperties);

                    for (int i = 0; i < _regions.Count; ++i)
                    {
                        foreach (Card_Region j in parentCard._regions)
                        {
                            if (_regions[i].id == j.id)
                            {
                                properties[i].Add(j.RenderInherittableProperties);
                            }
                        }
                    }
                }
                
                for (int i = 0; i < _regions.Count; ++i)
                {

                    _regions[i].SetRenderInherittableProperties(properties[i], deckProperties);
                }
            }
        }

        public void CalcInherittableProperties_Card(ref Card_Collection allCardsRef, Inherittable_Properties DeckProperties)
        {
            CalcInherittableProperties(ref allCardsRef, DeckProperties);
        }
        public DrawingGroup Render_Card(Rect location, ref Card_Collection allCardsRef, double PPI)
        {
            DrawingGroup card_drawing = new DrawingGroup();

            if (location.Height == 0 || location.Width == 0)
            {
                return card_drawing;
            }

            Inherittable_Properties renderProperties = new Inherittable_Properties();

            Card parentCard = new Card();

            if (ParentCard != -1)
            {
                parentCard = allCardsRef.Find_Card_In_Collection(ParentCard);
            }

            foreach (Card_Region i in Regions)
            {
                if (ParentCard != -1)
                {
                    foreach (Card_Region j in parentCard.Regions)
                    {
                        if (i.id == j.id)
                        {

                        }
                    }
                }
                Rect draw_location = new Rect
                {
                    X = location.X + i.ideal_location.X * location.Width,
                    Y = location.Y + i.ideal_location.Y * location.Height,
                    Width = location.Width * i.ideal_location.Width,
                    Height = location.Height * i.ideal_location.Height
                };

                card_drawing.Children.Add(i.Draw_Region(draw_location, PPI));
            }

            return card_drawing;
        }

        public void SetAllInherittableProperties(bool val)
        {
            foreach (Card_Region i in Regions)
            {
                i.DesiredInherittedProperties.SetAllInheritValues(val);
            }
        }

        public void CopyAll<T>(T source, T target)
        {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties())
            {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
            foreach (var sourceField in type.GetFields())
            {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }
    }
}
