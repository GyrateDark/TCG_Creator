using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace TCG_Creator
{
    public class Card_Collection
    {
        private const int StartCardId = 1000;
        private const int StartDeckId = 1000;

        public int NextRegionId = 10000;

        List<Deck> _decks = new List<Deck>();
        List<Card> _cards = new List<Card>();
        List<Card> _baseTemplates = new List<Card>();
        Card failCard;
        Card newCard;

        private const string RESOURCE_BLANKCARD_BASE = "pack://application:,,,/TCG_Creator;component";

        private GUI_Settings _GUISettings = new GUI_Settings();

        public Card_Collection()
        {
            failCard = new Card
            {
                Id = -1,
                Regions = new List<Card_Region>
                {
                    new Card_Region()
                }
            };
            newCard = new Card
            {
                Id = -2,
                Name = "<New>"
            };

            var conv = new ImageSourceConverter();
            CreateBaseTemplateCards();
            /*
            Card tmp_card = new Card();

            tmp_card.Name = "Blank Hero Card";

            tmp_card.Regions.Add(new Card_Region(ref nextRegionId));

            tmp_card.Regions[0].background_location_type = IMAGE_LOCATION_TYPE.Online;
            tmp_card.Regions[0].background_location = "https://cdn.discordapp.com/attachments/125738853616058369/351546993421844492/Hero_card.jpg";
            tmp_card.Regions[0].background_image_filltype = IMAGE_OPTIONS.Letterbox;
            tmp_card.Regions[0].ideal_location = new System.Windows.Rect(0, 0, 1, 1);

            tmp_card.Regions.Add(new Card_Region(ref nextRegionId));

            tmp_card.Regions[1].strings = new String_Container();
            tmp_card.Regions[1].strings.strings.Add(new String_Drawing("Blank"));
            tmp_card.Regions[1].ideal_location = new System.Windows.Rect(0.0957575757575758, 0.0702222222222222, 0.8096969696969697, 0.0595555555555556);

            tmp_card.Regions.Add(new Card_Region(ref nextRegionId));
            tmp_card.Regions[2].ideal_location = new System.Windows.Rect(80/825.0, 156/1125.0, 666/825.0, 495/1125.0);

            tmp_card.Regions.Add(new Card_Region(ref nextRegionId));
            tmp_card.Regions[3].ideal_location = new System.Windows.Rect(80/825.0, 663/1125.0, 666/825.0, 267/1125.0);

            tmp_card.Regions.Add(new Card_Region(ref nextRegionId));
            tmp_card.Regions[4].ideal_location = new System.Windows.Rect(115/825.0, 640/1125.0, 447/825.0, 50/1125.0);

            tmp_card.IsTemplateCard = true;
            */

            //Add_Card_To_Collection(tmp_card);
        }

        [XmlIgnore]
        public List<Card> CardCollection
        {
            get
            {
                List<Card> result = new List<Card>();

                result.AddRange(_baseTemplates);
                result.AddRange(_cards);

                return result;
            }
        }
        public List<Card> CustomCardsOnly
        {
            get
            {
                return _cards;
            }
        }
        public List<Deck> Decks
        {
            get
            {
                return _decks;
            }
            set
            {
                if (value != _decks)
                {
                    _decks = value;
                }
            }
        }
        
        public void Add_Card_To_Collection(Card card)
        {
            card.Id = Find_Next_Empty_Card_Id();

            _cards.Add(card);
        }
        public void Add_Deck(Deck deck)
        {
            deck.Id = Find_Next_Empty_Card_Id();

            _decks.Add(deck);
        }

        public bool Modify_Card_In_Collection(Card card)
        {
            for (int i = 0; i < _cards.Count; ++i)
            {
                if (_cards[i].Id == card.Id)
                {
                    _cards[i] = card;
                    return true;
                }
            }
            return false;
        }

        public Card Find_Card_In_Collection(int cardId)
        {
            for (int i = 0; i < CardCollection.Count; ++i)
            {
                if (CardCollection[i].Id == cardId)
                {
                    return CardCollection[i];
                }
            }
            //throw new IndexOutOfRangeException("Given card id does not exist, " + cardId.ToString());
            return failCard;
        }

        private int Find_Next_Empty_Card_Id()
        {
            if (CardCollection.Count == 0)
            {
                return StartCardId;
            }

            List<int> cardIds = new List<int>();

            foreach (Card i in CardCollection)
            {
                cardIds.Add(i.Id);
            }

            cardIds.Sort();

            int prev_id = StartCardId-1;

            foreach (int i in cardIds)
            {
                if (i - prev_id >= 2)
                {
                    return prev_id + 1;
                }
                prev_id = i;
            }

            return prev_id + 1;
        }
        private int Find_Next_Empty_Deck_Id()
        {
            if (Decks.Count == 0)
            {
                return StartDeckId;
            }

            List<int> deckIds = new List<int>();

            foreach (Deck i in Decks)
            {
                deckIds.Add(i.Id);
            }

            deckIds.Sort();

            int prev_id = StartDeckId-1;

            foreach (int i in deckIds)
            {
                if (i - prev_id >= 2)
                {
                    return prev_id + 1;
                }
                prev_id = i;
            }

            return prev_id + 1;
        }

        public List<Tree_View_Card> Get_Tree_View_Cards(ref Card_Collection coll, bool OnlyTemplateCards, Tree_View_Card.OnPropertyChangedDelegate propertyChanged)
        {
            var searchCards = new List<Card>();

            if(OnlyTemplateCards)
            {
                foreach (Card i in CardCollection)
                {
                    if (i.IsTemplateCard)
                    {
                        searchCards.Add(i);
                    }
                }
            }
            else
            {
                searchCards.AddRange(CardCollection);
            }
            

            List<Tree_View_Card> result = new List<Tree_View_Card>();

            if (searchCards.Count >= 1)
            {
                result = Get_Tree_View_Cards_For_Parent(null, ref coll, propertyChanged);
            }
            result.Add(new Tree_View_Card(-2, "<New>", -1, ref coll, propertyChanged));

            return result;
        }

        private List<Tree_View_Card> Get_Tree_View_Cards_For_Parent(Tree_View_Card Parent, ref Card_Collection coll, Tree_View_Card.OnPropertyChangedDelegate propertyChanged)
        {
            List<Tree_View_Card> result = new List<Tree_View_Card>();


            foreach (Card i in CardCollection)
            {
                if ((Parent == null && i.ParentCard == -1) || (Parent != null && i.ParentCard == Parent.Id))
                {
                    Tree_View_Card tmp = new Tree_View_Card(i.Id, i.Name, i.ParentCard, ref coll, propertyChanged);
                    tmp.Children = Get_Tree_View_Cards_For_Parent(tmp, ref coll, propertyChanged);
                    tmp.Parent = Parent;

                    if (tmp.Id == SelectedCardId)
                    {
                        tmp.SetIsSelected();
                        Tree_View_Card parent = tmp.Parent;
                        while(parent != null)
                        {
                            parent.IsExpanded = true;
                            parent = parent.Parent;
                        }
                    }

                    result.Add(tmp);
                }
            }

            return result;
        }

        #region GUI Properties
        public int SelectedRegionId
        {
            get
            {
                return _GUISettings.SelectedRegionId;
            }
            set
            {
                _GUISettings.SelectedRegionId = value;
            }
        }
        public int SelectedCardId
        {
            get
            {
                return _GUISettings.SelectedCardId;
            }
            set
            {
                _GUISettings.SelectedCardId = value;
            }
        }
        public int SelectedDeckId
        {
            get {return _GUISettings.SelectedDeckId; }
            set { _GUISettings.SelectedDeckId = value;}
        }
        public bool ShowAllRegions
        {
            get
            {
                return _GUISettings.ShowAllRegions;
            }
            set
            {
                _GUISettings.ShowAllRegions = value;
            }
        }
        public bool ShowDeckSettings
        {
            get { return _GUISettings.ShowDeckSettings; }
            set { _GUISettings.ShowDeckSettings = value; }
        }
        #endregion

        public double PPI { get; set; } = 300;

        private void CreateBaseTemplateCards()
        {
            int currentCardRegionId = 0;
            int currentCardId = 0;

            _baseTemplates.AddRange(CreateTemplateSotMCards(ref currentCardRegionId, ref currentCardId));

            foreach (Card i in _baseTemplates)
            {
                foreach (Card_Region j in i.Regions)
                {
                    if (j.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation != "")
                    {
                        j.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation = RESOURCE_BLANKCARD_BASE + j.DesiredInherittedProperties.ImageProperties.BackgroundImageLocation;
                    }
                }
            }
        }

        private List<Card> CreateTemplateSotMCards(ref int currentCardRegionId, ref int currentCardId)
        {
            List<Card> result = new List<Card>();

            result.Add(CreateBlankHeroCard(ref currentCardRegionId, ref currentCardId));

            return result;
        }
        private Card CreateBlankHeroCard(ref int currentCardRegionId, ref int currentCardId)
        {
            Card templateCard = new Card();

            templateCard.IsBaseTemplate = true;
            templateCard.Name = "Blank Hero Card";

            templateCard.Regions.Add(new Card_Region(ref currentCardRegionId));

            templateCard.Regions[0].DesiredInherittedProperties.ImageProperties.BackgroundImageLocation = "/Resources/BlankCards/SotM_Blank_Hero_Card.jpg";
            templateCard.Regions[0].DesiredInherittedProperties.ImageProperties.BackgroundImageFillType = IMAGE_OPTIONS.Fill;
            templateCard.Regions[0].ideal_location = new System.Windows.Rect(0, 0, 1, 1);

            templateCard.Regions.Add(new Card_Region(ref currentCardRegionId));

            templateCard.Regions[1].DesiredInherittedProperties.StringContainer = new String_Container();
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.strings.Add(new String_Drawing("Title"));
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.FontFamily = "#CrashLanding BB";
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.FontSize = 70;
            templateCard.Regions[1].description = "Title";
            templateCard.Regions[1].ideal_location = new System.Windows.Rect(0.11, 0.0702222222222222, 0.79, 0.0595555555555556);

            templateCard.Regions.Add(new Card_Region(ref currentCardRegionId));
            templateCard.Regions[2].description = "Image";
            templateCard.Regions[2].ideal_location = new System.Windows.Rect(80 / 825.0, 156 / 1125.0, 666 / 825.0, 495 / 1125.0);

            templateCard.Regions.Add(new Card_Region(ref currentCardRegionId));
            templateCard.Regions[3].description = "Text";
            templateCard.Regions[3].ideal_location = new System.Windows.Rect(80 / 825.0, 663 / 1125.0, 666 / 825.0, 267 / 1125.0);

            templateCard.Regions.Add(new Card_Region(ref currentCardRegionId));
            templateCard.Regions[4].description = "Class";
            templateCard.Regions[4].ideal_location = new System.Windows.Rect(115 / 825.0, 640 / 1125.0, 447 / 825.0, 50 / 1125.0);

            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.StrokeOn = true;
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.StrokeThickness = 1;
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.TextVerticalAlignment = VerticalAlignment.Center;

            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.TextBrushColorMode = TextBrushMode.Gradient;
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.GradientTextBrushAngle = 90;
            templateCard.Regions[1].DesiredInherittedProperties.StringContainer.stringProperties.GradientTextBrushStops = new GradientStopCollection
            {
                new GradientStop(Colors.Orange, 0.15),
                new GradientStop(Colors.Red, 0.85)
            };

            foreach (Card_Region i in templateCard.Regions)
            {
                i.IsLocked = true;
            }

            templateCard.SetAllInherittableProperties(false);

            templateCard.IsTemplateCard = true;
            templateCard.Id = currentCardId;
            ++currentCardId;

            return templateCard;
        }
    }
}
