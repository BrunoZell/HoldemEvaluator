using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HoldemEvaluator
{
    public static partial class Notation
    {
        #region Static data

        // Todo: Create all Regex expressions dynamically with these rank and suit values
        /// <summary>
        /// String representation of the ranks
        /// </summary>
        public static readonly char[] Ranks = { '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };

        /// <summary>
        /// String representation of the suit values
        /// </summary>
        public static readonly char[] Suits = { 'd', 'c', 'h', 's' };

        #endregion

        #region Card rank and suit representation

        /// <summary>
        /// Get the rank value of a rank char representation. The higher the better. Minimum zero.
        /// </summary>
        /// <param name="rank">The rank as a char representing it.</param>
        public static int ParseRank(char rank)
        {
            rank = Char.ToUpper(rank, CultureInfo.InvariantCulture);
            if (!Ranks.Contains(rank))
                throw new ArgumentOutOfRangeException(nameof(rank), rank, $"{String.Join(", ", Ranks)} are the only valid ranks");
            return Array.IndexOf(Ranks, rank);
        }

        /// <summary>
        /// Get the suit value of a suit char representation. The higher the better. Minimum zero.
        /// </summary>
        /// <param name="suit">The suit as a char representing it.</param>
        public static int ParseSuit(char suit)
        {
            suit = Char.ToLower(suit, CultureInfo.InvariantCulture);
            if (!Suits.Contains(suit))
                throw new ArgumentOutOfRangeException(nameof(suit), suit, $"{String.Join(", ", Suits)} are the only valid suits");
            return Array.IndexOf(Suits, suit);
        }

        #endregion

        #region Formatting utilities

        /// <summary>
        /// Formats a string that represents a collection of cards or a range.
        /// (Card collection: "As Kh 6c 3c 6s" or "Jh, Th, Ks, 9h, 4d, 4s, Qc"; Range: "ATs+ 94o+ 66+ AK KQs 72o-76o TT-AA").
        /// Allowed separators are comas, semicolons, spaces, tabs and new lines.
        /// </summary>
        /// <param name="rawString">Raw string representation of the hand</param>
        /// <returns>Formatted string representation of the hand. If the input is null an empty string is returned</returns>
        public static string NormalizeRepresentation(string rawString)
        {
            if (rawString == null)
                return String.Empty;

            rawString = Regex.Replace(rawString, @"[\s,;]+", " ").Trim();
            return HandleUpperLowerCase(rawString);
        }

        /// <summary>
        /// Transforms all chars that could represent ranks into upper case and all chars that could represent suit values into lower case.
        /// Anything unknown is untouched,
        /// </summary>
        /// <param name="stringRepresentation">Unformatted string representation</param>
        /// <returns>String representation with correct cases applied</returns>
        private static string HandleUpperLowerCase(string stringRepresentation)
        {
            char[] stringCharArray = stringRepresentation.ToCharArray();
            for (int i = 0; i < stringCharArray.Length; i++) {

                char lowerCase = Char.ToLower(stringCharArray[i], CultureInfo.InvariantCulture);
                if (Suits.Contains(lowerCase))
                    stringCharArray[i] = lowerCase;

                char upperCase = Char.ToUpper(stringCharArray[i], CultureInfo.InvariantCulture);
                if (Ranks.Contains(upperCase))
                    stringCharArray[i] = upperCase;
            }
            return new string(stringCharArray);
        }

        #endregion
    }
}
