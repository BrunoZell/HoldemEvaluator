using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace HoldemEvaluator
{
    public static partial class Hand
    {
        /// <summary>
        /// Helper class for handling ranges that represent one of the 169 general poker preflop hands
        /// </summary>
        public static class Grid
        {
            /// <summary>
            /// The grid of all possible 169 general preflop hole cards
            /// </summary>
            private static readonly string[] HandGrid = GenerateHandGrid().ToArray();

            /// <summary>
            /// Get the position of a specific hand in a hole card grid
            /// </summary>
            /// <param name="hand">The hand to locate</param>
            /// <returns>A Tuple of two integers containing the column and the row the hand appears in a hole card grid. The tuple is basically the x and y position</returns>
            public static Tuple<int, int> GetPosition(string hand)
            {
                int handIndex = GetIndex(hand);
                return new Tuple<int, int>(handIndex % RankCount, handIndex / RankCount);
            }

            /// <summary>
            /// Get the index of the hand in the continuous hole card grid list
            /// </summary>
            /// <param name="hand">The hand. E.g. "AKs" or "QQ". case sensitive</param>
            /// <returns>Integer from 0 to 168</returns>
            private static int GetIndex(string hand)
            {
                if (hand == null || !HandGrid.Contains(NormalizeHandString(hand)))
                    throw new ArgumentOutOfRangeException(nameof(hand), hand, $"Not a valid hole card combination");
                return Array.IndexOf(HandGrid, NormalizeHandString(hand));
            }

            /// <summary>
            /// Get the hole card hands from column and row number (zero based).
            /// (0, 0) is AA
            /// </summary>
            /// <param name="column">Zero based number of column</param>
            /// <param name="row">Zero based number of column</param>
            /// <returns>The string representation of the corresponding hand</returns>
            public static string GetNotation(int column, int row)
            {
                if (column < 0 || column > RankCount - 1)
                    throw new ArgumentOutOfRangeException(nameof(column), column, $"{nameof(column)} can't be greater than {RankCount - 1} or less than zero");
                if (row < 0 || row > RankCount - 1)
                    throw new ArgumentOutOfRangeException(nameof(row), row, $"{nameof(row)} can't be greater than {RankCount - 1} or less than zero");

                return HandGrid[row * RankCount + column];
            }

            /// <summary>
            /// Get the string representation ("AKs", "QQ", "9To") from the hand grid index
            /// </summary>
            internal static string GetNotation(int handIndex)
            {
                return HandGrid[handIndex];
            }


            #region Helpers

            /// <summary>
            /// Normalize the string representation of two hole cards.
            /// This function applies upper case to ranks and lower vase to suit values (suited or offsuit).
            /// It also makes sure the highest rank is positioned in the beginning of the string.
            /// </summary>
            /// <param name="handString">The raw representation of two hole cards (e.g. "AKs", "aks", "KaS", "AKO", "jj")</param>
            /// <returns>The formatted string (e.g. "AKs", "AKo", "JJ")</returns>
            private static string NormalizeHandString(string handString)
            {
                if (!Regex.IsMatch(handString, Range.Notation.HandRegex, RegexOptions.IgnoreCase))
                    throw new ArgumentOutOfRangeException(nameof(handString), handString, $"Hole card string must match the regex \"{ Range.Notation.HandRegex }\"");

                // Only the two chars for the ranks
                string cardSubString = handString.Substring(0, 2).ToUpper(CultureInfo.InvariantCulture);
                if (Notation.ParseRank(handString[0]) < Notation.ParseRank(handString[1])) {
                    // swap ranks if the second one is ranked higher
                    cardSubString = new string(cardSubString.Reverse().ToArray());
                }

                // If necessary add the suit back
                if (handString.Length == 3)
                    return cardSubString + Char.ToLower(handString[2], CultureInfo.InvariantCulture);
                return cardSubString;
            }

            /// <summary>
            /// Tests if a hand string represents a pocket pair with no suit ("AA", "tt", "77", "Qq")
            /// </summary>
            /// <returns>True, of parameter "hand" represents a pocket pair, false otherwise</returns>
            public static bool isPocketPair(string handString)
            {
                if (handString == null)
                    throw new ArgumentNullException(nameof(handString));

                return Regex.IsMatch(handString, Range.Notation.HandRegex, RegexOptions.IgnoreCase) &&
                    handString.Length == 2 && Char.ToUpper(handString[0], CultureInfo.InvariantCulture) == Char.ToUpper(handString[1], CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Creates a table like list of all possible hole card combination ranges (like it's been showing in the RangeView)
            /// </summary>
            /// <returns>A sequential list of all possible hole cards (in format like "AKs" or "JJ"), starting with the top row continuously flowed by the rows behind.</returns>
            private static IEnumerable<string> GenerateHandGrid()
            {
                for (int col = RankCount - 1; col >= 0; col--) {
                    for (int row = RankCount - 1; row >= 0; row--) {
                        if (col == row) {
                            yield return String.Format(CultureInfo.InvariantCulture, "{0}{0}", Notation.Ranks[col]);
                            continue;
                        }
                        yield return (String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                            Notation.Ranks[Math.Max(col, row)],
                            Notation.Ranks[Math.Min(col, row)],
                            row < col ? 's' : 'o'));
                    }
                }
            }

            #endregion
        }
    }
}
