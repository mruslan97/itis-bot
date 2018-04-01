using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleServices.Core.Extensions
{
    public  static class EnumerableExtenions
    {
        public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.GroupJoin(inner, outerKeySelector,
                    innerKeySelector,
                    (@out, @in) => new {Out = @out, In = @in})
                .SelectMany(xy => xy.In.DefaultIfEmpty(), (x, y) => resultSelector(x.Out, y));
        }
    }
}
