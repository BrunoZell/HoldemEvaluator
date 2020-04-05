using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    public partial class Board
    {
        public static class Enum
        {
            /// <summary>
            /// Get all possible boards on a specific street
            /// </summary>
            /// <param name="progress">The street of the board</param>
            /// <param name="deadCards">All boards containing one or more cards from this parameter will be excluded</param>
            /// <returns>A list of boards according to the parameters</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<Board> All(Progression progress, CardCollection deadCards = null)
            {
                return All((int)progress, deadCards).Select(cc => new Board(cc));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IEnumerable<ulong> All(int progress, ulong deadCards = 0UL)
            {
                return CardCollection.Enum.All(progress, deadCards);
            }

            /// <summary>
            /// Get all possible boards on a specific street and specific included cards
            /// </summary>
            /// <param name="progress">The street of the board</param>
            /// <param name="includedCards">Every board must contain all of these cards</param>
            /// <param name="deadCards">All boards containing one or more cards from this parameter will be excluded</param>
            /// <returns>A list of boards according to the parameters</returns>
            public static IEnumerable<Board> Include(Progression progress, CardCollection includedCards, CardCollection deadCards = null)
            {
#if DEBUG
                if(includedCards.Count > (int)progress)
                    throw new ArgumentException("There can't be more cards included than are dealt on the current progression of the board.", nameof(includedCards));
#endif

                return Include((int)progress, includedCards, deadCards).Select(cc => new Board(cc));
            }

            internal static IEnumerable<ulong> Include(int progress, ulong includedCards, ulong deadCards = 0UL)
            {
#if DEBUG
                if(Hand.Bin.GetCardCount(includedCards) > progress)
                    throw new ArgumentException("There can't be more cards included than are dealt on the current progression of the board.", nameof(includedCards));
#endif
                return CardCollection.Enum.Include(progress, includedCards, deadCards);
            }
        }
    }
}
