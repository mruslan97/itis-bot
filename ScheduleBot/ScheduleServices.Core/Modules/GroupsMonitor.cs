using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class GroupsMonitor : IGroupsMonitor
    {
        private readonly ILogger<GroupsMonitor> logger;

        //anyone can change any group from allGroups, but then it will be removed from this collection
        //and restored from backup
        private readonly ConcurrentDictionary<string, IScheduleGroup> allGroups =
            new ConcurrentDictionary<string, IScheduleGroup>();

        //no one should have access to change items from backup
        private readonly ConcurrentDictionary<string, IScheduleGroup> backup =
            new ConcurrentDictionary<string, IScheduleGroup>();

        private IEnumerable<ICompatibleGroupsRule> rules;

        public GroupsMonitor(IEnumerable<IScheduleGroup> groups, ILogger<GroupsMonitor> logger = null)
        {
            this.logger = logger;
            foreach (var group in groups)
            {
                backup.AddOrUpdate(group.Name, group, (s, oldGroup) => group);
                var clone = (IScheduleGroup) group.Clone();
                clone.PropertyChanged += OnMainPropertyChanged;
                allGroups.AddOrUpdate(clone.Name, clone, (s, scheduleGroup) => clone);
            }
        }

        public GroupsMonitor(IEnumerable<IScheduleGroup> groups, IEnumerable<ICompatibleGroupsRule> rules, ILogger<GroupsMonitor> logger = null) :
            this(groups, logger)
        {
            this.rules = rules;
        }

        private void OnMainPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            logger?.LogWarning("Groups monitor found group property changed: {0}, {1}",
                JsonConvert.SerializeObject(sender), JsonConvert.SerializeObject(propertyChangedEventArgs));
            var cloneFromMain = (IScheduleGroup) sender;
            if (propertyChangedEventArgs.PropertyName == "Name")
            {
                var badPair = allGroups.FirstOrDefault(pair => pair.Key != pair.Value.Name);
                RestoreByKey(badPair.Key);
            }
            else
            {
                RestoreByKey(cloneFromMain.Name);
            }

            //unsubscribe
            cloneFromMain.PropertyChanged -= OnMainPropertyChanged;

            void RestoreByKey(string key)
            {
                logger?.LogWarning("Groups monitor restoring group from backup: {0}", key);
                if (backup.TryGetValue(key, out IScheduleGroup original))
                {
                    var clone = (IScheduleGroup) original.Clone();
                    allGroups.AddOrUpdate(key, clone, (stringkey, badVal) => clone);
                }
                else
                {
                    //bad situation, try to hard restore
                    logger?.LogWarning("Original not found: {0}", key);
                    foreach (var diff in backup.Keys.Except(allGroups.Keys).ToList())
                    {
                        logger?.LogWarning("Diff: {0}", diff);
                        if (backup.ContainsKey(diff))
                        {
                            
                            var clone = (IScheduleGroup) backup[diff].Clone();
                            clone.PropertyChanged += OnMainPropertyChanged;
                            allGroups.AddOrUpdate(key, clone, (stringkey, badVal) => clone);
                        }
                        else
                        {
                            allGroups.TryRemove(diff, out IScheduleGroup trash);
                            trash.PropertyChanged -= OnMainPropertyChanged;
                        }
                    }
                }
            }
        }


        public IEnumerable<IScheduleGroup> AvailableGroups =>
            allGroups.Values;

        public bool TryFindGroupByName(string name, out IScheduleGroup resultGroup)
        {
            return allGroups.TryGetValue(name, out resultGroup);
        }

        public IEnumerable<IScheduleGroup> GetAllowedGroups(ScheduleGroupType ofType, IScheduleGroup target)
        {
            return allGroups.Values.Where(g =>
                    g.GType == ofType && (rules?.Any(rule => rule.AreCompatible(target, g)) ?? true))
                .ToList();
        }

        public IEnumerable<IScheduleGroup> RemoveInvalidGroupsFrom(IEnumerable<IScheduleGroup> groups)
        {
            if (groups != null)
                return groups.Where(IsGroupPresent);
            throw new ArgumentNullException("groups");
        }

        public bool IsGroupPresent(IScheduleGroup group)
        {
            if (group != null)
                return backup.TryGetValue(group.Name, out IScheduleGroup value) && value.Equals(group);
            throw new ArgumentNullException("group");
        }

        public bool TryGetCorrectGroup(IScheduleGroup sample, out IScheduleGroup correct)
        {
            if (sample != null)
                return allGroups.TryGetValue(sample.Name, out correct);
            throw new ArgumentNullException("sample");
        }
    }
}