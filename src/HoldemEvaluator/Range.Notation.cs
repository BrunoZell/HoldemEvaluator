using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class Range
    {
        public static class Notation
        {

            #region Regex

            /// <summary>
            /// One hand with or without suit specified. Case sensitive.
            /// "AK" or "AKs" or "KAo".
            /// </summary>
            public static readonly string HandRegex = @"([2-9TJQKA]){2}([so])?";

            /// <summary>
            /// Sub range with or without suit including pocket pairs. Case sensitive.
            /// "66-TT" or "A5o-ATo" or "TAs-AKs"
            /// </summary>
            private static readonly string SubRangeRegex = @"([2-9TJQKA]{2}[so]?)-([2-9TJQKA]{2}[so]?)";

            /// <summary>
            /// Sub range with or without suit specified. Case sensitive.
            /// "66+" or "A5s+"
            /// </summary>
            private static readonly string OpenSubRangeRegex = @"([2-9TJQKA]{2}[so]?)\+";

            /// <summary>
            /// Matches a complete notation of a range.
            /// "ATs+ 94o+ 66+ AK KQs 72o-76o TT-AA"
            /// </summary>
            private static readonly string RangeNotationRegex = string.Format(@"^(?:(?:{0})|(?:{1})|(?:{2})|\s+)*$", HandRegex, SubRangeRegex, OpenSubRangeRegex);

            #endregion

            #region Parsing

            /// <summary>
            /// Parse a range of hole cards. Ranges represent a set of hole cards seen on the hole card grid.
            /// Valid notations are:
            /// "AA KK AKs AKo"
            /// "KK+ AK",
            /// "A9s+, Q5s-QTs",
            /// "A9s+;QT-Q5",
            /// "33-55 99-JJ"
            /// </summary>
            /// <param name="rangeString">The raw string to parse</param>
            /// <returns>A Range object where the data property is set accordingly to the range input string</returns>
            public static Range Parse(string rangeString)
            {
                try {
                    // Normalization
                    rangeString = HoldemEvaluator.Notation.NormalizeRepresentation(rangeString);

                    // Validation
                    if(!isValidNotation(rangeString))
                        throw new NotARangeException();

                    Range result = new Range();

                    // AKs JTo QT
                    foreach(Match hand in Regex.Matches(rangeString, HandRegex)) {
                        if(hand.Success) {
                            if(hand.Value.Length == 2 && !Hand.Grid.isPocketPair(hand.Value)) {
                                // no suit specified
                                // add hand for both suit
                                SetResultData(result, Hand.Grid.GetPosition(hand.Value + 's'));
                                SetResultData(result, Hand.Grid.GetPosition(hand.Value + 'o'));
                            } else {
                                // suit specified
                                SetResultData(result, Hand.Grid.GetPosition(hand.Value));
                            }
                        }
                    }

                    // 22-66 72o-76o 98s-94s A5-AT
                    IncludeSubRanges(rangeString, result, SubRangeRegex, 1, 2);

                    // 66+ 72o+ A9+
                    IncludeSubRanges(rangeString, result, OpenSubRangeRegex, 1);

                    return result;
                } catch(Exception ex) {
                    throw new NotARangeException(null, ex);
                }
            }

            #region Data add

            /// <summary>
            /// Add al hands of a specific range type to the result
            /// </summary>
            /// <param name="rangeString">Complete normalized representation of a range</param>
            /// <param name="result">Result object containing the range data to add the hands to</param>
            /// <param name="subRangeRegex">Regex for finding all sub range occurrences in the complete range string</param>
            /// <param name="groupNumStart">Group number of which the regex captures the lower bound of the sub range (captures without '+' or '-')</param>
            /// <param name="groupNumEnd">Group number of which the regex captures the lower bound of the sub range. Null if the range regex finds open ranges (Like "66+" or "A9o+")</param>
            private static void IncludeSubRanges(string rangeString, Range result, string subRangeRegex, int groupNumStart, int? groupNumEnd = null)
            {
                int groupCount = 1 + (groupNumEnd.HasValue ? Math.Max(groupNumStart, groupNumEnd.Value) : groupNumStart);
                foreach(Match subRange in Regex.Matches(rangeString, subRangeRegex)) {
                    if(subRange.Success && subRange.Groups.Count >= groupCount) {
                        foreach(var hand in GetSubRangeHands(subRange.Groups[groupNumStart].Value,
                                                          groupNumEnd.HasValue ? subRange.Groups[groupNumEnd.Value].Value : null)) {
                            SetResultData(result, Hand.Grid.GetPosition(hand));
                        }
                    }
                }
            }

            /// <summary>
            /// Sets the card of the range on dataPos true.
            /// </summary>
            /// <param name="result">The result object where the data is modified in</param>
            /// <param name="dataPos">The coordinates of the data to set to true</param>
            private static void SetResultData(Range result, Tuple<int, int> dataPos)
            {
                result[dataPos.Item1, dataPos.Item2] = true;
            }

            #endregion

            #region Get sub ranges

            /// <summary>
            /// Get every hand that's represented by a range of hands.
            /// The range is defined by a starting hand and an optional
            /// ending hand, all hand in between them including the
            /// bonding hands are the output if this function
            /// </summary>
            /// <param name="endHand">Null when it represents an open range (e.g. "66+", "74o+")</param>
            /// <returns>An array of all hands included in the range</returns>
            private static string[] GetSubRangeHands(string startHand, string endHand)
            {
                // Validate arguments
                if(startHand != null && endHand != null) {
                    // none argument null
                    if(Hand.Grid.isPocketPair(startHand) ^ Hand.Grid.isPocketPair(endHand))
                        throw new ParsingException("Arguments not compatible. Either both are pocket pairs or both aren't");
                } else {
                    if(startHand != null ^ endHand != null) {
                        // One argument null
                        if(startHand == null) {
                            // If one argument is null, it's always the second one
                            startHand = endHand;
                            endHand = null;
                        }
                    } else {
                        // Both arguments are null
                        throw new ArgumentNullException(nameof(startHand), "At least one argument must be not null");
                    }
                }

                // Check for open sub ranges first
                if(endHand == null) {
                    // 66+ or ATs+ (startHand is then "66" or "ATs" or "A9")
                    return GetOpenSubRange(startHand);
                }

                if(Hand.Grid.isPocketPair(startHand) && Hand.Grid.isPocketPair(endHand)) {
                    // Pair range
                    // 66-TT
                    return GetBoundPairedSubRange(startHand, endHand);
                } else {
                    // Another range
                    // Q5o-Q9o
                    return GetBoundSubRange(startHand, endHand);
                }
            }

            /// <summary>
            /// Get all hands in a general sub ranges  ("Q5o-Q9o", "A5s-AJs")
            /// </summary>
            /// <param name="startHand">Lower bound of the range</param>
            /// <param name="endHand">Upper bound of the range (can also be smaller than the lower bound)</param>
            /// <returns></returns>
            private static string[] GetBoundSubRange(string startHand, string endHand)
            {
                if(startHand == null)
                    throw new ArgumentNullException(nameof(startHand));
                if(endHand == null)
                    throw new ArgumentNullException(nameof(endHand));

                if(startHand.Length == 2 && !Hand.Grid.isPocketPair(startHand) &&
                    endHand.Length == 2 && !Hand.Grid.isPocketPair(endHand)) {
                    // No pocket pairs. No suit specified.
                    // Apply the range for both suited and offsuit
                    return GetBoundSubRange(startHand + 's', endHand + 's')
                        .Concat(GetBoundSubRange(startHand + 'o', endHand + 'o'))
                        .ToArray();
                }

                var pos1 = Hand.Grid.GetPosition(startHand);
                var pos2 = Hand.Grid.GetPosition(endHand);

                if(pos1.Item1 == pos2.Item1) {

                    int start = Math.Min(pos1.Item2, pos2.Item2);
                    int end = Math.Max(pos1.Item2, pos2.Item2);

                    return Enumerable.Range(start, end - start + 1)
                        .Select(i => Hand.Grid.GetNotation(pos1.Item1, i))
                        .ToArray();

                } else if(pos1.Item2 == pos2.Item2) {

                    int start = Math.Min(pos1.Item1, pos2.Item1);
                    int end = Math.Max(pos1.Item1, pos2.Item1);

                    return Enumerable.Range(start, end - start + 1)
                        .Select(i => Hand.Grid.GetNotation(i, pos1.Item2))
                        .ToArray();

                } else {
                    throw new ParsingException("Start and end hand of the range are not compatible");
                }
            }

            /// <summary>
            /// Returns all hands included in an pocket pair sub range (e.g. "66-TT", "A5o-AJo")
            /// </summary>
            /// <param name="startHand">Lower bound of the range ("66" for a sub range of "66-TT")</param>
            /// <param name="endHand">Upper bound of the range  ("TT" for a sub range of "66-TT", can also be smaller than the lower bound)</param>
            private static string[] GetBoundPairedSubRange(string startHand, string endHand)
            {
                var pos1 = Hand.Grid.GetPosition(startHand).Item1;
                var pos2 = Hand.Grid.GetPosition(endHand).Item1;

                int start = Math.Min(pos1, pos2);
                int end = Math.Max(pos1, pos2);

                return Enumerable.Range(start, end - start + 1)
                    .Select(i => Hand.Grid.GetNotation(i, i))
                    .ToArray();
            }

            /// <summary>
            /// Returns all hands included in an open sub range (e.g. "66+", "74o+")
            /// </summary>
            /// <param name="startHand">"66" for an open range of "66+", "74o" for an open range of "74o+"</param>
            private static string[] GetOpenSubRange(string startHand)
            {
                if(startHand == null)
                    throw new ArgumentNullException(nameof(startHand));

                if(Hand.Grid.isPocketPair(startHand)) {
                    return Enumerable.Range(0, Hand.Grid.GetPosition(startHand).Item1 + 1)
                        .Select(i => Hand.Grid.GetNotation(i, i))
                        .ToArray();
                } else {
                    if(startHand.Length == 2) {
                        // No suit specified. Apply for both suited and offsuit.
                        return GetOpenSubRange(startHand + 's')
                            .Concat(GetOpenSubRange(startHand + 'o'))
                            .ToArray();
                    }

                    var pos = Hand.Grid.GetPosition(startHand);
                    int min = Math.Min(pos.Item1, pos.Item2);
                    int max = Math.Max(pos.Item1, pos.Item2);
                    return Enumerable.Range(min + 1, max - min)
                        .Select(i => Hand.Grid.GetNotation(max == pos.Item1 ? i : pos.Item1,
                                             max == pos.Item2 ? i : pos.Item2))
                        .ToArray();
                }
            }

            #endregion

            #endregion

            #region Get range notation

            public static string GetRangeNotation(Range range)
            {
                // Todo: Write a range notation converter
                var Representation = new StringBuilder();
                for(int x = 0; x < Hand.RankCount; x++) {
                    for(int y = 0; y < Hand.RankCount; y++) {
                        if(range[x, y]) {
                            Representation.Append(Hand.Grid.GetNotation(x, y));
                            Representation.Append(' ');
                        }
                    }
                }
                return Representation.ToString();
            }

            /// <summary>
            /// Shrinks a range notation using different pattern like sub ranges (e.g. "QQ+" for "QQ KK AA")
            /// </summary>
            /// <param name="notation">The range notation to be compressed</param>
            /// <returns>A range notation equal to the input, but with sub ranges and other patterns applied</returns>
            private static string CompressRangeNotation(string notation)
            {
                return GetRangeNotation(Range.Parse(notation));
            }

            #endregion

            #region Validation

            /// <summary>
            /// Test is a string representation of a range is valid.
            /// It tests if there are only valid face and suit values for the hole cards and sub ranges.
            /// </summary>
            /// <param name="rangeString">The string to validate</param>
            /// <returns>True if the string is valid, false otherwise.</returns>
            public static bool isValidNotation(string rangeString)
            {
                if(rangeString == null)
                    return false;
                if(rangeString == String.Empty)
                    return true;

                // Normalize the string representation
                return Regex.IsMatch(HoldemEvaluator.Notation.NormalizeRepresentation(rangeString), RangeNotationRegex);
            }

            #endregion
        }
    }
}
