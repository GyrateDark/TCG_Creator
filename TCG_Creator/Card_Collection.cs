using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TCG_Creator
{
    public class Card_Collection : ObservableObject
    {
        List<Card> _cards = new List<Card>();
        Card failCard;

        public Card_Collection()
        {
            failCard = new Card
            {
                Id = -1
            };

            Card tmp_card = new Card();
            tmp_card.Regions.Add(new Card_Region());

            tmp_card.Name = "1";
            tmp_card.Regions[0].text = "Hi";
            tmp_card.Regions[0].text_typeface = new Typeface("Times New Roman");
            tmp_card.IsTemplateCard = true;
            //tmp_card.Regions[0].text_brush = Brushes.Black;

            tmp_card.Regions[0].ideal_location = new System.Windows.Rect(0, 0, 1, 1);

            Add_Card_To_Collection(tmp_card);
        }

        public List<Card> CardCollection
        {
            get { return _cards; }
        }
        
        public void Add_Card_To_Collection(Card card)
        {
            card.Id = Find_Next_Empty_Card_Id();

            _cards.Add(card);
            OnPropertyChanged("CardCollection");
        }

        public bool Modify_Card_In_Collection(Card card)
        {
            for (int i = 0; i < _cards.Count; ++i)
            {
                if (_cards[i].Id == card.Id)
                {
                    _cards[i] = card;
                    OnPropertyChanged("CardCollection");
                    return true;
                }
            }
            return false;
        }

        public Card Find_Card_In_Collection(int cardId)
        {
            for (int i = 0; i < _cards.Count; ++i)
            {
                if (_cards[i].Id == cardId)
                {
                    return _cards[i];
                }
            }

            return failCard;
        }

        private int Find_Next_Empty_Card_Id()
        {
            if (CardCollection.Count == 0)
            {
                return 0;
            }

            List<int> cardIds = new List<int>();

            foreach (Card i in CardCollection)
            {
                cardIds.Add(i.Id);
            }

            cardIds.Sort();

            int prev_id = -1;

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

        public List<Tree_View_Card> Get_Tree_View_Template_Cards()
        {
            var onlyTemplateCards = new List<Card>();

            foreach (Card i in _cards)
            {
                if (i.IsTemplateCard)
                {
                    onlyTemplateCards.Add(i);
                }
            }

            if (onlyTemplateCards.Count == 0)
            {
                return new List<Tree_View_Card>();
            }

            return Get_Tree_View_Template_Cards_For_Parent(-1);
        }

        private List<Tree_View_Card> Get_Tree_View_Template_Cards_For_Parent(int searchParentId)
        {
            List<Tree_View_Card> result = new List<Tree_View_Card>();


            foreach (Card i in _cards)
            {
                if (i.ParentCard == searchParentId)
                {
                    Tree_View_Card tmp = new Tree_View_Card(i.Id, i.Name, i.ParentCard);
                    tmp.Children = Get_Tree_View_Template_Cards_For_Parent(i.Id);

                    result.Add(tmp);
                }
            }

            return result;
        }
    }
}
