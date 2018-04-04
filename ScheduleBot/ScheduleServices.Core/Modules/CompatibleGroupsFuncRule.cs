using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class CompatibleGroupsFuncRule : ICompatibleGroupsRule
    {
        protected CompatibleGroupsFuncRule()
        {
        }
        public string Name { get; set; }
        protected Func<IScheduleGroup, IScheduleGroup, bool> CheckFunc;
        public CompatibleGroupsFuncRule(string name, Func<IScheduleGroup, IScheduleGroup, bool> checkFunc)
        {
            this.CheckFunc = checkFunc;
            Name = name;
        }

        public virtual bool AreCompatible(IScheduleGroup first, IScheduleGroup second)
        {
            return CheckFunc.Invoke(first, second);
        }
    }

    public class CompatibleGroupsFuncWithStoreRule<T> : CompatibleGroupsFuncRule
    {
        private readonly IEnumerable<T> storage;
        private readonly Func<IEnumerable<T>, IScheduleGroup, IScheduleGroup, bool> checkFuncWithStorage;

        public CompatibleGroupsFuncWithStoreRule(string name, IEnumerable<T> storage,  Func<IEnumerable<T>, IScheduleGroup,  IScheduleGroup, bool> checkFuncWithStorage)
        {
            this.storage = storage;
            this.checkFuncWithStorage = checkFuncWithStorage;
            Name = name;
        }
        public override bool AreCompatible(IScheduleGroup first, IScheduleGroup second)
        {
            return checkFuncWithStorage.Invoke(storage, first, second);
        }
    }
}
