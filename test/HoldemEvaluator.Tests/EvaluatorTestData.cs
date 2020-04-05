using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator.Tests
{
    public static class EvaluatorTestData
    {
        private static readonly List<string[]> _data = new List<string[]>
        {
            new string[] {"AhKh", "AsKs", "Jh9h8h7s2c"},
            new string[] {"AhAs", "KhKs", "9h7c6s3hTc"},
            new string[] {"Ts2s", "Th3h", "AhTc9h2c7s"},
            new string[] {"6h6c", "7h7c", "6s3h4hThJd"},
            new string[] {"7h7c", "6h6c", "6s7d4hThJd" },
            new string[] {"6h6c", "7h7c", "6s7d6dThJd"},
            new string[] {"6s6c", "7h8h", "6h7c6dThJh"},
            new string[] {"7h8h", "6s5c", "6h7c6dThJh"},
        };

        public static IEnumerable<object[]> TestData {
            get {
                return _data;
            }
        }
    }
}
