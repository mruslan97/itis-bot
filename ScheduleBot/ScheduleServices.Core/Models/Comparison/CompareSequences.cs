using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScheduleServices.Core.Models.Comparison
{
    public static class CompareSequencesExtensions
    {
        public static bool UnorderEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            //both same or both null
            if (ReferenceEquals(source, other)) return true;
            if (source != null && other != null)
            {
                //if (source.GetType() != other.GetType()) return false;
                if (source.Count() != other.Count())
                    return false;
                return source.All(other.Contains);
            }
            else
            {
                return false;
            }
            
        }
    }
}
