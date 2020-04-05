using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class CardCollection
    {
        public static class Notation
        {
            #region Regex

            /// <summary>
            /// Regex for identifying a collection of single cards.
            /// Every suit must be specified. Case sensitive.
            /// Valid example:
            /// "As Kh 6c 3c 6s"
            /// Regex includes captures for each card (in group 2)
            /// </summary>
            public static readonly string CardCollectionRegex = @"^(?:([2-9TJQKA]{1}[dchs]{1})|\s*)*$";

            #endregion

            #region Parsing

            /// <summary>
            /// Parses a collection of single cards.
            /// </summary>
            /// <param name="collectionString">
            /// The string representation of the collection. E.g. "As Kh 6c 3c 6s" or "Jh, Th, Ks, 9h, 4d, 4s, Qc"
            /// </param>
            /// <returns>A CardCollection containing all cards the string specified</returns>
            public static CardCollection Parse(string collectionString)
            {
                // Hand contains either invalid strings or duplicate entries
                if(!isValidNotation(collectionString))
                    throw new NotACollectionException();

                // Normalize string
                collectionString = HoldemEvaluator.Notation.NormalizeRepresentation(collectionString);

                // Empty collection
                if(String.IsNullOrWhiteSpace(collectionString))
                    return new CardCollection();

                try {
                    return new CardCollection(SplitCards(collectionString)
                        .Where(str => Card.Notation.isValidNotation(str))
                        .Select(str => Card.Notation.Parse(str)));

                } catch(Exception ex) {
                    throw new NotACollectionException(null, ex);
                }
            }

            /// <summary>
            /// Splits a list of cards into each single one
            /// </summary>
            /// <param name="collectionString">A list of single cards ("6h As Tc")</param>
            /// <returns></returns>
            private static IEnumerable<string> SplitCards(string collectionString)
            {
                // Normalize string
                collectionString = HoldemEvaluator.Notation.NormalizeRepresentation(collectionString);

                // return all matches
                var match = Regex.Match(collectionString, CardCollectionRegex);
                if(match.Success && match.Groups.Count >= 2) {
                    foreach(Capture capture in match.Groups[1].Captures) {
                        yield return capture.Value;
                    }
                }
            }

            #endregion

            #region Get Notation

            /// <summary>
            /// Get the notation of a card collection
            /// </summary>
            /// <returns>String representation of the card collection, e.g. "Ac Th 6s "</returns>
            public static string GetNotation(CardCollection collection)
            {
                return GetNotation(collection.Binary);
            }

            /// <summary>
            /// Get the notation of a card collection
            /// </summary>
            /// <returns>String representation of the card collection, e.g. "Ac Th 6s "</returns>
            public static string GetNotation(ulong collection)
            {
                return String.Join(" ", Bin.GetAllCards(collection));
            }

            #endregion

            #region Validation

            /// <summary>
            /// Test is a string representation of a card collection is valid.
            /// It tests if there are only valid face and suit values are used and checks for duplicated cards.
            /// </summary>
            /// <param name="collectionString">The string to validate</param>
            /// <returns>True if the string is valid, false otherwise.</returns>
            public static bool isValidNotation(string collectionString)
            {
                if(collectionString == null)
                    return false;
                if(collectionString == String.Empty)
                    return true;

                // Normalize the string representation
                collectionString = HoldemEvaluator.Notation.NormalizeRepresentation(collectionString);
                if(!Regex.IsMatch(collectionString, CardCollectionRegex))
                    return false;

                // Testing for duplicated cards
                return !(collectionString.Split(' ')
                    .GroupBy(n => n)
                    .Any(c => c.Count() > 1));
            }

            #endregion
        }
    }
}
