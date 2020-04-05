using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public static partial class Hand
    {
        internal static class Bin
        {
            #region Card counting
            /// <summary>
            /// Counts how many cards are selected in the card collection.
            /// (Hamming weight)
            /// </summary>
            /// <returns>The amount of selected cards</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int GetCardCount(ulong cards)
            {
                cards &= Masks.ValidBitMask;

                if(cards == 0UL)
                    return 0;

                // Todo: Test for best improved version of bit count

                // Todo: validate output
                return
                    _trueBits[cards & 0x00000000000000FFUL] +
                    _trueBits[(cards & 0x000000000000FF00UL) >> 8] +
                    _trueBits[(cards & 0x0000000000FF0000UL) >> 16] +
                    _trueBits[(cards & 0x00000000FF000000UL) >> 24] +
                    _trueBits[(cards & 0x000000FF00000000UL) >> 32] +
                    _trueBits[(cards & 0x0000FF0000000000UL) >> 40] +
                    _trueBits[(cards & 0x000F000000000000UL) >> 48];

                //int count = 0;
                //while(cards != 0) {
                //    count++;
                //    cards &= (cards - 1);
                //}
                //return count;

                //cards = cards - ((cards >> 1) & 0x5555555555555555UL);
                //cards = (cards & 0x3333333333333333UL) + ((cards >> 2) & 0x3333333333333333UL);
                //return (int)(((cards + (cards >> 4) & 0xF0F0F0FF0F0F0FUL) * 0x10101011010101UL) >> 24);

                //cards = cards - ((cards >> 1) & 0x55555555);
                //cards = (cards & 0x33333333) + ((cards >> 2) & 0x33333333);
                //cards = (cards + (cards >> 4)) & 0x0f0f0f0f;
                //cards = cards + (cards >> 8);
                //cards = cards + (cards >> 16);
                //return (int)(cards & 0x3f);
            }

            /// <summary>
            /// Bit count table from snippets.org. Tells how many true bits there are for each byte.
            /// </summary>
            private static readonly byte[] _trueBits =
            {
                0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,  /* 0   - 15  */
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 16  - 31  */
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 32  - 47  */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 48  - 63  */
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 64  - 79  */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 80  - 95  */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 96  - 111 */
                3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 112 - 127 */
                1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,  /* 128 - 143 */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 144 - 159 */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 160 - 175 */
                3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 176 - 191 */
                2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,  /* 192 - 207 */
                3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 208 - 223 */
                3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,  /* 224 - 239 */
                4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8   /* 240 - 255 */
            };

            #endregion

            /// <summary>
            /// The rank of a card from a specified index.
            /// </summary>
            /// <param name="bitIndex">Index of the card in a cards-dataset</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int GetRankFromBitIndex(int bitIndex)
            {
                // Todo: LUT
                return (TotalCards - bitIndex - 1) / SuitCount;
            }

            /// <summary>
            /// The suit value of a card from a specified index.
            /// </summary>
            /// <param name="bitIndex">Index of the card in a cards-dataset</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int GetSuitFromBitIndex(int bitIndex)
            {
                // Todo: LUT
                return (TotalCards - bitIndex - 1) % SuitCount;
            }

            /// <summary>
            /// Determines the index of the bit which is set in the binary representation of this card
            /// </summary>
            /// <returns>Index of the bit ranging from 0 to 51. The higher the worse</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int GetBitIndex(ulong card)
            {
                return GetHighestBitIndex(card);
            }

            /// <summary>
            /// Determines the index of the left most bit (highest card) that is set (counts the number of trailing zeros)
            /// </summary>
            internal static int GetHighestBitIndex(ulong data)
            {
#if DEBUG
                if(GetCardCount(data) == 0)
                    throw new ArgumentException($"At least one of the first {TotalCards} bits (total amount of cards) has to be set", nameof(data));
#endif
                // number of trailing zeros
                int n = 64;
                if(data != 0)
                    n--;
                if((data & 0x00000000FFFFFFFFUL) != 0)
                    n -= 32;
                if((data & 0x0000FFFF0000FFFFUL) != 0)
                    n -= 16;
                if((data & 0x00FF00FF00FF00FFUL) != 0)
                    n -= 8;
                if((data & 0x0F0F0F0F0F0F0F0FUL) != 0)
                    n -= 4;
                if((data & 0x3333333333333333UL) != 0)
                    n -= 2;
                if((data & 0x5555555555555555UL) != 0)
                    n -= 1;
                return n;
            }

            /// <summary>
            /// Get a list of all bits by the index which are true and can represent a card (first 52 bits)
            /// </summary>
            internal static List<int> GetAllBitIndices(ulong cards)
            {
                // Todo: yield return
                var indices = new List<int>();
                for(var destPos = 0; destPos < TotalCards && cards != 0; destPos++) {
                    if((cards & 1) == 1)
                        indices.Add(destPos);
                    cards >>= 1;
                }
                return indices;
            }

            #region Shift 1 bit LUT

            /// <summary>
            /// Uses a lookup table to speed up a (1UL &lt;&lt; index) operation
            /// </summary>
            /// <param name="index">The index of the bit which is set to 1</param>
            /// <returns>A left shifted one</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ulong leftShift1bit(int index)
            {
                return leftShiftLUT[index];
            }

            private static readonly ulong[] leftShiftLUT = {1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 2097152, 4194304, 8388608, 16777216, 33554432, 67108864, 134217728, 268435456, 536870912, 1073741824, 2147483648, 4294967296, 8589934592, 17179869184, 34359738368, 68719476736, 137438953472, 274877906944, 549755813888, 1099511627776, 2199023255552, 4398046511104, 8796093022208, 17592186044416, 35184372088832, 70368744177664, 140737488355328, 281474976710656, 562949953421312, 1125899906842624, 2251799813685248 };

            #endregion

            #region Enumerable Ranges

            /// <summary>
            /// A list with continuous numbers from 0 to TotalCards - 1.
            /// </summary>
            internal static readonly List<int> totalCardsRange = Enumerable.Range(0, TotalCards).ToList();

            /// <summary>
            /// A list with continuous numbers from 0 to RankCount - 1.
            /// </summary>
            internal static readonly List<int> totalRanksRange = Enumerable.Range(0, RankCount).ToList();

            #endregion
        }
    }
}
