using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class PhysCultureParser : IParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            switch (input.Time)
            {
                case "08.30-10.00":
                    input.Time = "8.00-9.30";
                    break;
                case "10.10-11.40":
                    input.Time = "10.05-11.35";
                    break;
                case "11.50-13.20":
                    input.Time = "12.00-13.30";
                    break;
                case "15.20-16.50":
                    input.Time = "16.00-17.30";
                    break;
            }
            var parsedSubject = new ParsedSubject
            {
                Cabinet = "",
                Notation = "",
                SubjectName = "Физическая культура",
                Time = input.Time,
                Group = input.Group
            };
            return parsedSubject;
        }
    }
}
