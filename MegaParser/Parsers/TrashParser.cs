﻿using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class TrashParser : IParser
    {
        public ParsedSubject Parse(string input)
        {
            return new ParsedSubject();
        }
    }
}
