using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;
using ScheduleServices.Core.Modules.Interfaces;

namespace ScheduleServices.Core.Modules
{
    public class GroupsMonitor : IGroupsMonitor
    {
        //anyone can change any group from allGroups, but then it will be removed from this collection
        //and restored from backup
        private readonly ConcurrentDictionary<string, IScheduleGroup> allGroups =
            new ConcurrentDictionary<string, IScheduleGroup>();
        //no one should have access to change items from backup
        private readonly ConcurrentDictionary<string, IScheduleGroup> backup =
            new ConcurrentDictionary<string, IScheduleGroup>();

        
        public event EventHandler UpdatedEvent;
        public GroupsMonitor(IEnumerable<IScheduleGroup> groups)
        {
            foreach (var group in groups)
            {
                backup.AddOrUpdate(group.Name, group, (s, oldGroup) => group);
                var clone = (IScheduleGroup)group.Clone();
                clone.PropertyChanged += OnMainPropertyChanged;
                allGroups.AddOrUpdate(clone.Name, clone, (s, scheduleGroup) => clone);
            }

            
        }

        private void OnMainPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
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
                if (backup.TryGetValue(key, out IScheduleGroup original))
                {
                    var clone = (IScheduleGroup)original.Clone();
                    allGroups.AddOrUpdate(key, clone, (stringkey, badVal) => clone);
                }
                else
                {
                    //bad situation, try to hard restore
                    foreach (var diff in backup.Keys.Except(allGroups.Keys).ToList())
                    {
                        if (backup.ContainsKey(diff))
                        {
                            var clone = (IScheduleGroup)backup[diff].Clone();
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
            allGroups.Values.ToList();

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

        private async void OnScheduleServiceUpdated(object sender, EventArgs e)
        {
            // because this method is 'async void' using try-catch to not miss exception
            /*try
            {
                var groups = await scheduleServise.GetAvailableGroupsAsync();
                foreach (var group in groups)
                {
                    allGroups.AddOrUpdate(group.Name, group, (name, oldGroup) =>
                    {
                        oldGroup.GType = group.GType;
                        oldGroup.Name = group.Name;
                        return oldGroup;
                    });
                }
            }
            catch (Exception exception)
            {
                //todo: log
                Console.WriteLine(exception);
                throw;
            }*/
        }
    }
}