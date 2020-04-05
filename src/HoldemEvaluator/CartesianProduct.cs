using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEvaluator
{
    static class CartesianProducts
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sources)
        {
            return sources.Skip(1).Any() ?
                sources.Skip(1)
                       .CartesianProduct()
                       .SelectMany(cp => sources.First().Select(s => new[] { s }.Concat(cp))) :
                sources.First()
                       .Select(c => new[] { c });
        }
    }
}
