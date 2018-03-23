using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class LectureParser : IParser
    {
        public ParsedSubject Parse(string input)
        {
            return new ParsedSubject();
        }
    }
}
