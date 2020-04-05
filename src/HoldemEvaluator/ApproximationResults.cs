using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HoldemEvaluator
{
    public partial struct ApproximationResults : IEnumerable<ApproximationResults.PlayerResults>
    {
        public ApproximationResults(IEnumerable<PlayerResults> results, float splitProbability)
        {
            _playerResults = results.ToList();
            SplitProbability = splitProbability;
        }

        /// <summary>
        /// A list of all players hand approximations.
        /// The order is the same as preflop play (or as entered
        /// in the constructor)
        /// </summary>
        private List<PlayerResults> _playerResults;

        /// <summary>
        /// How many players are participating in the hand (which was approximated)
        /// </summary>
        public int PlayerCount {
            get {
                return _playerResults.Count;
            }
        }

        /// <summary>
        /// The probability of a split pot
        /// </summary>
        public float SplitProbability { get; private set; }

        /// <summary>
        /// Access a players approximation result by his position at the table
        /// </summary>
        public PlayerResults this[int position] => _playerResults[position];

        #region IEnumerable implementation
        public IEnumerator<PlayerResults> GetEnumerator() => _playerResults.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
