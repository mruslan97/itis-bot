using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Modules
{
    public class ScheduleConstructorException : ArgumentOutOfRangeException
    {
        public ScheduleConstructorException(string paramName) : base(paramName)
        {
        }
    }
}
