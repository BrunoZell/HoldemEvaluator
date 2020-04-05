using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HoldemEvaluator
{
    public partial struct EvaluationResults : IEnumerable<EvaluationResults.PlayerResults>
    {
        public EvaluationResults(IEnumerable<PlayerResults> results)
        {
            _playerResults = results.OrderByDescending(p => p.HandStrength).ToList();
        }

        /// <summary>
        /// A list of all player results. It's ordered so that the best player
        /// has the lowest index and the worst player has the highest index.
        /// </summary>
        private List<PlayerResults> _playerResults;

        /// <summary>
        /// Access the player with the best result
        /// </summary>
        public PlayerResults Winner => _playerResults.First();

        /// <summary>
        /// How many players are participating in the hand (which was evaluated)
        /// </summary>
        public int PlayerCount => _playerResults.Count;

        /// <summary>
        /// If one or more players have the same best hand strength
        /// </summary>
        public bool IsSplit {
            get {
                if (_playerResults.Count < 2)
                    return false;

                return _playerResults[0].HandStrength == _playerResults[1].HandStrength;
            }
        }

        /// <summary>
        /// Access a players result by his position at the table
        /// </summary>
        public PlayerResults this[int position] => _playerResults.First(p => p.Position == position);

        #region IEnumerable implementation
        public IEnumerator<PlayerResults> GetEnumerator() =>
            _playerResults.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
        #endregion
    }
}
