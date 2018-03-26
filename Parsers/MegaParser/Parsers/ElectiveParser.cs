using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class ElectiveParser : IParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            var parsedSubject = new ParsedSubject
            {
                SubjectName = "Курс по выбору",
                Time = input.Time,
                Group = input.Group
            };
            return parsedSubject;
        }
    }
}
