using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HoldemEvaluator.Hand.Eval;

namespace HoldemEvaluator
{
    public partial struct EvaluationResults
    {
        public struct PlayerResults
        {
            public PlayerResults(int postion, Holdings holding, uint handStrength, HoleCards holeCards)
            {
                Position = postion;
                Holding = holding;
                HandStrength = handStrength;
                HoleCards = holeCards;
            }

            /// <summary>
            /// The position at the table in the hand
            /// </summary>
            public int Position { get; }

            /// <summary>
            /// The holding type of this player
            /// </summary>
            public Holdings Holding { get; }

            /// <summary>
            /// The hand value of this player. The higher the value the better the holding
            /// </summary>
            public uint HandStrength { get; }

            /// <summary>
            /// The cards the player is holding in the hand
            /// </summary>
            public HoleCards HoleCards { get; }

            public override string ToString()
            {
                return $"{HoleCards}: {Holding} ({HandStrength})";
            }
        }
    }
}
