using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class CardCollection : IEnumerable<Card>
    {
        #region Constructors

        /// <summary>
        /// Creates an empty collection
        /// </summary>
        public CardCollection()
        {
        }

        /// <summary>
        /// Creates a collection from a list of cards
        /// </summary>
        public CardCollection(IEnumerable<Card> cards)
        {
            Cards = cards;
        }

        /// <summary>
        /// Clones a card collection
        /// </summary>
        public CardCollection(CardCollection collection)
        {
            if(collection != null)
                Binary = collection.Binary;
        }

        /// <summary>
        /// Creates a collection from an existing one
        /// </summary>
        internal CardCollection(ulong collection)
        {
            Binary = collection;
        }

        /// <summary>
        /// Parses a collection of single cards.
        /// </summary>
        /// <param name="collectionString">
        /// The string representation of the collection. E.g. "As Kh 6c 3c 6s" or "Jh, Th, Ks, 9h, 4d, 4s, Qc"
        /// </param>
        /// <returns>A CardCollection containing all cards the string specified</returns>
        public static CardCollection Parse(string collectionString)
        {
            return Notation.Parse(collectionString);
        }

        /// <summary>
        /// Creates a collection containing random cards.
        /// </summary>
        /// <param name="cardAmount">The amount of cards the collection should hold</param>
        /// <param name="excludedCards">The cards the collection can't hold.</param>
        /// <returns>A new collection containing random cards which weren't excluded</returns>
        public static CardCollection Random(int cardAmount, CardCollection excludedCards = null)
        {
#if DEBUG
            if(excludedCards.Count + cardAmount > Hand.TotalCards)
                throw new NashException("Not enough cards left after excluding cards");
#endif
            return new CardCollection(RandomAsUlong(cardAmount, excludedCards));
        }

        internal static ulong RandomAsUlong(int cardAmount, ulong excludedCards = 0UL, ulong includedCards = 0UL)
        {
            if((excludedCards & Hand.Masks.ValidBitMask) == 0UL) {
                return Bin.RandomNBitNumber(cardAmount);
            }

            ulong mask = ~(excludedCards ^ includedCards);
            return Bin.ExpandRight(Bin.RandomNBitNumber(cardAmount, Hand.Bin.GetCardCount(mask)), mask) | includedCards;
        }

        #endregion

        /// <summary>
        /// The actual set of cards
        /// </summary>
        internal ulong Binary { get; set; } = 0UL;

        /// <summary>
        /// How many cards are in the collection
        /// </summary>
        public int Count {
            get {
                return Hand.Bin.GetCardCount(Binary);
            }
        }

        /// <summary>
        /// Access the list of cards directly
        /// </summary>
        public IEnumerable<Card> Cards {
            get {
                return Bin.GetAllCards(Binary);
            }
            set {
                Binary = 0UL;
                foreach(var card in value) {
                    Binary |= card.Binary;
                }
            }
        }

        /// <summary>
        /// Adds a card to the collection if it not already contains it.
        /// </summary>
        /// <param name="card">Card to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(Card card)
        {
            Binary |= card.Binary;
        }

        /// <summary>
        /// Adds all cards from one collection to this collection if it not already contains it.
        /// </summary>
        /// <param name="collection">A collection with all cards to add</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(CardCollection collection)
        {
            Binary |= collection.Binary;
        }

        /// <summary>
        /// Adds all cards from one binary representation to this collection if it not already contains it.
        /// </summary>
        /// <param name="cards">A collection with all cards to add in a binary format</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Include(ulong cards)
        {
            Binary |= cards;
        }

        /// <summary>
        /// Removes a card from the collection. No error is thrown
        /// if the card doesn't exist in this collection before.
        /// </summary>
        /// <param name="card">Card to remove</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude(Card card)
        {
            Binary &= ~card.Binary;
        }

        /// <summary>
        /// Removes all cards of another collection from this collection.
        /// No error is thrown if the cards doesn't exist in this collection before.
        /// </summary>
        /// <param name="collection">A collection with all cards to remove</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude(CardCollection collection)
        {
            Binary &= ~collection.Binary;
        }

        /// <summary>
        /// Removes all cards of another binary representation from this collection.
        /// No error is thrown if the cards doesn't exist in this collection before.
        /// </summary>
        /// <param name="cards">A collection with all cards to remove in binary format</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Exclude(ulong cards)
        {
            Binary &= ~cards;
        }

        /// <summary>
        /// Tests of a single card is contained in the collection
        /// </summary>
        /// <param name="card">Card to test for</param>
        /// <returns>Returns true if the card is contained in this collection</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Card card)
        {
            return (Binary & card.Binary) == card.Binary;
        }

        /// <summary>
        /// Tests if all cards of another collection are contained in this collection
        /// </summary>
        /// <param name="collection">Another collection to compare with</param>
        /// <returns>True, if all cards of the other collection are contained in this one</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(CardCollection collection)
        {
            return (Binary & collection.Binary) == collection.Binary;
        }

        /// <summary>
        /// Tests if all cards of another collection are contained in this collection
        /// </summary>
        /// <param name="cards">Another collection to compare with in a binary format</param>
        /// <returns>True, if all cards of the other collection are contained in this one</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Contains(ulong cards)
        {
            return (Binary & cards) == cards;
        }

        /// <summary>
        /// Tests if one ore more cards of another collection are contained in this collection
        /// </summary>
        /// <param name="collection">Another collection to compare with</param>
        /// <returns>True, if one ore more cards of the other collection are contained in this one</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsSome(CardCollection collection)
        {
            return (Binary & collection.Binary) != 0UL;
        }

        /// <summary>
        /// Tests if one ore more cards of another collection are contained in this collection
        /// </summary>
        /// <param name="cards">Another collection to compare with in a binary format</param>
        /// <returns>True, if one ore more cards of the other collection are contained in this one</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool ContainsSome(ulong cards)
        {
            return (Binary & cards) != 0UL;
        }

        #region Native overloads

        public static bool operator ==(CardCollection collection1, CardCollection collection2)
        {
            if(ReferenceEquals(collection1, null)) {
                return ReferenceEquals(collection2, null);
            }
            return collection1.Equals(collection2);
        }

        public static bool operator !=(CardCollection collection1, CardCollection collection2)
        {
            if(ReferenceEquals(collection1, null)) {
                return !ReferenceEquals(collection2, null);
            }
            return !collection1.Equals(collection2);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is CardCollection && ((CardCollection)obj).Binary == Binary;
        }

        /// <summary>
        /// Creates a new collection containing all cards that both collections have contained
        /// </summary>
        public static CardCollection operator &(CardCollection collection1, CardCollection collection2)
        {
            return new CardCollection(collection1.Binary & collection2.Binary);
        }

        /// <summary>
        /// Creates a new collection containing all cards of both collections
        /// </summary>
        public static CardCollection operator |(CardCollection collection1, CardCollection collection2)
        {
            return new CardCollection(collection1.Binary | collection2.Binary);
        }

        /// <summary>
        /// Shortcut for including a card in a collection
        /// </summary>
        public static CardCollection operator |(CardCollection collection, Card card)
        {
            if(collection == null)
                return null;

            if(card == null)
                return collection;

            collection.Include(card);
            return collection;
        }

        /// <summary>
        /// Shortcut for including a card in a collection
        /// </summary>
        public static CardCollection operator |(Card card, CardCollection collection)
        {
            if(collection == null)
                return null;

            if(card == null)
                return collection;

            collection.Include(card);
            return collection;
        }

        /// <summary>
        /// Shortcut for including a card in a collection
        /// </summary>
        public static CardCollection operator |(CardCollection collection, HoleCards holeCards)
        {
            if(collection == null)
                return null;

            if(holeCards == null)
                return collection;

            collection.Include((CardCollection)holeCards);
            return collection;
        }


        /// <summary>
        /// Create a card collection with all cards included. No error is thrown on duplicated cards.
        /// </summary>
        public static CardCollection operator |(HoleCards holeCards, CardCollection collection)
        {
            if(collection == null)
                return null;

            if(holeCards == null)
                return collection;

            collection.Include((CardCollection)holeCards);
            return collection;
        }

        public override string ToString()
        {
            return Notation.GetNotation(Binary);
        }

        public override int GetHashCode()
        {
            return (int)Binary + (int)(Binary >> 32);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Card> GetEnumerator()
        {
            return Cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cards.GetEnumerator();
        }

        #endregion

        #region Type conversion

        public static implicit operator ulong(CardCollection collection)
        {
            return collection == null ? 0UL : collection.Binary;
        }

        public static implicit operator CardCollection(ulong binary)
        {
            return new CardCollection(binary);
        }

        #endregion
    }
}
