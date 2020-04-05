using System;
using System.Text.RegularExpressions;

namespace HoldemEvaluator
{
    public partial struct Card
    {
        public static class Notation
        {
            #region Regex

            /// <summary>
            /// Regex for identifying a single card. Case sensitive.
            /// Valid examples:
            /// "Ah" or "7s" or "Tc" or "9d"
            /// </summary>
            public static readonly string CardRegex = @"^[2-9TJQKA]{1}[dchs]{1}$";

            #endregion

            #region Parsing

            /// <summary>
            /// Parses a single card (e.g. 6h or As) into a binary format
            /// </summary>
            /// <param name="cardString">The notation of a single card ("Ks" or "Th")</param>
            /// <returns>The binary representation with the according bit set</returns>
            public static Card Parse(string cardString)
            {
                // Normalize string
                cardString = HoldemEvaluator.Notation.NormalizeRepresentation(cardString);

                if (!isValidNotation(cardString))
                    throw new NotACardException();

                return new Card(HoldemEvaluator.Notation.ParseRank(cardString[0]), HoldemEvaluator.Notation.ParseSuit(cardString[1]));
            }

            #endregion

            #region Get Notation

            /// <summary>
            /// Get the notation of a single card
            /// </summary>
            /// <returns>String representation of the card, e.g. "Ac" or "6s"</returns>
            public static string GetNotation(Card card) =>
                GetNotation(card.Rank, card.Suit);

            /// <summary>
            /// Get the notation of a single card
            /// </summary>
            /// <returns>String representation of the card, e.g. "Ac" or "6s"</returns>
            public static string GetNotation(int rank, int suit) =>
                String.Concat(HoldemEvaluator.Notation.Ranks[rank], HoldemEvaluator.Notation.Suits[suit]);

            /// <summary>
            /// Get the notation of a single card
            /// </summary>
            /// <returns>String representation of the card, e.g. "Ac" or "6s"</returns>
            internal static string GetNotation(ulong card) =>
                GetNotation(Bin.GetRank(card), Bin.GetSuit(card));

            #endregion

            #region Validation

            /// <summary>
            /// Test is a string representation of a card is valid.
            /// It tests if there are only valid ranks and suits are used.
            /// </summary>
            /// <param name="cardString">The string to validate</param>
            /// <returns>True if the string is valid, false otherwise.</returns>
            public static bool isValidNotation(string cardString)
            {
                if (String.IsNullOrWhiteSpace(cardString))
                    return false;

                // Normalize the string representation
                cardString = HoldemEvaluator.Notation.NormalizeRepresentation(cardString);
                return Regex.IsMatch(cardString, CardRegex);
            }

            #endregion
        }
    }
}
