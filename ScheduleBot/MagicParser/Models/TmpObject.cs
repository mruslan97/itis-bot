﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MagicParser.Models
{
    public class TmpObject
    {
        public string Time { get; set; }
        public string Content { get; set; }
        public string Group { get; set; }
        public int Course { get; set; }
        public bool? IsOnEvenWeek { get; set; }
    }
}
