using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Modules
{
    public class ScheduleConstructorException : ArgumentOutOfRangeException
    {
        public ScheduleConstructorException(string message) : base(message)
        {
        }
        public ScheduleConstructorException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}
