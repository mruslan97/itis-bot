using MegaParser.Models;

namespace MegaParser.Parsers
{
    public class SeminarParser : IParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            var initialsCounter = 0;
            var parsedSubject = new ParsedSubject
            {
                Cabinet = "",
                Notation = "",
                SubjectName = "",
                Teacher = "",
                Time = input.Time,
                Group = input.Group
            };
            var notationCheck = false;
            var upperCaseCheck = false;
            var i = 0;
            foreach (var c in input.Content)
            {
                i++;
                if (c.Equals('(')) notationCheck = true;
                if (i > 1)
                    if (char.IsUpper(c))
                        upperCaseCheck = true;
                if (char.IsNumber(c) && parsedSubject.Cabinet.Length < 4 && notationCheck == false
                )
                {
                    parsedSubject.Cabinet += c;
                }
                else
                {
                    if (upperCaseCheck == false)
                    {
                        parsedSubject.SubjectName += c;
                    }
                    else if (notationCheck)
                    {
                        parsedSubject.Notation += c;
                    }
                    else if (initialsCounter < 3)
                    {
                        parsedSubject.Teacher += c;
                        if (char.IsUpper(c))
                            initialsCounter++;
                    }
                }

                if (c.Equals(')'))
                    notationCheck = false;
            }

            parsedSubject.Teacher += ".";
            return parsedSubject;
        }
    }
}