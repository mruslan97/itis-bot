using System;
using System.Collections.Generic;
using System.Text;
using MagicParser.Models;

namespace MagicParser.Parsers
{
    public class LectureParser : IParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            int i = 0, fmCheck = 0, cabCount = 0;
            bool upCheck = false, notationCheck = false;
            var parsedSubject = new ParsedSubject
            {
                Cabinet = "",
                Notation = "",
                SubjectName = "",
                Teacher = "",
                Time = input.Time,
                Group = input.Group
            };

            foreach (var c in input.Content)
            {
                i++;
                if (c.Equals('(')) notationCheck = true;
                if (i > 1)
                    if (char.IsUpper(c))
                        upCheck = true;
                if (char.IsNumber(c) && cabCount < 4 && notationCheck == false)
                {
                    #region tin

                    if (cabCount == 1 && !c.Equals('3')) cabCount++;
                    parsedSubject.Cabinet += c;
                    if (parsedSubject.Cabinet.Equals("140"))
                        cabCount--;
                    if (parsedSubject.Cabinet.Equals("115"))
                        parsedSubject.Cabinet = "1508";
                    cabCount++;

                    #endregion
                }
                else
                {
                    if (!upCheck)
                    {
                        parsedSubject.SubjectName += c;
                    }
                    else if (notationCheck)
                    {
                        parsedSubject.Notation += c;
                    }
                    else if (fmCheck < 3)
                    {
                        parsedSubject.Teacher += c;
                        if (char.IsUpper(c))
                            fmCheck++;
                    }
                }

                if (c.Equals(')'))
                    notationCheck = false;
            }

            return parsedSubject;
        }
    }
}
