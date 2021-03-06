﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TCG_Creator
{
    public class Card_Collection : ObservableObject
    {
        List<Card> _cards = new List<Card>();
        Card failCard;
        Card newCard;

        public Card_Collection()
        {
            failCard = new Card
            {
                Id = -1
            };
            newCard = new Card
            {
                Id = -2,
                Name = "<New>"
            };

            var conv = new ImageSourceConverter();
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

        public List<Tree_View_Card> Get_Tree_View_Template_Cards(ref Card_Collection coll)
        {
            var onlyTemplateCards = new List<Card>();

            foreach (Card i in _cards)
            {
                if (i.IsTemplateCard)
                {
                    onlyTemplateCards.Add(i);
                }
            }

            List<Tree_View_Card> result = new List<Tree_View_Card>();

            if (onlyTemplateCards.Count >= 1)
            {
                result = Get_Tree_View_Template_Cards_For_Parent(-1, ref coll);
            }
            result.Add(new Tree_View_Card(-2, "<New>", -1, ref coll));

            return result;
        }

        private List<Tree_View_Card> Get_Tree_View_Template_Cards_For_Parent(int searchParentId, ref Card_Collection coll)
        {
            List<Tree_View_Card> result = new List<Tree_View_Card>();


            foreach (Card i in _cards)
            {
                if (i.ParentCard == searchParentId)
                {
                    Tree_View_Card tmp = new Tree_View_Card(i.Id, i.Name, i.ParentCard, ref coll);
                    tmp.Children = Get_Tree_View_Template_Cards_For_Parent(i.Id, ref coll);

                    result.Add(tmp);
                }
            }

            return result;
        }
    }
}
