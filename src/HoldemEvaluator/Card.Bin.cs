using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial struct Card
    {
        /// <summary>
        /// A set of static functions to convert between a binary representation and single suits and ranks
        /// </summary>
        internal static class Bin
        {
            /// <summary>
            /// Extract the highest rank found. Ranks range from 0 (duce) to 12 (ace).
            /// </summary>
            internal static int GetRank(ulong card)
            {
#if DEBUG
                if(Hand.Bin.GetCardCount(card) < 1)
                    throw new ArgumentException("At least one bit has to be set to represent a card", nameof(card));
#endif
                return Hand.Bin.GetRankFromBitIndex(Hand.Bin.GetHighestBitIndex(card));
            }

            /// <summary>
            /// Extracts the suit out of the highest card (0 to 3)
            /// </summary>
            internal static int GetSuit(ulong card)
            {
#if DEBUG
                if(Hand.Bin.GetCardCount(card) < 1)
                    throw new ArgumentException("At least one bit has to be set to represent a card", nameof(card));
#endif
                return Hand.Bin.GetSuitFromBitIndex(Hand.Bin.GetHighestBitIndex(card));
            }

            #region Binary Card Representation
            /// <summary>
            /// Get the binary representation of a specific card
            /// </summary>
            internal static ulong GetBinary(int rank, int suit)
            {
                return binaryCardLUT[rank][suit];
                // Pre LUT:
                // return leftShift1bit(GetBitIndex(rank, suit));
            }

            private static readonly ulong[][] binaryCardLUT = {
                new ulong[]{ 2251799813685248, 1125899906842624, 562949953421312, 281474976710656 },
                new ulong[]{ 140737488355328, 70368744177664, 35184372088832, 17592186044416 },
                new ulong[]{ 8796093022208, 4398046511104, 2199023255552, 1099511627776 },
                new ulong[]{ 549755813888, 274877906944, 137438953472, 68719476736 },
                new ulong[]{ 34359738368, 17179869184, 8589934592, 4294967296 },
                new ulong[]{ 2147483648, 1073741824, 536870912, 268435456 },
                new ulong[]{ 134217728, 67108864, 33554432, 16777216 },
                new ulong[]{ 8388608, 4194304, 2097152, 1048576 },
                new ulong[]{ 524288, 262144, 131072, 65536 },
                new ulong[]{ 32768, 16384, 8192, 4096 },
                new ulong[]{ 2048, 1024, 512, 256 },
                new ulong[]{ 128, 64, 32, 16 },
                new ulong[]{ 8, 4, 2, 1 }
            };

            #endregion

            #region GetBitIndex

            /// <summary>
            /// Determines the index of the bit representing that specific rank and suit
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static byte GetBitIndex(int rank, int suit)
            {
                return bitIndexLUT[rank][suit];
                // Pre LUT:
                // return (byte)(-suit + Hand.RankCount * Hand.SuitCount - rank * Hand.SuitCount - 1);
            }

            private static readonly byte[][] bitIndexLUT = {
                new byte[]{51, 50, 49, 48},
                new byte[]{47, 46, 45, 44},
                new byte[]{43, 42, 41, 40},
                new byte[]{39, 38, 37, 36},
                new byte[]{35, 34, 33, 32},
                new byte[]{31, 30, 29, 28},
                new byte[]{27, 26, 25, 24},
                new byte[]{23, 22, 21, 20},
                new byte[]{19, 18, 17, 16},
                new byte[]{15, 14, 13, 12},
                new byte[]{11, 10, 9, 8},
                new byte[]{7, 6, 5, 4},
                new byte[]{3, 2, 1, 0}
            };

            #endregion
        }
    }
}
