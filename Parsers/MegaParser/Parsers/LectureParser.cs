using MegaParser.Models;

namespace MegaParser.Parsers
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
                Time = input.Time
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
                    if (cabCount == 1 && c.Equals('3') == false) cabCount++;
                    parsedSubject.Cabinet += c;
                    cabCount++;
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