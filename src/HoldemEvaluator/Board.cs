using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class Board : IEnumerable<Card>
    {
        public enum Progression
        {
            Preflop,
            Flop = 3,
            Turn,
            River
        }

        #region Constructors

        public Board()
        {
            _cards = new CardCollection();
        }

        /// <summary>
        /// Creates a board from a collection of cards.
        /// </summary>
        /// <param name="collection">The collection must hold either 0, 3, 4 or 5 cards that represent the board</param>
        public Board(CardCollection collection)
        {
#if DEBUG
            if(!System.Enum.GetValues(typeof(Progression)).Cast<int>().Contains(collection.Count))
                throw new ArgumentException("The collection must hold either 0, 3, 4 or 5 cards that represent the board", nameof(collection));
#endif

            _cards = new CardCollection(collection);
        }

        /// <summary>
        /// Creates a board from a collection of cards.
        /// </summary>
        /// <param name="collection">The collection must hold either 0, 3, 4 or 5 cards that represent the board</param>
        internal Board(ulong collection)
        {
            _cards = collection;
#if DEBUG
            // Low performance
            if(!System.Enum.GetValues(typeof(Progression)).Cast<int>().Contains(_cards.Count))
                throw new ArgumentException("The collection must hold either 0, 3, 4 or 5 cards that represent the board", nameof(collection));
#endif
        }

        /// <summary>
        /// Clones a board
        /// </summary>
        public Board(Board board)
        {
            if(board != null)
                _cards = new CardCollection(board._cards);
        }

        /// <summary>
        /// Parses a board from a string. Same syntax as a card collection
        /// </summary>
        public static Board Parse(string boardString)
        {
            return Notation.Parse(boardString);
        }

        /// <summary>
        /// Creates a random board.
        /// </summary>
        /// <param name="progress">The progression of the board. Specifies how many cards are on the board.</param>
        /// <param name="deadCards">Dead cards. Specifies all cards that can't be contained on the board such as hole cards or boxed cards.</param>
        /// <returns>A random board according to the parameters</returns>
        public static Board Random(Progression progress = Progression.River, CardCollection deadCards = null)
        {
            return new Board(CardCollection.RandomAsUlong((int)progress, deadCards));
        }

        #endregion

        /// <summary>
        /// The collection which holds all cards on the board
        /// </summary>
        private CardCollection _cards;

        /// <summary>
        /// On which street the board currently is
        /// </summary>
        public Progression Progress {
            get {
                return (Progression)_cards.Count;
            }
        }

        #region Evaluation and approximation

        /// <summary>
        /// Evaluate the holdings for all players on a completed board (river)
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the holdings for</param>
        /// <returns>A EvaluationResult-struct which holds all evaluation results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EvaluationResults Evaluate(params HoleCards[] holeCards)
        {
            return Evaluate(_cards.Binary, Array.ConvertAll(holeCards, hc => (ulong)hc));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EvaluationResults Evaluate(params ulong[] holeCards)
        {
            return Evaluate(_cards.Binary, holeCards);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static EvaluationResults Evaluate(ulong board, params ulong[] holeCards)
        {
#if DEBUG
            if(holeCards.Any(hc => (board & hc) != 0UL))
                throw new ArgumentException("All hole cards of the players should not be contained in the board", nameof(holeCards));

            if(Hand.Bin.GetCardCount(board) != (int)Progression.River)
                throw new NashException("Evaluation only on completed boards allowed. For an approximation use the appropriate method.");
#endif
            return new EvaluationResults(GetPlayerEvalResults(board, holeCards));
        }

        /// <summary>
        /// Actually evaluating the holding for each player
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the holdings for</param>
        /// <returns>A list of player evaluation results</returns>
        private static IEnumerable<EvaluationResults.PlayerResults> GetPlayerEvalResults(ulong board, ulong[] holeCards)
        {
#if DEBUG
            ulong allHoleCards = 0;
            for (int i = 0; i < holeCards.Length; i++) {
                if ((allHoleCards & holeCards[i]) != 0)
                    throw new ArgumentException("There are duplicates in the hole cards", nameof(holeCards));
                allHoleCards |= holeCards[i];
            }
#endif
            for(int i = 0; i < holeCards.Length; i++) {
                ulong boardCards = board | holeCards[i];
                // Todo: add hand description (with good performance). Could be optional to save time if not needed
                yield return new EvaluationResults.PlayerResults(i, /*Hand.Eval.GetHoldingType(boardCards)*/0, Hand.Eval.GetHandStrength(boardCards), holeCards[i]);
            }
        }

        /// <summary>
        /// Calculates equity for two or more hole cards on the board by checking a few random outcomes. No splits considered
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the equity for</param>
        /// <returns>A list of player approximation results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ApproximationResults Approximate(params HoleCards[] holeCards)
        {
            return Approximate(Array.ConvertAll(holeCards, hc => (ulong)hc));
        }

        /// <summary>
        /// Calculates equity for two or more hole cards on the board by checking a few random outcomes. No splits considered
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the equity for</param>
        /// <returns>A list of player approximation results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ApproximationResults Approximate(CardCollection deadCards, params HoleCards[] holeCards)
        {
            return Approximate(Array.ConvertAll(holeCards, hc => (ulong)hc), deadCards);
        }


        public ApproximationResults Approximate(CardCollection deadCards, params Range[] ranges)
        {
            var allApprox = ranges.Select(r => r.SelectedHoleCardBinaries
                                     .Where(b => !_cards.ContainsSome(b) && (deadCards == null || !deadCards.ContainsSome(b))))
                             .CartesianProduct()
                             .Select(s => Approximate(s.ToArray(), deadCards, trials: 100))
                             .ToList();

            var players = new List<float>(new float[ranges.Length]);
            float splitProb = 0f;
            foreach(var approx in allApprox) {
                splitProb += approx.SplitProbability;
                for(int p = 0; p < ranges.Length; p++) {
                    players[p] += approx[p].Equity;
                }
            }
            players.ForEach(p => p /= allApprox.Count);
            splitProb /= allApprox.Count;

            return new ApproximationResults(players.Select((eq, i) => new ApproximationResults.PlayerResults(i, eq, 3UL)), splitProb); // Todo: add ranges to result
        }

        public void ApproximateLive(Action<IEnumerable<float>, float> updateLiveResults, CardCollection deadCards, CancellationToken? cancellationToken, params Range[] ranges)
        {
            var rnd = new Random();
            var nativeRanges = ranges.Select(r => r.SelectedHoleCardBinaries.Where(b => !_cards.ContainsSome(b) && (!deadCards?.ContainsSome(b) ?? true)).ToList()).ToList();

            cancellationToken?.ThrowIfCancellationRequested();


            var localHoleCards = new ulong[nativeRanges.Count];
            ulong allLocalHoleCards;

            var totalEquity = new float[nativeRanges.Count];
            float splitEquity = 0f;

            int firstSelectedPlayer = 0;
            int skippedSituations = 0;
            bool skipSituation = false;

            for (int i = 0; i < Int32.MaxValue; i++) {

                // Stop if requested
                cancellationToken?.ThrowIfCancellationRequested();

                allLocalHoleCards = 0;
                // Select random hole cards from each player to test
                // Do not select from the same player first every time,
                // the equities can be affected by this (e.g. AA-KK vs AA-KK vs AA-KK)
                int j = firstSelectedPlayer;
                do {
                    var validHoleCards = nativeRanges[j].Where(hc => (hc & allLocalHoleCards) == 0).ToList();

                    if (!validHoleCards.Any()) {
                        if (skippedSituations > (i < 1000 ? 950 : i * 0.95f))
                            throw new NashException("Ranges are too narrow. Please adjust.");
                        skipSituation = true;
                        break;
                    }

                    localHoleCards[j] = validHoleCards[rnd.Next(validHoleCards.Count)];
                    allLocalHoleCards |= localHoleCards[j];

                    // if the last player is reached wrap around to the first one
                    if (++j >= nativeRanges.Count)
                        j = 0;
                } while (j != firstSelectedPlayer);

                // skip this situation if not all hole cards were chosen
                // try with same first player to not influence the results
                if (skipSituation) {
                    skippedSituations++;
                    skipSituation = false;
                    continue;
                }

                // update first player for next run
                if (++firstSelectedPlayer >= nativeRanges.Count)
                    firstSelectedPlayer = 0;

                // Add to total equity
                var results = Approximate(localHoleCards, deadCards, trials: 1000);
                for (int k = 0; k < nativeRanges.Count; k++) {
                    totalEquity[k] += results[k].Equity;
                }
                splitEquity += results.SplitProbability;

                // Update live results
                if (i % 10 == 0) {
                    int simulationCount = (i - skippedSituations + 1);
                    updateLiveResults?.Invoke(totalEquity.Select(eq => Math.Min(eq / simulationCount, 1f)), Math.Min(splitEquity / simulationCount, 1f));
                }
            }
        }

        /// <param name="deadCardsCount">If the count of the dead cards is already calculated it can be passed in this function to save the calculation. If not just leave the default value.</param>
        /// <param name="trials">How many random boards should be dealt. If this number is higher than the amount of possible distinct boards it is ignored.</param>
        internal ApproximationResults Approximate(ulong[] holeCards, ulong deadCards = 0UL, int? deadCardsCount = null, int? trials = null)
        {
            int appoxAmt = trials ?? 8000 * holeCards.Length;
            // If there are less possible outcomes than we want to test we calculate just those.
            if(appoxAmt > CardCollection.Enum.CombinationCount(Hand.TotalCards - (deadCardsCount ?? Hand.Bin.GetCardCount(deadCards)) - _cards.Count, (int)Progression.River - _cards.Count)) {
                return ApproximateExact(deadCards, holeCards);
            }

            // include hole cards into dead cards
            Array.ForEach(holeCards, hc => deadCards |= hc);

            var winnings = new int[holeCards.Length];
            int splits = 0;
            var board = new Board(_cards);
            for(int i = 0; i < appoxAmt; i++) {
                // Create a virtual board
                board._cards.Binary = _cards.Binary;
                if(board.Progress != Progression.River) {
                    // deal random cards to the river
                    board.DealRandomCards(Progression.River, deadCards);
                }
                var eval = board.Evaluate(holeCards);
                if(eval.IsSplit) {
                    splits++;
                } else {
                    winnings[eval.Winner.Position]++;
                }
            }
            return new ApproximationResults(holeCards.Select((hc, i) => new ApproximationResults.PlayerResults(i, (float)winnings[i] / appoxAmt, hc)), (float)splits / appoxAmt);
        }

        /// <summary>
        /// Calculates equity for two or more hole cards on the board by checking all possible outcomes. No splits considered
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the equity for</param>
        /// <returns>A list of player approximation results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ApproximationResults ApproximateExact(params HoleCards[] holeCards)
        {
            return ApproximateExact(0UL, Array.ConvertAll(holeCards, hc => (ulong)hc));
        }

        /// <summary>
        /// Calculates equity for two or more hole cards on the board by checking all possible outcomes. No splits considered
        /// </summary>
        /// <param name="holeCards">The list of hole cards to evaluate the equity for</param>
        /// <returns>A list of player approximation results</returns>
        public ApproximationResults ApproximateExact(CardCollection deadCards, params HoleCards[] holeCards)
        {
            return ApproximateExact(deadCards, Array.ConvertAll(holeCards, hc => (ulong)hc));
        }

        internal ApproximationResults ApproximateExact(ulong deadCards, params ulong[] holeCards)
        {
            var winnings = new int[holeCards.Length];
            int splits = 0;

            if(Progress == Progression.River) {
                // Only one board possible, because all five cards are given
                var eval = Evaluate(holeCards);
                if(eval.IsSplit) {
                    splits++;
                } else {
                    winnings[eval.Winner.Position]++;
                }
                return new ApproximationResults(holeCards.Select((hc, i) => new ApproximationResults.PlayerResults(i, winnings[i], hc)), splits);
            } else {
                // include hole cards into dead cards
                Array.ForEach(holeCards, hc => deadCards |= hc);

                // iterate over all possible boards
                var boards = Enum.Include((int)Progression.River, _cards.Binary, deadCards).ToList();
                //Parallel.ForEach(boards, (board) => winnings[Evaluate(board, holeCards).Winner.Position]++);

                foreach(ulong board in boards) {
                    var eval = Evaluate(board, holeCards);
                    if (eval.IsSplit) {
                        splits++;
                    } else {
                        winnings[eval.Winner.Position]++;
                    }
                }
                return new ApproximationResults(holeCards.Select((hc, i) => new ApproximationResults.PlayerResults(i, (float)winnings[i] / boards.Count, hc)), (float)splits / boards.Count);
            }
        }

        #endregion

        /// <summary>
        /// Adds as many random cards to the board as needed to get to the desired state
        /// </summary>
        /// <param name="targetState">The street the board should be after dealing the cards</param>
        /// <param name="deadCards">Cards that cannot be dealt</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DealRandomCards(Progression targetState, CardCollection deadCards = null)
        {
            if(Progress >= targetState)
                return;

            DealRandomCards(targetState, (ulong)deadCards);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DealRandomCards(Progression targetState, ulong deadCards = 0UL)
        {
            _cards.Include(CardCollection.RandomAsUlong(targetState - Progress, deadCards | _cards.Binary));
        }

        #region Native overloads

        public static bool operator ==(Board board1, Board board2)
        {
            if(ReferenceEquals(board1, null)) {
                return ReferenceEquals(board2, null);
            }
            return board1.Equals(board2);
        }

        public static bool operator !=(Board board1, Board board2)
        {
            if(ReferenceEquals(board1, null)) {
                return !ReferenceEquals(board2, null);
            }
            return !board1.Equals(board2);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Board && ((Board)obj)._cards == _cards;
        }

        public override int GetHashCode()
        {
            return _cards.GetHashCode();
        }

        public override string ToString()
        {
            return Notation.GetNotation(this);
        }

        #endregion

        #region IEnumerable

        public IEnumerator<Card> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        #endregion
    }
}
