using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial struct HoleCards
    {
        public static class Notation
        {
            #region Regex

            /// <summary>
            /// Regex for identifying two hole cards. Case sensitive.
            /// Valid examples:
            /// "Ah7s" or "Tc9d" or "QdQs"
            /// </summary>
            public static readonly string HoleCardRegex = @"^(?:[2-9TJQKA]{1}[dchs]{1}){2}$";

            #endregion

            #region Parsing

            /// <summary>
            /// Parses a two hole cards (e.g. "As6h")
            /// </summary>
            /// <param name="holeCardString">The notation of a single card ("Ks" or "Th")</param>
            /// <returns>The correct HoleCard object</returns>
            public static HoleCards Parse(string holeCardString)
            {
                if(!isValidNotation(holeCardString))
                    throw new NoHoleCardsException();

                return new HoleCards(CardCollection.Notation.Parse(holeCardString).Binary);
            }

            #endregion

            #region Get Notation

            /// <summary>
            /// Get the string representation ("AsKs", "QQ", "9cTh")
            /// </summary>
            public static string GetNotation(HoleCards holeCards)
            {
                return GetNotation(holeCards.Binary);
            }

            /// <summary>
            /// Get the string representation ("AsKs", "QQ", "9cTh") from the binary representation
            /// </summary>
            internal static string GetNotation(ulong holeCards)
            {
                if(Hand.Bin.GetCardCount(holeCards) != 2)
                    throw new ArgumentException("Hole cards have to be exactly two cards", nameof(holeCards));

                return String.Join("", CardCollection.Bin.GetAllCards(holeCards));
            }


            #endregion

            #region Validation

            /// <summary>
            /// Test is a string representation of two hole cards is valid.
            /// It tests if there are only valid ranks and suits are used.
            /// </summary>
            /// <param name="holeCardString">The string to validate</param>
            /// <returns>True if the string is valid, false otherwise.</returns>
            public static bool isValidNotation(string holeCardString)
            {
                if(holeCardString == null)
                    return false;
                if(holeCardString == String.Empty)
                    return true;

                // Normalize the string representation
                return Regex.IsMatch(HoldemEvaluator.Notation.NormalizeRepresentation(holeCardString), HoleCardRegex);
            }

            #endregion
        }
    }
}
