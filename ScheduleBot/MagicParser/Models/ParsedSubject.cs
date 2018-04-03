﻿using System;
using System.Collections.Generic;
using System.Text;
using ScheduleServices.Core.Models.Interfaces;

namespace MagicParser.Models
{
    public class ParsedSubject
    {
        public string Group { get; set; }
        public string Time { get; set; }
        public string SubjectName { get; set; }
        public string Teacher { get; set; }
        public string Cabinet { get; set; }
        public string Notation { get; set; }
        public bool? IsOnEvenWeek { get; set; }
        public ScheduleGroupType Type { get; set; }
    }
}
