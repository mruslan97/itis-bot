using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using ScheduleBot.AspHost.BotServices.Interfaces;
using ScheduleServices.Core;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Models.ScheduleElems;
using ScheduleServices.Core.Modules;

namespace ScheduleBot.AspHost.Updating
{
    public class UpdateTeachersListJob : IJob, IDynamicElemVisitor, ITeachersSource
    {
        private readonly IScheduleService service;
        private readonly ILogger<UpdateTeachersListJob> logger;
        private List<string> teachersNames = new List<string>();
        private HashSet<string> foundNames;
        private byte ticks = 0;
        public UpdateTeachersListJob(IScheduleService service, ILogger<UpdateTeachersListJob> logger = null)
        {
            this.service = service;
            this.logger = logger;
        }

        public void VisitElem(IScheduleElem elem)
        {
            dynamic @dynamic = elem;
            Visit(@dynamic);
        }

        protected void Visit(Undefined elem)
        {
            logger?.LogWarning($"Undefind type found while visiting IScheduleElems: {elem.GetType().FullName}");
        }

        protected  void Visit(Lesson elem)
        {
            if (!String.IsNullOrWhiteSpace(elem?.Teacher) && elem.Teacher.Trim().Length > 2)
            {
                var firstSpace = elem.Teacher.Trim().IndexOf(' ');
                foundNames.Add(elem.Teacher.TrimStart().Substring(0, firstSpace > 0 ? firstSpace : elem.Teacher.TrimStart().Length).TrimEnd());
            }
                
        }

        protected void Visit(Week elem)
        {
            Visit((IScheduleElem)elem);
        }

        protected void Visit(Day elem)
        {
            Visit((IScheduleElem)elem);
        }

        protected void Visit(IScheduleElem elem)
        {
            if (elem?.Elems != null)
                foreach (var scheduleElem in elem.Elems)
                    VisitElem(scheduleElem);
        }

        public async void Execute()
        {
            if (ticks % 6 == 0)
            {
                ticks = 0;
                try
                {
                    foundNames = new HashSet<string>();
                    await service.RunVisitorThrougthStorage(this);
                    lock (teachersNames)
                    {
                        teachersNames = foundNames.OrderBy(s => s).ToList();
                    }
                    foundNames = null;
                }
                catch (Exception e)
                {
                    logger?.LogError(e, "Exc");
                }
            }
            ticks++;

        }

        public IList<string> GetTeachersNames()
        {
            lock (teachersNames)
            {
                return teachersNames;
            }
        }
    }
}