using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial struct HoleCards
    {
        public static class Enum
        {
            #region All

            private static readonly HoleCards[] _all = CardCollection.Enum.AllBitCombos(2, Hand.TotalCards)
                                                                          .Select(hc => new HoleCards(hc))
                                                                          .ToArray();

            /// <summary>
            /// A collection of all possible hole cards with optional dead cards.
            /// </summary>
            /// <param name="deadCards">Dead cards. If no dead cards needed leave it null</param>
            public static IEnumerable<HoleCards> All(CardCollection deadCards = null)
            {
                if(deadCards == null)
                    return _all;

                return All((ulong)deadCards);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static IEnumerable<HoleCards> All(ulong deadCards = 0UL)
            {
                if(deadCards == 0UL)
                    return _all;

                return _all.Where(hc => (deadCards & (hc.HighCard.Binary | hc.LowCard.Binary)) == 0UL);
            }

            #endregion

            #region Include

            /// <summary>
            /// A collection of all possible hole cards which include one or more cards from a card collection with optional dead cards.
            /// </summary>
            /// <param name="card"></param>
            /// <param name="deadCards">Dead cards. If no dead cards needed leave it null</param>
            public static IEnumerable<HoleCards> Include(CardCollection includedCards, CardCollection deadCards = null) =>
                Include((ulong)includedCards, (ulong)deadCards);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static IEnumerable<HoleCards> Include(ulong includedCards, ulong deadCards = 0UL) =>
                All(deadCards).Where(hc => (includedCards & (hc.HighCard.Binary | hc.LowCard.Binary)) != 0UL);

            #endregion

            #region Grid cell

            #region Bit masks 

            private static readonly ulong[] AllPocketPairs = {
                0x3, // 0011
                0x5, // 0101
                0x6, // 0110
                0x9, // 1001
                0xA, // 1010
                0xC, // 1100
            };

            private static readonly ulong[] AllSingleCards = {
                0x1, // 0001
                0x2, // 0010
                0x4, // 0100
                0x8, // 1000
            };

            #endregion

            /// <summary>
            /// Iterate over all hole cards that are represented by a single hole card grid cell.
            /// Each grid cell represents a different amount of combos:
            /// Pocket pair: 6 combos.
            /// Suited: 4 combos.
            /// Offsuit: 12 Combos.
            /// </summary>
            /// <param name="col">Zero based x coordinate in the grid where A is 0</param>
            /// <param name="row">Zero based y coordinate in the grid where A is 0</param>
            /// <returns>All combos represented by that grid cell</returns>
            internal static IEnumerable<ulong> GridCellBinary(int col, int row)
            {
                if(col < 0 || col > Hand.RankCount)
                    throw new ArgumentOutOfRangeException(nameof(col));
                if(row < 0 || row > Hand.RankCount)
                    throw new ArgumentOutOfRangeException(nameof(row));

                int rankA = col * Hand.SuitCount;
                int rankB = row * Hand.SuitCount;

                if(row == col) {
                    // pair
                    return AllPocketPairs.Select(pp => pp << rankA);

                } else if(row > col) {
                    // offsuit
                    return AllSingleCards.SelectMany(ocA => AllSingleCards
                                                                 .Where(ocB => ocA != ocB)
                                                                 .Select(ocB => (ocA << rankA) | (ocB << rankB)));
                } else {
                    // suited
                    return AllSingleCards.Select(sc => (sc << rankA) | (sc << rankB));
                }
            }

            /// <summary>
            /// Iterate over all hole cards that are represented by a single hole card grid cell.
            /// Each grid cell represents a different amount of combos:
            /// Pocket pair: 6 combos.
            /// Suited: 4 combos.
            /// Offsuit: 12 Combos.
            /// </summary>
            /// <param name="col">Zero based x coordinate in the grid where A is 0</param>
            /// <param name="row">Zero based y coordinate in the grid where A is 0</param>
            /// <returns>All combos represented by that grid cell</returns>
            public static IEnumerable<HoleCards> GridCell(int col, int row) =>
                GridCellBinary(col, row).Select(hc => new HoleCards(hc));

            #endregion

            #region Pocket pairs

            private static readonly HoleCards[] _pocketPairs = new ulong[]
            {
                3,
                5,
                6,
                9,
                10,
                12,
                48,
                80,
                96,
                144,
                160,
                192,
                768,
                1280,
                1536,
                2304,
                2560,
                3072,
                12288,
                20480,
                24576,
                36864,
                40960,
                49152,
                196608,
                327680,
                393216,
                589824,
                655360,
                786432,
                3145728,
                5242880,
                6291456,
                9437184,
                10485760,
                12582912,
                50331648,
                83886080,
                100663296,
                150994944,
                167772160,
                201326592,
                805306368,
                1342177280,
                1610612736,
                2415919104,
                2684354560,
                3221225472,
                12884901888,
                21474836480,
                25769803776,
                38654705664,
                42949672960,
                51539607552,
                206158430208,
                343597383680,
                412316860416,
                618475290624,
                687194767360,
                824633720832,
                3298534883328,
                5497558138880,
                6597069766656,
                9895604649984,
                10995116277760,
                13194139533312,
                52776558133248,
                87960930222080,
                105553116266496,
                158329674399744,
                175921860444160,
                211106232532992,
                844424930131968,
                1407374883553280,
                1688849860263936,
                2533274790395904,
                2814749767106560,
                3377699720527872
            }.Select(pp => new HoleCards(pp)).ToArray();

            public static IEnumerable<HoleCards> PocketPairs => _pocketPairs;

            #endregion

            #region Broadways

            private static readonly HoleCards[] _broadWays = new ulong[]
            {
                196608,
                327680,
                393216,
                589824,
                655360,
                786432,
                65537,
                131074,
                262148,
                524296,
                131073,
                262145,
                524289,
                65538,
                262146,
                524290,
                65540,
                131076,
                524292,
                65544,
                131080,
                262152,
                65552,
                131104,
                262208,
                524416,
                131088,
                262160,
                524304,
                65568,
                262176,
                524320,
                65600,
                131136,
                524352,
                65664,
                131200,
                262272,
                65792,
                131584,
                263168,
                526336,
                131328,
                262400,
                524544,
                66048,
                262656,
                524800,
                66560,
                132096,
                525312,
                67584,
                133120,
                264192,
                69632,
                139264,
                278528,
                557056,
                135168,
                266240,
                528384,
                73728,
                270336,
                532480,
                81920,
                147456,
                540672,
                98304,
                163840,
                294912,
                3,
                5,
                6,
                9,
                10,
                12,
                48,
                80,
                96,
                144,
                160,
                192,
                768,
                1280,
                1536,
                2304,
                2560,
                3072,
                12288,
                20480,
                24576,
                36864,
                40960,
                49152,
                17,
                34,
                68,
                136,
                257,
                514,
                1028,
                2056,
                4097,
                8194,
                16388,
                32776,
                33,
                65,
                129,
                18,
                66,
                130,
                20,
                36,
                132,
                24,
                40,
                72,
                513,
                1025,
                2049,
                258,
                1026,
                2050,
                260,
                516,
                2052,
                264,
                520,
                1032,
                8193,
                16385,
                32769,
                4098,
                16386,
                32770,
                4100,
                8196,
                32772,
                4104,
                8200,
                16392,
                272,
                544,
                1088,
                2176,
                4112,
                8224,
                16448,
                32896,
                528,
                1040,
                2064,
                288,
                1056,
                2080,
                320,
                576,
                2112,
                384,
                640,
                1152,
                8208,
                16400,
                32784,
                4128,
                16416,
                32800,
                4160,
                8256,
                32832,
                4224,
                8320,
                16512,
                4352,
                8704,
                17408,
                34816,
                8448,
                16640,
                33024,
                4608,
                16896,
                33280,
                5120,
                9216,
                33792,
                6144,
                10240,
                18432
            }.Select(pp => new HoleCards(pp)).ToArray();

            public static IEnumerable<HoleCards> BroadWays => _broadWays;

            #endregion

            // Todo: suited connector and suited gapper enumerations
        }
    }
}
