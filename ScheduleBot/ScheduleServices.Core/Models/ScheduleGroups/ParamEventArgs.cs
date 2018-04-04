using System;
using System.Collections.Generic;
using System.Text;

namespace ScheduleServices.Core.Models.ScheduleGroups
{
    public class ParamEventArgs<T> : EventArgs
    {
        public T Param { get; set; }
    }
}
