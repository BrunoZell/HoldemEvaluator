using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class Range
    {
        #region Constructors

        /// <summary>
        /// Create an empty range
        /// </summary>
        public Range()
        {
            _holeCards = new HashSet<ulong>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="range">Range to copy from</param>
        public Range(Range range)
        {
            _holeCards = new HashSet<ulong>(range._holeCards);
        }

        /// <summary>
        /// Create a range from a list of hole card combos
        /// </summary>
        public Range(IEnumerable<HoleCards> holeCards) :
            this(holeCards.Select(hc => hc.Binary))
        {
        }

        /// <summary>
        /// Creates a range with only one specific combo selected.
        /// Useful for running approximations with other more detailed ranges.
        /// </summary>
        /// <param name="onlyHoleCards"></param>
        public Range(HoleCards onlyHoleCards) :
            this()
        {
            _holeCards.Add(onlyHoleCards);
        }

        /// <summary>
        /// Create a range from a list of hole card combos
        /// </summary>
        internal Range(IEnumerable<ulong> holeCards)
        {
            _holeCards = new HashSet<ulong>(holeCards.Distinct());
        }

        /// <summary>
        /// Create a range with top x percent of cards selected.
        /// </summary>
        /// <param name="topPercent">How many hands should be selected in percent</param>
        /// <param name="playerAmount">How many players are at the table. Player amount changes the strength of hole card combos</param>
        public Range(float topPercent, int playerAmount)
        {
            if(topPercent < 0f || topPercent > 1f)
                throw new ArgumentOutOfRangeException(nameof(topPercent), topPercent, "Only percentages between zero and one are supported");

            _holeCards = new HashSet<ulong>();

            var orderingSet = HoleCards.Ranking.GetHandOrdering(playerAmount);

            float pPerCombo = 1f / 1326f; // The percent added per combo. Since there are 1326 total hole card combinations its 1 over 1326
            float p = 0f; // Currently added percent
            int i = 0; // Currently added chunks (from the data source)
            while(i < orderingSet.Length) {
                p += pPerCombo * orderingSet[i].Length;
                if(p > topPercent)
                    return;
                _holeCards.UnionWith(orderingSet[i++]);
            }
        }

        /// <summary>
        /// Create a range from hole card grid data.
        /// </summary>
        /// <param name="gridData">Array containing the hole card grid data. 1st dimension are the columns, containing 13 rows each</param>
        public Range(bool[][] gridData)
        {
            if (gridData == null || gridData.Length == 0)
                return;

            _holeCards = new HashSet<ulong>();

            // Resize if necessary
            if (gridData.Length > Hand.RankCount)
                Array.Resize(ref gridData, Hand.RankCount);

            // Handle inner collections (resize to Hand.FaceCount (13))
            for (int i = 0; i < gridData.Length; i++) {
                if (gridData[i] != null && gridData[i].Length != 0) {
                    var cellValues = gridData[i].Take(Hand.RankCount).ToArray();
                    for (int j = 0; j < cellValues.Length; j++) {
                        if (cellValues[j])
                            this[i, j] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Create a range from hole card grid data.
        /// </summary>
        /// <param name="gridData">Array containing the hole card grid data. 1st dimension are the columns, containing 13 rows each</param>
        public Range(bool[,] gridData)
        {
            if (gridData == null ||
                gridData.LongLength != Hand.RankCount * Hand.RankCount ||
                gridData.GetLength(0) != Hand.RankCount)
                return;

            _holeCards = new HashSet<ulong>();

            for (int col = 0; col < Hand.RankCount; col++) {
                for (int row = 0; row < Hand.RankCount; row++) {
                    if (gridData[col, row]) {
                        this[col, row] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Parse a range from a string representing it
        /// </summary>
        /// <param name="rangeString">String representing a range. E.g. "AK JJ+ A5s-ATs"</param>
        /// <returns>A Range representing the input</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Range Parse(string rangeString)
        {
            return Notation.Parse(rangeString);
        }

        #endregion

        /// <summary>
        /// Collection of all selected hole cards. Each entry has two bits set.
        /// </summary>
        private HashSet<ulong> _holeCards;

        /// <summary>
        /// Adds a collection of hole cards to the range
        /// </summary>
        /// <param name="holeCards"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddHoldeCards(IEnumerable<ulong> holeCards)
        {
            _holeCards.UnionWith(holeCards);
        }

        /// <summary>
        /// Adds a collection of hole cards to the range
        /// </summary>
        /// <param name="holeCards"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHoldeCards(IEnumerable<HoleCards> holeCards)
        {
            _holeCards.UnionWith(holeCards.Select(hc => hc.Binary));
        }

        /// <summary>
        /// The percentage of all possible hole cards included in this range
        /// </summary>
        public float Percentage {
            get {
                return _holeCards.Count / 1326f; // There are 1326 total hand combos
            }
        }

        #region Suit filters

        /// <summary>
        /// Bit flags for every suit combo
        /// </summary>
        [Flags]
        public enum SuitCombos
        {
            None = 0,
            All = 0xffff,

            SpadeSpade = 0x8000,
            SpadeHeart = 0x4000,
            SpadeClub = 0x2000,
            SpadeDiamond = 0x1000,
            HighSpadeOnly = 0xf000,
            LowSpadeOnly = 0x8888,

            HeartSpade = 0x0800,
            HeartHeart = 0x0400,
            HeartClub = 0x0200,
            HeartDiamond = 0x0100,
            HighHeartOnly = 0x0f00,
            LowHeartOnly = 0x4444,

            ClubSpade = 0x0080,
            ClubHeart = 0x0040,
            ClubClub = 0x0020,
            ClubDiamond = 0x0010,
            HighClubOnly = 0x00f0,
            LowClubOnly = 0x2222,

            DiamondSpade = 0x0008,
            DiamondHeart = 0x0004,
            DiamondClub = 0x0002,
            DiamondDiamond = 0x0001,
            HighDiamondOnly = 0x000f,
            LowDiamondOnly = 0x1111,
        }

        /// <summary>
        /// Generates the bit flag for a suit combo from two suits
        /// </summary>
        public static SuitCombos GetSuitCombo(int highCardSuit, int lowCardSuit)
        {
            return (SuitCombos)(1 << (highCardSuit * 4 + lowCardSuit));
        }

        /// <summary>
        /// Filters this range by specific valid suit groups and includes
        /// only those hole cards that match the filter.
        /// </summary>
        /// <param name="suitFilter">A 16-bit mask for all possible suit
        /// combinations to filter for</param>
        public void FilterBySuits(SuitCombos suitFilter)
        {
            var validSuitCombos = new List<(int high, int low)>();

            int filter = (int)suitFilter;
            for (int i = 0; i < 4; i++) {
                int row = filter & 0b1111;
                validSuitCombos.AddRange(Hand.Bin.GetAllBitIndices((ulong)row).Select(b => (i, b)));
                filter >>= 4;
            }

            _holeCards.IntersectWith(_holeCards
                .Select(hc => new HoleCards(hc))
                .Where(hc => validSuitCombos.Any(vc => vc.high == hc.HighCard.Suit && vc.low == hc.LowCard.Suit))
                .Select(hc => hc.Binary));
        }

        #endregion

        /// <summary>
        /// Returns an array containing the range represented as 13x13 hole card grid data.
        /// 1st dimension are the 13 columns, containing 13 rows each
        /// </summary>
        public bool[,] GetGridData()
        {
            var grid = new bool[Hand.RankCount, Hand.RankCount];

            for (int col = 0; col < Hand.RankCount; col++) {
                for (int row = 0; row < Hand.RankCount; row++) {
                    grid[col, row] = this[col, row, true];
                }
            }
            
            return grid;
        }

        /// <summary>
        /// Whether or not this range has a minimum of one combo included in a specific grid cell.
        /// </summary>
        /// <param name="col">Column of the cell to access in the hole card grid</param>
        /// <param name="row">Row of the cell to access in the hole card grid</param>
        public bool HasCardInGridCell(int col, int row)
        {
            return HoleCards.Enum.GridCellBinary(col, row)
                .Any(hc => _holeCards.Contains(hc));
        }

        /// <summary>
        /// How many combos this range has included in a specific grid cell.
        /// </summary>
        /// <param name="col">Column of the cell to access in the hole card grid</param>
        /// <param name="row">Row of the cell to access in the hole card grid</param>
        public int CountCardsInGridCell(int col, int row)
        {
            return HoleCards.Enum.GridCellBinary(col, row)
                .Count(hc => _holeCards.Contains(hc));
        }

        /// <summary>
        /// How many combos this range has included in a specific grid cell
        /// relative to the total possible amount of this cell.
        /// </summary>
        /// <param name="col">Column of the cell to access in the hole card grid</param>
        /// <param name="row">Row of the cell to access in the hole card grid</param>
        public float SelectedPercentageInGridCell(int col, int row)
        {
            var allCardsInCell = HoleCards.Enum.GridCellBinary(col, row);
            return (float)allCardsInCell.Where(hc => _holeCards.Contains(hc)).Count() / allCardsInCell.Count();
        }

        /// <summary>
        /// Get the range notation of the range
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Notation.GetRangeNotation(this);
        }

        #region Enumerations

        /// <summary>
        /// Iterates over all hole card combinations included in this range
        /// </summary>
        internal IEnumerable<ulong> SelectedHoleCardBinaries {
            get {
                return _holeCards;
            }
        }

        /// <summary>
        /// Iterates over all hole card combinations included in this range
        /// </summary>
        public IEnumerable<HoleCards> SelectedHoleCards {
            get {
                return _holeCards.Select(hc => new HoleCards(hc));
            }
        }
        #endregion

        #region this accessors

        /// <summary>
        /// Select all hole cards in a range or test if all hole cards in a range are selected.
        /// </summary>
        /// <param name="rangeNotation">A string representation of the range to test</param>
        public bool this[string rangeNotation] {
            get {
                return this[Notation.Parse(rangeNotation)];
            }
            set {
                this[Notation.Parse(rangeNotation)] = value;
            }
        }

        /// <summary>
        /// Select all hole cards in a range or test if all hole cards in a range are selected.
        /// </summary>
        /// <param name="range">The range to test</param>
        public bool this[Range range] {
            get {
                return range.SelectedHoleCardBinaries
                    .All(hc => _holeCards.Contains(hc));
            }
            set {
                foreach(ulong holeCard in range.SelectedHoleCardBinaries) {
                    this[holeCard] = value;
                }
            }
        }

        /// <summary>
        /// Select one hole card combination or test if it is selected
        /// </summary>
        /// <param name="holeCards">The hole cards to test</param>
        public bool this[HoleCards holeCards] {
            get {
                return this[holeCards.Binary];
            }
            set {
                this[holeCards.Binary] = value;
            }
        }

        /// <summary>
        /// Select one hole card combination or test if it is selected
        /// </summary>
        /// <param name="holeCards">The hole cards to test in a binary format</param>
        internal bool this[ulong holeCards] {
            get {
                return _holeCards.Contains(holeCards);
            }
            set {
                if(value) {
                    // set to true
                    if(!_holeCards.Contains(holeCards)) {
                        _holeCards.Add(holeCards);
                    }
                } else {
                    // set to false
                    if(_holeCards.Contains(holeCards)) {
                        _holeCards.Remove(holeCards);
                    }
                }
            }
        }

        /// <summary>
        /// Access the range in a hole card grid styled way
        /// </summary>
        /// <param name="col">Column of the cell to access in the hole card grid</param>
        /// <param name="row">Row of the cell to access in the hole card grid</param>
        /// <param name="any">If false, this method will return only true, if all combos
        /// of this grid cell are selected. If true, this method will return true if one
        /// or more combos of this grid cell are selected. This parameter is ignored
        /// when setting the property.</param>
        public bool this[int col, int row, bool any = false] {
            get {
                if (any) {
                    return HoleCards.Enum.GridCellBinary(col, row)
                        .Any(hc => _holeCards.Contains(hc));
                }
                return HoleCards.Enum.GridCellBinary(col, row)
                    .All(hc => _holeCards.Contains(hc));
            }
            set {
                foreach(ulong holeCards in HoleCards.Enum.GridCellBinary(col, row)) {
                    this[holeCards] = value;
                }
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (!(obj is Range r)) {
                return false;
            }
            return SelectedHoleCardBinaries.OrderBy(b => b).SequenceEqual(r.SelectedHoleCardBinaries.OrderBy(b => b));
        }

        public override int GetHashCode() {
            return -649190805 + EqualityComparer<HashSet<ulong>>.Default.GetHashCode(_holeCards);
        }
    }
}
