using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class EnglishParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            var parsedSubject = new ParsedSubject
            {
                SubjectName = "Иностранный язык",
                Time = input.Time,
                Group = input.Group
            };
            return parsedSubject;
        }
    }
}
