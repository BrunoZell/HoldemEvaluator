using System;
using System.Collections.Generic;
using System.Linq;

namespace HoldemEvaluator
{
    /// <summary>
    /// Object for representing a players two exact hole cards.
    /// For a more general representation use a Range.
    /// </summary>
    public partial struct HoleCards
    {
        private static Random _rnd = new Random();

        #region Constructors

        /// <summary>
        /// Creates HoleCards from two single cards
        /// </summary>
        public HoleCards(Card card1, Card card2)
        {
            if (card1 == null)
                throw new ArgumentNullException(nameof(card1));
            if (card2 == null)
                throw new ArgumentNullException(nameof(card2));

            Binary = card1.Binary | card2.Binary;
        }

        /// <summary>
        /// Creates hole cards from two cards in a binary format
        /// </summary>
        /// <param name="cards">Exactly two of the first 52 bits have to be true representing the cards.</param>
        internal HoleCards(ulong cards)
        {
            if (Hand.Bin.GetCardCount(cards) != 2)
                throw new ArgumentException("Only two cards allowed as hole cards", nameof(cards));

            Binary = cards;
        }

        // <summary>
        /// Parses a two hole cards (e.g. "As6h")
        /// </summary>
        /// <param name="holeCardString">The notation of a single card ("Ks" or "Th")</param>
        /// <returns>The correct HoleCard object</returns>
        public static HoleCards Parse(string holeCardString)
        {
            return Notation.Parse(holeCardString);
        }

        /// <summary>
        /// Creates two new random hole cards
        /// </summary>
        /// <param name="excludedCards">The cards the hole cards can't contain</param>
        /// <returns>New random hole cards</returns>
        public static HoleCards Random(CardCollection excludedCards = null)
        {
            if (excludedCards != null && excludedCards.Count + 2 > Hand.TotalCards)
                throw new ArgumentException("Not enough cards left after excluding cards", nameof(excludedCards));

            return new HoleCards(CardCollection.RandomAsUlong(2, excludedCards));
        }

        /// <summary>
        /// Creates a random hole card combo but with different chances for different strength of hands.
        /// </summary>
        /// <param name="distribution">The distribution function with input range [0; 1] and output range [0; 1].
        /// A random parameter is generated (linear) and this function determines the rank of the hole card
        /// combo to return</param>
        /// <param name="playerAmount">The amount of players at the table</param>
        /// <returns>A random hole card combo but with different chances for different strength of hands.</returns>
        public static HoleCards Random(Func<float, float> distribution, int playerAmount)
        {
            return Ranking.GetHoleCardsFromRankInternal(distribution((float)_rnd.NextDouble()), playerAmount);
        }

        /// <summary>
        /// Creates a list of random hole cards with no card duplicates
        /// </summary>
        /// <param name="holeCardAmount">How many hole card pairs are required</param>
        /// <param name="excludedCards">Which cards should be excluded since the beginning</param>
        /// <returns>A list of random hole cards</returns>
        public static IEnumerable<HoleCards> RandomList(int holeCardAmount, CardCollection excludedCards = null)
        {
            if (excludedCards != null && excludedCards.Count + holeCardAmount * 2 > Hand.TotalCards)
                throw new ArgumentException("Not enough cards left after excluding cards", nameof(excludedCards));

            if (excludedCards == null)
                excludedCards = new CardCollection();

            for (int i = 0; i < holeCardAmount; i++) {
                var newHoleCards = Random(excludedCards);
                yield return newHoleCards;
                excludedCards.Include(newHoleCards.Binary);
            }
        }

        #endregion

        /// <summary>
        /// Two bits are true, they are representing the cards. The lower the index (least significant bit) the higher the card.
        /// </summary>
        internal ulong Binary { get; private set; }

        /// <summary>
        /// If both cards have the same suit
        /// </summary>
        public bool IsSuited {
            get {
                int[] suits = CardCollection.Bin.GetAllSuits(Binary);
                return suits[0] == suits[1];
            }
        }

        /// <summary>
        /// If both cards have the same rank
        /// </summary>
        public bool IsPocketPair {
            get {
                int[] ranks = CardCollection.Bin.GetAllRanks(Binary);
                return ranks[0] == ranks[1];
            }
        }

        /// <summary>
        /// Rank of the higher card (the higher the better)
        /// </summary>
        public Card HighCard => CardCollection.Bin.GetAllCards(Binary).Max();

        /// <summary>
        /// Rank of the lower card (the higher the better)
        /// </summary>
        public Card LowCard => CardCollection.Bin.GetAllCards(Binary).Min();

        #region Native overloads

        public static bool operator ==(HoleCards obj1, HoleCards obj2) => obj1.Binary == obj2.Binary;
        public static bool operator !=(HoleCards obj1, HoleCards obj2) => obj1.Binary != obj2.Binary;
        public override bool Equals(object obj) => obj is HoleCards && ((HoleCards)obj).Binary == Binary;
        public override int GetHashCode() => Binary.GetHashCode();
        public override string ToString() => Notation.GetNotation(Binary);

        /// <summary>
        /// Create a card collection with all cards included. No error is thrown on duplicated cards.
        /// </summary>
        public static CardCollection operator |(HoleCards holeCards1, HoleCards holeCards2)
        {
            if (holeCards1 != null && holeCards2 != null)
                return new CardCollection(holeCards1.Binary | holeCards2.Binary);

            if (holeCards1 == null && holeCards2 != null)
                return new CardCollection(holeCards2.Binary);

            if (holeCards1 != null && holeCards2 == null)
                return new CardCollection(holeCards1.Binary);

            return new CardCollection();
        }

        #endregion

        #region Type conversion

        public static implicit operator ulong(HoleCards holeCards) => holeCards.Binary;

        public static implicit operator HoleCards(ulong binary) => new HoleCards(binary);

        public static explicit operator CardCollection(HoleCards holeCards) => new CardCollection(holeCards.Binary);

        #endregion
    }
}
