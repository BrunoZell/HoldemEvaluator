using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class CardCollection
    {
        // Todo: Check for multiple executed IEnumerables
        public static class Enum
        {
            /// <summary>
            /// Get all card collections with a specific amount of cards.
            /// </summary>
            /// <param name="cardAmount">The amount of cards every collection should have</param>
            /// <param name="excludedCards">All collections containing one or more cards from this parameter will be excluded</param>
            /// <returns>A list of card collections according to the parameters</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<CardCollection> All(int cardAmount, CardCollection excludedCards = null)
            {
                return All(cardAmount, (ulong)excludedCards).Select(cc => new CardCollection(cc));
            }

            internal static IEnumerable<ulong> All(int cardAmount, ulong excludedCards = 0UL)
            {
                // double check for saving an &-operation almost all of the time
                if(excludedCards == 0UL || (excludedCards & Hand.Masks.ValidBitMask) == 0UL) {
                    return AllBitCombos(cardAmount, Hand.TotalCards);
                } else {
                    return AllBitCombos(cardAmount, Hand.TotalCards, excludedBits: excludedCards);
                }
                //var allCombos = AllBitCombos(cardAmount, Hand.TotalCards);
                //if((excludedCards & Hand.Masks.ValidBitMask) == 0UL)
                //    return allCombos;
                //return allCombos.Where(cc => (cc & excludedCards) == 0UL);
            }

            /// <summary>
            /// Get all card collections with a specific amount of cards and specific included cards
            /// </summary>
            /// <param name="cardAmount">The amount of cards every collection should have</param>
            /// <param name="includedCards">Every collection must contain all of these cards</param>
            /// <param name="excludedCards">All collections containing one or more cards from this parameter will be excluded</param>
            /// <returns>A list of card collections according to the parameters</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<CardCollection> Include(int cardAmount, CardCollection includedCards, CardCollection excludedCards = null)
            {
                return Include(cardAmount, includedCards.Binary, excludedCards.Binary).Select(cc => new CardCollection(cc));
                //return All(cardAmount, excludedCards).Where(cc => cc.Contains(includedCards));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static IEnumerable<ulong> Include(int cardAmount, ulong includedCards, ulong excludedCards = 0UL)
            {
                return AllBitCombos(cardAmount - Hand.Bin.GetCardCount(includedCards), Hand.TotalCards, includedCards, excludedCards);
                // return All(cardAmount, excludedCards).Where(cc => (cc & includedCards) == includedCards);
            }

            /// <summary>
            /// Enumerates all possible bit combinations with a specific amount of true bits
            /// </summary>
            /// <param name="bitAmount">The amount of true bits that each value should include</param>
            /// <param name="bitRange">The scale of the search space to look for those numbers.
            /// E.g. in a search space of 2 there would be just two combos of one bit</param>
            /// <returns>An enumerable of all possible bit combinations with 'bitAmount' of true bit.</returns>
            internal static IEnumerable<ulong> AllBitCombos(int bitAmount, int bitRange)
            {
                if(bitAmount == 0) {
                    if(bitRange > 0)
                        yield return 0;
                    yield break;
                }

#if DEBUG
                if(bitRange > 64 || bitRange < 0)
                    throw new ArgumentOutOfRangeException(nameof(bitRange), bitRange, $"{nameof(bitRange)} has to be a positive number and smaller than 64 due to the maximum capacity of the data type long");

                if(bitAmount > bitRange)
                    throw new ArgumentException($"{nameof(bitAmount)} can't be larger than {nameof(bitRange)}", nameof(bitAmount));
#endif

                // v is the minimal possible number using 'bitAmount' number of true bits
                // v is the starting number for the sequence
                long v = (long)(~(0xFFFFFFFFFFFFFFFFUL << bitAmount));

                // Only scope numbers with a maximum of 'bitRange' amount of bits
                long max = (long)(~(0xFFFFFFFFFFFFFFFFUL << bitRange));
                do {
                    yield return (ulong)v;

                    // calculating next number with 'bitAmount' number of true bits
                    long t = (v | (v - 1)) + 1;
                    v = v == 0 ? 0 : t | ((((t & -t) / (v & -v)) >> 1) - 1);

                } while(v < max);
            }

            /// <summary>
            /// Enumerates all possible bit combinations with a specific amount of true bits with options to exclude specific bits and include other ones
            /// </summary>
            /// <param name="bitAmount">The amount of true bits that each value should include in addition to the included bits via the parameter.</param>
            /// <param name="bitRange">>The scale of the search space to look for those numbers.
            /// E.g. in a search space of 2 there would be just two combos of one bit</param>
            /// <param name="includedBits">A bit mask containing all bits that have to be set in each output</param>
            /// <param name="excludedBits">A bit mask containing all bits that are never set in any output</param>
            internal static IEnumerable<ulong> AllBitCombos(int bitAmount, int bitRange, ulong includedBits = 0UL, ulong excludedBits = 0UL)
            {
#if DEBUG
                if(bitRange > 64 || bitRange < 0)
                    throw new ArgumentOutOfRangeException(nameof(bitRange), bitRange, $"{nameof(bitRange)} has to be a positive number and smaller than 64 due to the maximum capacity of the data type long");

                // Hand.Bin.GetCardCount is slow, that's why we double check this, to avoid a call
                if(bitAmount > bitRange || Hand.Bin.GetCardCount(includedBits) + bitAmount > bitRange)
                    throw new ArgumentException($"{nameof(bitAmount)} can't be larger than {nameof(bitRange)}", nameof(bitAmount));

                if((includedBits & excludedBits) != 0)
                    throw new ArgumentException($"{nameof(includedBits)} and {nameof(includedBits)} are in conflict with each other", nameof(includedBits));
#endif
                // mask of variable combination bits (included and excluded bits removed)
                ulong mask = ~includedBits & ~excludedBits;

                // get lowest possible combo with mask
                ulong combo = Bin.ExpandRight(~(0xFFFFFFFFFFFFFFFFUL << bitAmount), mask);

                // Don't include any combo bigger than max
                ulong max = ~(0xFFFFFFFFFFFFFFFFUL << bitRange);
                do {
                    yield return combo | includedBits;

                    ulong tmp = combo - 1;
                    ulong set = mask;
                    ulong rip = set & ((tmp | combo) - set);
                    for(combo = (tmp ^ rip) & combo; combo != 0; rip ^= tmp, set ^= tmp) {
                        tmp = set & (ulong)(-(long)set);
                        combo &= combo - 1;
                    }
                    combo = rip;
                } while(combo <= max);
            }

            /// <summary>
            /// Calculates the number of all possible combinations of n items selected randomly from a set of x items
            /// </summary>
            /// <param name="totalAmount">Number of elements to choose from. The amount of the "n items"</param>
            /// <param name="chooseAmount">Number of elements to choose. The amount of the "x items"</param>
            /// <returns>The number of all possible combinations of selection</returns>
            internal static int CombinationCount(int totalAmount, int chooseAmount)
            {
                int num = 1, denom = 1;
                int i;
                if(chooseAmount > totalAmount - chooseAmount)
                    chooseAmount = totalAmount - chooseAmount;
                for(i = 0; i < chooseAmount; ++i) {
                    num *= (totalAmount - i);
                    denom *= (chooseAmount - i);
                }
                return num / denom;
            }
        }
    }
}