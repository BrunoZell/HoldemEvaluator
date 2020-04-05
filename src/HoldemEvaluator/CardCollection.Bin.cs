using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class CardCollection
    {
        /// <summary>
        /// A set of static functions to convert between a binary representation and lists of suits and ranks
        /// </summary>
        internal static class Bin
        {
            /// <summary>
            /// Get a list of all card that are contained in a binary format.
            /// Higher cards are first in the list.
            /// </summary>
            internal static IEnumerable<Card> GetAllCards(ulong cards)
            {
                var ranks = GetAllRanks(cards);
                var suits = GetAllSuits(cards);
                return Enumerable.Range(0, ranks.Length)
                    .Select(i => new Card(ranks[i], suits[i]));
            }

            /// <summary>
            /// Extract all ranks from the cards bit representation
            /// </summary>
            /// <returns>List of every rank included in the cards data.
            /// There might be duplicates as in the original data, but the suit information is lost.
            /// 0 is a duce, 12 is an ace. So it's easy to compare two ranks.</returns>
            internal static int[] GetAllRanks(ulong cards)
            {
                return Hand.Bin.GetAllBitIndices(cards)
                    .Select(Hand.Bin.GetRankFromBitIndex)
                    .ToArray();
            }

            /// <summary>
            /// Extracts all occurring suits in the hand
            /// </summary>
            /// <returns>List of every suit included in the cards data</returns>
            internal static int[] GetAllSuits(ulong cards)
            {
                // Todo: PERFORMANCE
                return Hand.Bin.GetAllBitIndices(cards)
                    .Select(Hand.Bin.GetSuitFromBitIndex)
                    .ToArray();
            }

            /// <summary>
            /// Helper arrays for a fast selection of 
            /// </summary>
            private static Dictionary<int, ulong[]> ranges = new Func<Dictionary<int, ulong[]>>(
            () =>
            {
                var returnValue = new Dictionary<int, ulong[]>();
                for(int i = 1; i <= Hand.TotalCards; i++) {
                    returnValue.Add(i, Enumerable.Range(0, i).Select(u => Hand.Bin.leftShift1bit(u)).ToArray());
                }
                return returnValue;
            })();

            /// <summary>
            /// Returns a random n bit number (as ulong) with a specified amount of bits set.
            /// </summary>
            /// <param name="trueBits">How many bits should be true</param>
            /// <param name="bitCount">The index of the highest potential true bit (bit-length of the largest number)</param>
            /// <returns>A random n bit number with m true bits</returns>
            internal static ulong RandomNBitNumber(int trueBits, int bitCount = Hand.TotalCards)
            {
#if DEBUG
                if(trueBits > bitCount)
                    throw new ArgumentException("There can't be more bits set than the number contains", nameof(trueBits));
                if(bitCount > Hand.TotalCards)
                    throw new ArgumentException($"There are only up to {Hand.TotalCards} supported", nameof(bitCount));
#endif
                if(trueBits == 0 || bitCount == 0)
                    return 0UL;

                if(bitCount == trueBits)
                    return ~(0xFFFFFFFFFFFFFFFFUL << bitCount);

                var dict = ranges[bitCount];

                if(trueBits == 1) {
                    return dict[Hand.Randomizer.Next(0, dict.Length)];
                }

                // the shuffle stays in the static field, but it doesn't matter because its only shuffled in this method again
                dict.Shuffle();
                // Todo: only shuffle that part what really is needed (or just take n random elements from list)
                // OR: Create a n bit number und shuffle bits directly using the above called algorithm!

                ulong returnValue = 0UL;
                while(trueBits-- != 0)
                    returnValue |= dict[trueBits];
                return returnValue;
            }

            /// <summary>
            /// Used by the ExpandRight-Function.
            /// Defined as a static variable to only allocate memory once.
            /// </summary>
            private static ulong[] expandArray = new ulong[5];

            /// <summary>
            /// The inverse of the compress right function moves bits from the low-order end of a register to
            /// positions given by a mask, while keeping the bits in order. For example, expand(0000abcd, 10011010) = a00bc0d0.
            /// </summary>
            /// <param name="source">The bits to move</param>
            /// <param name="mask">The mask of moving</param>
            /// <returns>The value expanded to the right according to the mask</returns>
            internal static ulong ExpandRight(ulong source, ulong mask)
            {
                ulong m0, mk, mp, mv, t;
                int i;
                m0 = mask; // Save original mask.
                mk = ~mask << 1; // We will count 0's to right.
                for(i = 0; i < 5; i++) {
                    mp = mk ^ (mk << 1); // Parallel suffix.
                    mp = mp ^ (mp << 2);
                    mp = mp ^ (mp << 4);
                    mp = mp ^ (mp << 8);
                    mp = mp ^ (mp << 16);
                    mp = mp ^ (mp << 32);
                    mv = mp & mask; // Bits to move.
                    expandArray[i] = mv;
                    mask = (mask ^ mv) | (mv >> (1 << i)); // Compress m.
                    mk = mk & ~mp;
                }
                for(i = 4; i >= 0; i--) {
                    mv = expandArray[i];
                    t = source << (1 << i);
                    source = (source & ~mv) | (t & mv);
                }
                return source & m0; // Clear out extraneous bits.

            }
        }
    }
}
