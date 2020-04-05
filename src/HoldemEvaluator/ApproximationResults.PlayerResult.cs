using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial struct ApproximationResults
    {
        public struct PlayerResults
        {
            public PlayerResults(int postion, float equity, HoleCards holeCards)
            {
                Position = postion;
                Equity = equity;
                HoleCards = holeCards;
            }

            /// <summary>
            /// The position at the table in the hand
            /// </summary>
            public int Position { get; }

            /// <summary>
            /// The chance of winning in relation to all other players in the hand in percent
            /// </summary>
            public float Equity { get; }

            /// <summary>
            /// The cards the player is holding in the hand
            /// </summary>
            public HoleCards HoleCards { get; }

            public override string ToString()
            {
                return $"{HoleCards}: {Equity.ToString("0.00 %")}";
            }
        }
    }
}
