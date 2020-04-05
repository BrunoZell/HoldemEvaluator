using System;

namespace HoldemEvaluator
{
    /// <summary>
    /// Low level helper class for handling
    /// </summary>
    public static partial class Hand
    {
        #region readonly values

        /// <summary>
        /// Number of different ranks, ignoring suits
        /// </summary>
        public const int RankCount = 13;

        /// <summary>
        /// Number of different suits
        /// </summary>
        public const int SuitCount = 4;

        /// <summary>
        /// Total number of cards including all suits
        /// </summary>
        public const int TotalCards = 52;

        /// <summary>
        /// A collection of useful bit masks when dealing with binary formats
        /// </summary>
        internal static class Masks
        {
            /// <summary>
            /// Masks for selecting cards with one common rank
            /// </summary>
            internal static readonly ulong[] RankBitMasks = {
                0x000F000000000000UL,
                0x0000F00000000000UL,
                0x00000F0000000000UL,
                0x000000F000000000UL,
                0x0000000F00000000UL,
                0x00000000F0000000UL,
                0x000000000F000000UL,
                0x0000000000F00000UL,
                0x00000000000F0000UL,
                0x000000000000F000UL,
                0x0000000000000F00UL,
                0x00000000000000F0UL,
                0x000000000000000FUL
            };

            /// <summary>
            /// Masks for selecting cards with one common suit
            /// </summary>
            internal static readonly ulong[] SuitBitMasks = {
                0x0008888888888888UL,
                0x0004444444444444UL,
                0x0002222222222222UL,
                0x0001111111111111UL
            };

            /// <summary>
            /// Mask for selecting only bits that represent a card (first 52 bits)
            /// </summary>
            internal const ulong ValidBitMask = 0x000FFFFFFFFFFFFFUL;

            /// <summary>
            /// Mask for selecting all the outer left ranks. Used in the procedure of detecting a four of a kind;
            /// </summary>
            internal const ulong FourOfaKindBitMask = 0x0001111111111111UL;

            /// <summary>
            /// Mask for selecting all A, 2, 3, 4, 5. Used in detecting a wheel straight
            /// </summary>
            internal const ulong WheelBitMasks = 0x000FFFF00000000FUL;
        }
        #endregion

        /// <summary>
        /// Used to get random values for creating new data structures
        /// </summary>
        internal static Random Randomizer = new Random();
    }
}
