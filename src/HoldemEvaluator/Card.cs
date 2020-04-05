using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial struct Card : IComparable<Card>
    {
        #region Constructors

        public Card(int rank, int suit)
        {
            Binary = Bin.GetBinary(rank, suit);
            _suit = suit;
            _rank = rank;
        }

        internal Card(ulong card)
        {
            Binary = card;
            _suit = -1;
            _rank = -1;
        }

        /// <summary>
        /// Parses a single card (e.g. 6h or As) into a binary format
        /// </summary>
        /// <param name="cardString">The notation of a single card ("Ks" or "Th")</param>
        /// <returns>The binary representation with the according bit set</returns>
        public static Card Parse(string cardString)
        {
            return Notation.Parse(cardString);
        }

        #endregion

        /// <summary>
        /// One bit is true, that is representing the card. The lower the index (least significant bit) the higher the card.
        /// </summary>
        internal ulong Binary { get; private set; }

        /// <summary>
        /// Suit of the card (the higher the better)
        /// </summary>
        public int Suit {
            get {
                if(_suit < 0)
                    _suit = Bin.GetSuit(Binary);
                return _suit;
            }
        }
        private int _suit;

        /// <summary>
        /// Rank of the card (the higher the better)
        /// </summary>
        public int Rank {
            get {
                if(_rank < 0)
                    _rank = Bin.GetRank(Binary);
                return _rank;
            }
        }
        private int _rank;

        #region Native overloads

        public static bool operator ==(Card card1, Card card2)
        {
            return card1.Binary == card2.Binary;
        }

        public static bool operator !=(Card card1, Card card2)
        {
            return card1.Binary != card2.Binary;
        }

        public override bool Equals(object obj)
        {
            return obj is Card && ((Card)obj).Binary == Binary;
        }

        public int CompareTo(Card card)
        {
            if(Binary == card.Binary) {
                return 0;
            } else if(Binary > card.Binary) { // The larger the binary the lesser the card value
                return -1;
            } else {
                return 1;
            }
        }

        public static bool operator <(Card card1, Card card2)
        {
            return card1.CompareTo(card2) < 0;
        }

        public static bool operator >(Card card1, Card card2)
        {
            return card1.CompareTo(card2) > 0;
        }

        public override int GetHashCode()
        {
            return (int)Binary + (int)(Binary >> 32);
        }

        /// <summary>
        /// Create hole cards with both cards included
        /// </summary>
        public static HoleCards operator &(Card card1, Card card2)
        {
            return new HoleCards(card1.Binary | card2.Binary);
        }

        /// <summary>
        /// Create a card collection with both cards included
        /// </summary>
        public static CardCollection operator |(Card card1, Card card2)
        {
            if(card1 != null && card2 != null)
                return new CardCollection(card1.Binary | card2.Binary);

            if(card1 == null && card2 != null)
                return new CardCollection(card2.Binary);

            if(card1 != null && card2 == null)
                return new CardCollection(card1.Binary);

            return new CardCollection();
        }

        public override string ToString()
        {
            return Notation.GetNotation(Binary);
        }

        #endregion

        #region Type conversion

        public static implicit operator ulong(Card card)
        {
            return card.Binary;
        }

        public static implicit operator Card(ulong binary)
        {
            return new Card(binary);
        }

        #endregion
    }
}
