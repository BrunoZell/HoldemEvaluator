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
        /// <summary>
        /// Functions for evaluating and ranking the holding in a collection of cards.
        /// </summary>
        public static class Eval
        {
            #region static data

            /// <summary>
            /// Collection of all hand value getters indexed by the corresponding holding (enum can be used)
            /// </summary>
            private static readonly Func<ulong, int>[] GetHoldings = {
                GetHighCard,
                GetPair,
                GetTwoPair,
                GetThreeOfAKind,
                GetStraight,
                GetFlush,
                GetFullHouse,
                GetFourOfAKind,
                GetStraightFlush
            };

            /// <summary>
            /// Collection of all hand value getters indexed by the corresponding holding (enum can be used)
            /// </summary>
            private static readonly Func<ulong, int>[] GetHoldingStrengths = {
                GetHighCardStrength,
                GetPairStrength,
                GetTwoPairStrength,
                GetThreeOfAKindStrength,
                GetStraightStrength,
                GetFlushStrength,
                GetFullHouseStrength,
                GetFourOfAKindStrength,
                GetStraightFlushStrength
            };

            /// <summary>
            /// Enumeration of all holdem holdings starting with the lowest
            /// </summary>
            public enum Holdings
            {
                HighCard,
                Pair,
                TwoPair,
                ThreeOfAKind,
                Straight,
                Flush,
                FullHouse,
                FourOfAKind,
                StraightFlush
            }

            // Todo: Documentation

            private static readonly int HANDTYPE_SHIFT = 24;
            private static readonly uint HANDTYPE_VALUE_HIGHCARD = (uint)Holdings.HighCard << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_PAIR = (uint)Holdings.Pair << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_TWOPAIR = (uint)Holdings.TwoPair << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_TRIPS = (uint)Holdings.ThreeOfAKind << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_STRAIGHT = (uint)Holdings.Straight << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_FLUSH = (uint)Holdings.Flush << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_FULLHOUSE = (uint)Holdings.FullHouse << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_FOUR_OF_A_KIND = (uint)Holdings.FourOfAKind << HANDTYPE_SHIFT;
            private static readonly uint HANDTYPE_VALUE_STRAIGHTFLUSH = (uint)Holdings.StraightFlush << HANDTYPE_SHIFT;

            private static readonly int TOP_CARD_SHIFT = 16;
            private static readonly uint TOP_CARD_MASK = 0x000F0000;
            private static readonly int SECOND_CARD_SHIFT = 12;
            private static readonly uint SECOND_CARD_MASK = 0x0000F000;
            private static readonly int THIRD_CARD_SHIFT = 8;
            private static readonly uint FIFTH_CARD_MASK = 0x0000000F;
            private static readonly int CARD_WIDTH = 4;

            /// <summary>
            /// The number of possible different combinations per hand starting from high card (0th) to straight flush (12th)
            /// </summary>
            private static readonly int[] RankingPossibilities = {
                1287, 3718, 1014, 1014, 10, 1287, 156, 1014, 10
            };

            /// <summary>
            /// How many kickers are required for the different types of holdings. Indexed from high card (0th) to straight flush (12th).
            /// </summary>
            private static readonly int[] KickersForHolding = {
                4, 3, 1, 2, 0, 0, 0, 1, 0
            };

            /// <summary>
            /// The number of possible different combinations of x kickers where x is the index used to access the value. (max x of 5)
            /// </summary>
            private static readonly int[] KickerCombos = {
                0, 13, 78, 286, 715, 1287
            };


            /// <summary>
            /// Generates a ranking list for all possible n-kicker combos represented in a 13-bit binary format.
            /// 0110010101000 are the kickers {12,11,8,6,4}
            /// </summary>
            /// <param name="kickerAmount">How many kickers are there to rank? Normally In range 1 to 5.</param>
            /// <returns>All possible bit combinations in rising sequence.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<ulong> GenerateKickerRanking(int kickerAmount)
            {
                return CardCollection.Enum.AllBitCombos(kickerAmount, RankCount);
            }

            #endregion

            #region Binary Format conversion

            /// <summary>
            /// Method for converting from the rank-grouped representation to the suit-grouped representation,
            /// but for each suit a different 13 bit uint.
            /// </summary>
            /// <param name="cards">The source rank-grouped binary representation of the cards</param>
            /// <param name="spades">To save all the spade ranks</param>
            /// <param name="hearts">To save all the heart ranks</param>
            /// <param name="clubs">To save all the club ranks</param>
            /// <param name="diamonds">To save all the diamond ranks</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ConvertToSuits(ulong cards, out uint spades, out uint hearts, out uint clubs, out uint diamonds)
            {
                spades =   Reverse(ShiftBits(Masks.SuitBitMasks[3] & cards, 0));
                hearts =   Reverse(ShiftBits(Masks.SuitBitMasks[2] & cards, 1));
                clubs =    Reverse(ShiftBits(Masks.SuitBitMasks[1] & cards, 2));
                diamonds = Reverse(ShiftBits(Masks.SuitBitMasks[0] & cards, 3));
            }

            /// <summary>
            /// Shifts the bit from the rank-grouped representation to the
            /// suit-grouped representation for one specified suit.
            /// </summary>
            /// <param name="suitedCards">The 52-bit rank-grouped representation with only cards from the desired suit</param>
            /// <param name="shiftOffset">The trailing zeros of the left most possible bit of this suit (usually suitCount - suitIndex)</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static uint ShiftBits(ulong suitedCards, int shiftOffset)
            {
                return (uint)((suitedCards & Masks.RankBitMasks[12]) >> shiftOffset |
                             ((suitedCards & Masks.RankBitMasks[11]) >> (shiftOffset + 3)) |
                             ((suitedCards & Masks.RankBitMasks[10]) >> (shiftOffset + 6)) |
                             ((suitedCards & Masks.RankBitMasks[9]) >> (shiftOffset + 9)) |
                             ((suitedCards & Masks.RankBitMasks[8]) >> (shiftOffset + 12)) |
                             ((suitedCards & Masks.RankBitMasks[7]) >> (shiftOffset + 15)) |
                             ((suitedCards & Masks.RankBitMasks[6]) >> (shiftOffset + 18)) |
                             ((suitedCards & Masks.RankBitMasks[5]) >> (shiftOffset + 21)) |
                             ((suitedCards & Masks.RankBitMasks[4]) >> (shiftOffset + 24)) |
                             ((suitedCards & Masks.RankBitMasks[3]) >> (shiftOffset + 27)) |
                             ((suitedCards & Masks.RankBitMasks[2]) >> (shiftOffset + 30)) |
                             ((suitedCards & Masks.RankBitMasks[1]) >> (shiftOffset + 33)) |
                             ((suitedCards & Masks.RankBitMasks[0]) >> (shiftOffset + 36)));
            }

            // Todo: Reverse all lookup tables to save this operation
            /// <summary>
            /// Reverses a 13 bit integer
            /// </summary>
            private static uint Reverse(uint x)
            {
                uint y = 0;
                for (int i = 0; i < 13; ++i) {
                    y <<= 1;
                    y |= (x & 1);
                    x >>= 1;
                }
                return y;
            }

            #endregion

            #region Get holding information

            /// <summary>
            /// Get the type of holding the cards represent
            /// </summary>
            /// <returns>The type of holding represented as an enum</returns>
            public static Holdings GetHoldingType(ulong cards)
            {
                for(int i = GetHoldings.Length - 1; i >= 0; i--) {
                    int current = GetHoldings[i](cards);
                    if(!isError(current)) {
                        return (Holdings)i;
                    }
                }
                return Holdings.HighCard;
            }

            /// <summary>
            /// Get the value of the best possible holding using these cards
            /// </summary>
            /// <returns>An int array containing ranks where the first ranks
            /// are the most significant ones, e.g. the high pair in a two pair hand
            /// and the second value would be the face of the second pair in the two pair hand.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetHolding(ulong cards)
            {
                return GetHoldings[(int)GetHoldingType(cards)](cards);
            }

            /// <summary>
            /// Get the value of the specific type of holding using these cards
            /// </summary>
            /// <param name="holdingType">Which type of holding to analyze</param>
            /// <returns>An int array containing ranks where the first ranks
            /// are the most significant ones, e.g. the high pair in a two pair hand
            /// and the second value would be the face of the second pair in the two pair hand.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetHolding(ulong cards, Holdings holdingType)
            {
                return GetHoldings[(int)holdingType](cards);
            }

            /// <summary>
            /// Get a rank of the current holding.
            /// The higher the rank the better the holding. Equal ranks are equal value (split pots).
            /// </summary>
            /// <returns>Lowest rank is 1. Rank is -1 on error.</returns>
            public static uint GetHandStrength(ulong cards)
            {
                int cardCount = Bin.GetCardCount(cards);    // How many distinct cards are in the hand
#if DEBUG
                if(cardCount < 1 || cardCount > 7)
                    throw new ArgumentOutOfRangeException(nameof(cards), cards, "This function only supports 1 to 7 card hands");
#endif
                ConvertToSuits(cards, out uint ss, out uint sh, out uint sc, out uint sd);

                uint ranks = sc | sd | sh | ss;
                ushort rankCount = hammeringWeightLUT[ranks];  // How many distinct ranks are in the hand
                uint rankDuplicates = (uint)(cardCount - rankCount); // How many cards have the same rank as another

                uint returnValue = 0U;
                uint two_mask, three_mask, four_mask;

                /* Check for straight, flush, or straight flush, and return if we can
                   determine immediately that this is the best possible hand 
                */
                if(rankCount >= 5) {
                    if(hammeringWeightLUT[ss] >= 5) {
                        if(straightHighCardLUT[ss] != 0)
                            return HANDTYPE_VALUE_STRAIGHTFLUSH + ((uint)straightHighCardLUT[ss] << TOP_CARD_SHIFT);
                        else
                            returnValue = HANDTYPE_VALUE_FLUSH + topFiveCardsLUT[ss];
                    } else if(hammeringWeightLUT[sc] >= 5) {
                        if(straightHighCardLUT[sc] != 0)
                            return HANDTYPE_VALUE_STRAIGHTFLUSH + ((uint)straightHighCardLUT[sc] << TOP_CARD_SHIFT);
                        else
                            returnValue = HANDTYPE_VALUE_FLUSH + topFiveCardsLUT[sc];
                    } else if(hammeringWeightLUT[sd] >= 5) {
                        if(straightHighCardLUT[sd] != 0)
                            return HANDTYPE_VALUE_STRAIGHTFLUSH + ((uint)straightHighCardLUT[sd] << TOP_CARD_SHIFT);
                        else
                            returnValue = HANDTYPE_VALUE_FLUSH + topFiveCardsLUT[sd];
                    } else if(hammeringWeightLUT[sh] >= 5) {
                        if(straightHighCardLUT[sh] != 0)
                            return HANDTYPE_VALUE_STRAIGHTFLUSH + ((uint)straightHighCardLUT[sh] << TOP_CARD_SHIFT);
                        else
                            returnValue = HANDTYPE_VALUE_FLUSH + topFiveCardsLUT[sh];
                    } else {
                        uint st = straightHighCardLUT[ranks];
                        if(st != 0)
                            returnValue = HANDTYPE_VALUE_STRAIGHT + (st << TOP_CARD_SHIFT);
                    };

                    /* 
                       Another win -- if there can't be a FH/Quads (n_dups < 3), 
                       which is true most of the time when there is a made hand, then if we've
                       found a five card hand, just return.  This skips the whole process of
                       computing two_mask/three_mask/etc.
                    */
                    if(returnValue != 0 && rankDuplicates < 3)
                        return returnValue;
                }

                /*
                 * By the time we're here, either: 
                   1) there's no five-card hand possible (flush or straight), or
                   2) there's a flush or straight, but we know that there are enough
                      duplicates to make a full house / quads possible.  
                 */
                switch(rankDuplicates) {
                case 0:
                    /* It's a no-pair hand */
                    return HANDTYPE_VALUE_HIGHCARD + topFiveCardsLUT[ranks];
                case 1: {
                        /* It's a one-pair hand */
                        uint t, kickers;

                        two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);

                        returnValue = (uint)(HANDTYPE_VALUE_PAIR + (topCardLUT[two_mask] << TOP_CARD_SHIFT));
                        t = ranks ^ two_mask;      /* Only one bit set in two_mask */
                        /* Get the top five cards in what is left, drop all but the top three 
                         * cards, and shift them by one to get the three desired kickers */
                        kickers = (topFiveCardsLUT[t] >> CARD_WIDTH) & ~FIFTH_CARD_MASK;
                        returnValue += kickers;
                        return returnValue;
                    }

                case 2:
                    /* Either two pair or trips */
                    two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);
                    if(two_mask != 0) {
                        uint t = ranks ^ two_mask; /* Exactly two bits set in two_mask */
                        returnValue = (uint)(HANDTYPE_VALUE_TWOPAIR
                            + (topFiveCardsLUT[two_mask]
                            & (TOP_CARD_MASK | SECOND_CARD_MASK))
                            + (topCardLUT[t] << THIRD_CARD_SHIFT));

                        return returnValue;
                    } else {
                        uint t, second;
                        three_mask = ((sc & sd) | (sh & ss)) & ((sc & sh) | (sd & ss));
                        returnValue = (uint)(HANDTYPE_VALUE_TRIPS + (topCardLUT[three_mask] << TOP_CARD_SHIFT));
                        t = ranks ^ three_mask; /* Only one bit set in three_mask */
                        second = topCardLUT[t];
                        returnValue += (second << SECOND_CARD_SHIFT);
                        t ^= (1U << (int)second);
                        returnValue += (uint)(topCardLUT[t] << THIRD_CARD_SHIFT);
                        return returnValue;
                    }

                default:
                    /* Possible quads, fullhouse, straight or flush, or two pair */
                    four_mask = sh & sd & sc & ss;
                    if(four_mask != 0) {
                        uint tc = topCardLUT[four_mask];
                        returnValue = (uint)(HANDTYPE_VALUE_FOUR_OF_A_KIND
                            + (tc << TOP_CARD_SHIFT)
                            + ((topCardLUT[ranks ^ (1U << (int)tc)]) << SECOND_CARD_SHIFT));
                        return returnValue;
                    };

                    /* Technically, three_mask as defined below is really the set of
                       bits which are set in three or four of the suits, but since
                       we've already eliminated quads, this is OK */
                    /* Similarly, two_mask is really two_or_four_mask, but since we've
                       already eliminated quads, we can use this shortcut */

                    two_mask = ranks ^ (sc ^ sd ^ sh ^ ss);
                    if(hammeringWeightLUT[two_mask] != rankDuplicates) {
                        /* Must be some trips then, which really means there is a 
                           full house since n_dups >= 3 */
                        uint tc, t;
                        three_mask = ((sc & sd) | (sh & ss)) & ((sc & sh) | (sd & ss));
                        returnValue = HANDTYPE_VALUE_FULLHOUSE;
                        tc = topCardLUT[three_mask];
                        returnValue += (tc << TOP_CARD_SHIFT);
                        t = (two_mask | three_mask) ^ (1U << (int)tc);
                        returnValue += (uint)(topCardLUT[t] << SECOND_CARD_SHIFT);
                        return returnValue;
                    };

                    if(returnValue != 0) /* flush and straight */
                        return returnValue;
                    else {
                        /* Must be two pair */
                        uint top, second;

                        returnValue = HANDTYPE_VALUE_TWOPAIR;
                        top = topCardLUT[two_mask];
                        returnValue += (top << TOP_CARD_SHIFT);
                        second = topCardLUT[two_mask ^ (1 << (int)top)];
                        returnValue += (second << SECOND_CARD_SHIFT);
                        returnValue += (uint)((topCardLUT[ranks ^ (1U << (int)top) ^ (1 << (int)second)]) << THIRD_CARD_SHIFT);
                        return returnValue;
                    }
                }
            }

            /// <summary>
            /// Creates a string description of the holding incl. information of high card.
            /// No kicker information.
            /// </summary>
            /// <returns>A readable representation of the holding</returns>
            public static string GetHandDescription(ulong cards)
            {
                for(int i = GetHoldings.Length - 1; i >= 0; i--) {
                    var Current = GetHoldings[i](cards);
                    if(!isError(Current)) {
                        // Best holding found
                        // Todo: further rank description
                        return $"{Enum.GetName(typeof(Holdings), i)}";//, {String.Join(", ", Current.Select(c => Notation.Ranks[c]))}";
                    }
                }
                return String.Empty;
            }

            #endregion

            #region StraightFlush

            /// <summary>
            /// If in the hand is a straight flush it will calculate the high card
            /// </summary>
            /// <returns>High card of the straight flush. On error -1.</returns>
            private static int GetStraightFlush(ulong cards)
            {
                var highCard = cards & (cards >> 4) & (cards >> 8) & (cards >> 12) & (cards >> 16);
                if(Hand.Bin.GetCardCount(highCard) == 0) {
                    // Test for a wheel
                    for(int s = 0; s < SuitCount; s++) {
                        ulong suitedCards = cards & Masks.SuitBitMasks[s];
                        ulong suitedWheel = Masks.WheelBitMasks & Masks.SuitBitMasks[s];
                        if((suitedWheel & suitedCards) == suitedWheel) {
                            // Wheel found
                            return 3;
                        }
                    }
                    return -1; // No straight flush found
                }
                return Card.Bin.GetRank(highCard);
            }

            /// <summary>
            /// If in the hand is a straight flush it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the straight flush starting from 0 for the lowest possible. On error -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetStraightFlushStrength(ulong cards)
            {
                return GetStraightFlush(cards);
            }

            #endregion

            #region Four of a kind 

            /// <summary>
            /// If in the hand is a four of a kind  it will calculate the face card value
            /// </summary>
            /// <returns>Face card value of the four of a kind. On error -1.</returns>
            private static int GetFourOfAKind(ulong cards)
            {
                var rank = cards & (cards >> 1) & (cards >> 2) & (cards >> 3) & Masks.FourOfaKindBitMask;
                if(Hand.Bin.GetCardCount(rank) == 0)
                    return -1; // No four of a kind found
                return Card.Bin.GetRank(rank);
            }

            /// <summary>
            /// If in the hand is a four of a kind it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the four of a kind starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetFourOfAKindStrength(ulong cards)
            {
                var fourOfAKind = GetFourOfAKind(cards);
                if(isError(fourOfAKind))
                    return -1;

                // Remove card bits of the three of a kind
                cards &= ~(Masks.RankBitMasks[fourOfAKind]);

                int kickerAmt = KickersForHolding[(int)Holdings.FourOfAKind];
                int kickerRank = GetKickerStrength(cards, kickerAmt);
                if(kickerRank == -1)
                    return -1;
                return fourOfAKind * KickerCombos[kickerAmt] + kickerRank;
            }

            #endregion

            #region Full house

            /// <summary>
            /// If in the hand is a full house it will calculate the face card values
            /// </summary>
            /// <returns>Face card values of the full house. On error -1.</returns>
            private static int GetFullHouse(ulong cards)
            {
                int ranks = 0;
                bool firstPairFound = false;
                for(int i = Masks.RankBitMasks.Length - 1; i >= 0; i--) {
                    // Loop through all ranks and test for three of a kinds,
                    // after one is found it is searched for a pair
                    if(Hand.Bin.GetCardCount(cards & Masks.RankBitMasks[i]) == (!firstPairFound ? 3 : 2)) {
                        if(firstPairFound) {
                            ranks = SetBinaryRank(ranks, i, 1);
                            return SetBinaryRankCount(ranks, 2);
                        } else {
                            ranks = SetBinaryRank(ranks, i, 0);
                            firstPairFound = true;
                            i = Masks.RankBitMasks.Length;
                        }
                    }
                }
                return -1;
            }

            /// <summary>
            /// If in the hand is a full house it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the full house starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetFullHouseStrength(ulong cards)
            {
                var fullHouse = GetFullHouse(cards);
                if(isError(fullHouse) || GetBinaryRankCount(fullHouse) != 2)
                    return -1;

                // Save ranks for performance
                int rankA = GetBinaryRank(fullHouse, 0);
                int rankB = GetBinaryRank(fullHouse, 1);

                // Remove card bits of the full house
                cards &= ~(Masks.RankBitMasks[rankA]);
                cards &= ~(Masks.RankBitMasks[rankB]);

                return O(rankA) - (rankA - rankB) - 1;
            }

            // Todo: rename functions
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int O(int c)
            {
                return (c * (c + 1)) / 2;
            }

            #endregion

            #region Flush

            /// <summary>
            /// If in the hand is a flush it will calculate the high card value
            /// </summary>
            /// <returns>High card value of the flush. On error -1.</returns>
            private static int GetFlush(ulong cards)
            {
                for(int s = 0; s < SuitCount; s++) {
                    ulong suitedCards = cards & Masks.SuitBitMasks[s];
                    if(Hand.Bin.GetCardCount(suitedCards) >= 5) {
                        // Flush found
                        return Hand.Bin.GetRankFromBitIndex(Hand.Bin.GetHighestBitIndex(suitedCards));
                    }
                }
                return -1; // No flush found
            }

            /// <summary>
            /// If in the hand is a flush it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the flush starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetFlushStrength(ulong cards)
            {
                for(int s = 0; s < SuitCount; s++) {
                    ulong suitedCards = cards & Masks.SuitBitMasks[s];
                    if(Hand.Bin.GetCardCount(suitedCards) >= 5) {
                        // Flush found
                        return GetKickerStrength(suitedCards, 5);
                    }
                }
                return -1; // No flush found
            }

            #endregion

            #region Straight

            /// <summary>
            /// If in the hand is a straight it will calculate the high card value
            /// </summary>
            /// <returns>High card value of the straight. On error -1.</returns>
            private static int GetStraight(ulong cards)
            {
                foreach(var BitMask in Masks.RankBitMasks.Where(bm => Hand.Bin.GetCardCount(cards & bm) > 0))
                    cards |= BitMask;

                var HighCard = cards & (cards >> 4) & (cards >> 8) & (cards >> 12) & (cards >> 16);
                if(Hand.Bin.GetCardCount(HighCard) == 0) {
                    // Test for a wheel
                    ulong wheelCards = 0UL;
                    foreach(int f in new int[] { 0, 1, 2, 3, 12 }) {
                        if(Hand.Bin.GetCardCount(cards & Masks.RankBitMasks[f]) > 0)
                            wheelCards |= Masks.RankBitMasks[f];
                    }
                    if(wheelCards == Masks.WheelBitMasks) {
                        // Wheel found
                        return 3;
                    }
                    return -1; // No straight flush found
                }
                return Card.Bin.GetRank(HighCard);
            }

            /// <summary>
            /// If in the hand is a straight it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the straight starting from 0 for the lowest possible. On error -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetStraightStrength(ulong cards)
            {
                return GetStraight(cards);
            }

            #endregion

            #region Three of a kind

            /// <summary>
            /// If in the hand is a three of a kind  it will calculate the face card value
            /// </summary>
            /// <returns>Face card value of the three of a kind. On error -1.</returns>
            private static int GetThreeOfAKind(ulong cards)
            {
                for(int i = Masks.RankBitMasks.Length - 1; i >= 0; i--) {
                    // Loop through all ranks and test for three of a kinds
                    if(Hand.Bin.GetCardCount(cards & Masks.RankBitMasks[i]) == 3)
                        return i;
                }
                return -1;
            }

            /// <summary>
            /// If in the hand is a three of a kind it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the three of a kind starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetThreeOfAKindStrength(ulong cards)
            {
                var ThreeOfAKind = GetThreeOfAKind(cards);
                if(isError(ThreeOfAKind))
                    return -1;

                // Remove card bits of the three of a kind
                cards &= ~(Masks.RankBitMasks[ThreeOfAKind]);

                int KickerAmt = KickersForHolding[(int)Holdings.ThreeOfAKind];
                int KickerRank = GetKickerStrength(cards, KickerAmt);
                if(KickerRank == -1)
                    return -1;
                return ThreeOfAKind * KickerCombos[KickerAmt] + KickerRank;
            }

            #endregion

            #region Two pair

            /// <summary>
            /// If in the hand is a two pair it will calculate the face card values
            /// </summary>
            /// <returns>Face card values of the two pair. On error -1.</returns>
            private static int GetTwoPair(ulong cards)
            {
                int ranks = 0;
                bool firstPairFound = false;
                for(int i = Masks.RankBitMasks.Length - 1; i >= 0; i--) {
                    // Loop through all ranks and test for a pair,
                    // if one is found the search is continued to find a pair lower than the first one.
                    // In case of a four of a kind no pair is found.
                    if(Hand.Bin.GetCardCount(cards & Masks.RankBitMasks[i]) == 2) {
                        if(firstPairFound) {
                            ranks = SetBinaryRank(ranks, i, 1);
                            return SetBinaryRankCount(ranks, 2);
                        } else {
                            ranks = SetBinaryRank(ranks, i, 0);
                            firstPairFound = true;
                        }
                    }
                }
                return -1;
            }

            /// <summary>
            /// If in the hand is a two pair it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the two pair starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetTwoPairStrength(ulong cards)
            {
                var twoPair = GetTwoPair(cards);
                if(isError(twoPair) || GetBinaryRankCount(twoPair) != 2)
                    return -1;

                // Save ranks for performance
                int rankA = GetBinaryRank(twoPair, 0);
                int rankB = GetBinaryRank(twoPair, 1);

                // Remove card bits of the two pair
                cards &= ~(Masks.RankBitMasks[rankA]);
                cards &= ~(Masks.RankBitMasks[rankB]);

                // Get static kicker info
                int kickerAmt = KickersForHolding[(int)Holdings.TwoPair];
                int kickerRank = GetKickerStrength(cards, kickerAmt);
                if(isError(kickerRank))
                    return -1;

                // Calculate the unique strength for this holding 
                return (O(rankA) - (rankA - rankB) - 1) *
                    KickerCombos[kickerAmt] + kickerRank;
            }

            #endregion

            #region Pair

            /// <summary>
            /// If in the hand is a Pair it will calculate the face card value
            /// </summary>
            /// <returns>Face card value of the Pair. On error -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetPair(ulong cards)
            {
                for(int i = Masks.RankBitMasks.Length - 1; i >= 0; i--) {
                    // Loop through all ranks and test for a pair
                    if(Hand.Bin.GetCardCount(cards & Masks.RankBitMasks[i]) == 2)
                        return i;
                }
                return -1;
            }

            /// <summary>
            /// If in the hand is a Pair it will calculate rank of it.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the Pair starting from 0 for the lowest possible. On error -1.</returns>
            private static int GetPairStrength(ulong cards)
            {
                var pair = GetPair(cards);
                if(isError(pair))
                    return -1;

                // Remove card bits of the pair
                cards &= ~(Masks.RankBitMasks[pair]);

                int kickerAmt = KickersForHolding[(int)Holdings.Pair];
                int kickerRank = GetKickerStrength(cards, kickerAmt);
                if(kickerRank == -1)
                    return -1;
                return pair * KickerCombos[kickerAmt] + kickerRank;
            }

            #endregion

            #region High card
            /// <summary>
            /// It will find the highest rank in the cards
            /// </summary>
            /// <returns>Highest card value. On error -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetHighCard(ulong cards)
            {
                return Bin.GetRankFromBitIndex(Bin.GetHighestBitIndex(cards));
            }

            /// <summary>
            /// It will calculate rank of the high card hand.
            /// The higher the rank the better the holding. Equal ranks are equal value.
            /// </summary>
            /// <returns>Rank of the high card starting from 0 for the lowest possible. On error -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetHighCardStrength(ulong cards)
            {
                return GetKickerStrength(cards, 5);
            }
            #endregion

            #region Kicker

            /// <param name="cards">A 5 or 7 cards hand with the cards used in the actual holding REMOVED, so just cards that could be kicker should be set.</param>
            /// <param name="amount">How many kickers to search for</param>
            private static int GetKicker(ulong cards, int amount = 1)
            {
#if DEBUG
                if(amount < 0)
                    throw new ArgumentException("Amount has to be greater than zero", nameof(amount));
#endif

                int kicker = 0; // Binary data containing all kicker data
                int kickerAmt = 0; // Counts how many kickers were found

                for(; kickerAmt < amount && Hand.Bin.GetCardCount(cards) != 0; kickerAmt++) {
                    // Get next highest rank left in the cards
                    int rank = Bin.GetRankFromBitIndex(Bin.GetHighestBitIndex(cards));

                    // add found rank to the binary data
                    kicker = SetBinaryRank(kicker, rank, kickerAmt);

                    // remove found kicker from hand
                    cards &= ~Masks.RankBitMasks[rank];
                }

                // Set the binary count and return the binary value
                return SetBinaryRankCount(kicker, kickerAmt + 1);
            }

            /// <summary>
            /// Get the ranking for a pure kicker hand. The higher the better. Minimum 0, on error -1
            /// </summary>
            /// <param name="amount">The amount of kickers to consider can vary from 0 to 5</param>
            private static int GetKickerStrength(ulong cards, int amount = 5)
            {
                if(amount < 0)
                    return -1;
                if(amount == 0)
                    return 0;
                if(amount > 5)
                    return -1;

                // Get kicker list from cards. (The actual high card is also a kicker)
                int kickers = GetKicker(cards, amount);
                if(isError(kickers))
                    return -1;

                int kickerAmt = GetBinaryRankCount(kickers);
                if(kickerAmt != amount)
                    return -1;

                if(amount == 1)
                    return GetBinaryRank(kickers, 0);

                // Convert it into a binary 13-bit format
                ulong binaryRep = 0UL;
                for(int i = 0; i < kickerAmt; i++) {
                    binaryRep |= Bin.leftShift1bit(GetBinaryRank(kickers, i));
                }

                // Search binary number in the rankings. If not found return -1
                return Array.IndexOf(kickerRankingLUT[kickerAmt], binaryRep);
            }
            #endregion

            #region Binary format helper

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool isError(int data)
            {
                return data == -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetBinaryRank(int binaryData, int index)
            {
                return (binaryData >> (index * 4)) & 0xF;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int SetBinaryRank(int binaryData, int rank, int index)
            {
                return binaryData | (rank << (index * 4));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetBinaryRankCount(int binaryData)
            {
                return (binaryData >> 28) & 0x7;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int SetBinaryRankCount(int binaryData, int count)
            {
                return binaryData | ((count & 0x7) << 28);
            }

            #endregion
        }
    }
}
