using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class Board
    {
        public static class Notation
        {
            #region Regex

            /// <summary>
            /// Regex for identifying a single card. Case sensitive.
            /// Valid examples:
            /// "Ah" or "7s" or "Tc" or "9d"
            /// </summary>
            public static readonly string BoardRegex = @"^[2-9TJQKA]{1}[dchs]{1}$";

            #endregion

            #region Parsing

            /// <summary>
            /// Parses a single card (e.g. 6h or As) into a binary format
            /// </summary>
            /// <param name="boardString">The notation of a single card ("Ks" or "Th")</param>
            /// <returns>The binary representation with the according bit set</returns>
            public static Board Parse(string boardString)
            {
                // Normalize string
                boardString = HoldemEvaluator.Notation.NormalizeRepresentation(boardString);

                if(!isValidNotation(boardString))
                    throw new NotABoardException();

                return new Board(CardCollection.Parse(boardString));
            }

            #endregion

            #region Get Notation

            /// <summary>
            /// Get the notation of a single card
            /// </summary>
            /// <returns>String representation of the card, e.g. "Ac" or "6s"</returns>
            public static string GetNotation(Board board)
            {
                return GetNotation(board._cards.Binary);
            }

            /// <summary>
            /// Get the notation of a single card
            /// </summary>
            /// <returns>String representation of the card, e.g. "Ac" or "6s"</returns>
            internal static string GetNotation(ulong cards)
            {
                return CardCollection.Notation.GetNotation(cards);
            }

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
                return CardCollection.Notation.isValidNotation(cardString);
            }

            #endregion
        }
    }
}
