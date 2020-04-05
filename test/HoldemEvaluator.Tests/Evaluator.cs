using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HoldemEvaluator.Tests
{
    public class Evaluator
    {
        [Theory]
        [InlineData("AhKh", "AsKs")]
        [InlineData("Jh2c", "Jd2s")]
        [InlineData("6c3s", "3c6s")]
        [InlineData("As3h", "Ad3s")]
        [InlineData("AdJh", "AsJc")]
        [InlineData("4h5h", "4s5d")]
        [InlineData("5h2s", "5c2d")]
        public void Eval_EqualStrength(string hand1, string hand2)
        {
            var board = Board.Parse("Ac Js 7h 6h 3d");
            var hc1 = HoleCards.Parse(hand1);
            var hc2 = HoleCards.Parse(hand2);

            var results = board.Evaluate(hc1, hc2);

            Assert.Equal(results[0].HandStrength, results[1].HandStrength);
        }

        [Theory]
        [MemberData("TestData", MemberType = typeof(EvaluatorTestData))]
        public void Eval_Hand1Wins(string hand1, string hand2, string board)
        {
            var thisBoard = Board.Parse(board);
            var hc1 = HoleCards.Parse(hand1);
            var hc2 = HoleCards.Parse(hand2);

            var results = thisBoard.Evaluate(hc1, hc2);

            Assert.True(results[0].HandStrength > results[1].HandStrength);
        }
    }
}
