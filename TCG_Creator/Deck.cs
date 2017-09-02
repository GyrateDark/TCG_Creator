using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCG_Creator
{
    public class Deck
    {
        public Inherittable_Properties DesiredDeckProperties { get; set; } = new Inherittable_Properties();

        public int Id { get; set; } = -1;
        public List<DeckCard> Cards { get; set; } = new List<DeckCard>();

        public string Name { get; set; } = "";
        public string DisplayName
        {
            get
            {
                return "Deck " + Id.ToString() + " - " + Name;
            }
        }
    }

    public class DeckCard
    {
        public DeckCard()
        {

        }
        public DeckCard(int cardId, int numberOfCardsInDeck)
        {
            CardId = cardId;
            NumberOfCardInDeck = numberOfCardsInDeck;
        }

        public int NumberOfCardInDeck { get; set; } = -1;
        public int CardId { get; set; } = -1;
    }
}
