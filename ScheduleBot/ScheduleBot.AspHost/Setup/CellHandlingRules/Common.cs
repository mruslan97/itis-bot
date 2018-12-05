using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using TableRules.Core;

namespace ScheduleBot.AspHost.Setup
{
    public partial class CellHandlingRules
    {
        public static IEnumerable<(IScheduleElem, IScheduleGroup)> PrepareLectureOrSeminar(Lesson lesson,
            TableContext context,
            IEnumerable<IScheduleGroup> groupsForLecture)
        {
            if (lesson.Place.StartsWith("108") || lesson.Place.StartsWith("109"))
                return groupsForLecture.Select(g => new ValueTuple<IScheduleElem, IScheduleGroup>(lesson, g));
            else
                return Enumerable.Repeat(new ValueTuple<IScheduleElem, IScheduleGroup>(lesson,
                        groupsForLecture.FirstOrDefault(group =>
                            @group.Name.Contains(context.CurrentGroupLabel,
                                StringComparison.InvariantCultureIgnoreCase)))
                    , 1);
        }
    }
}