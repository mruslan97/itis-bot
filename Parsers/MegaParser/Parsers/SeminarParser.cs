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
                Time = input.Time
            };
            var notationCheck = false;
            var upperCaseCheck = false;
            var i = 0;
            foreach (var char_ in input.Content)
            {
                i++;
                if (char_.Equals('(')) notationCheck = true;
                if (i > 1)
                    if (char.IsUpper(char_))
                        upperCaseCheck = true;
                if (char.IsNumber(char_) && parsedSubject.Cabinet.Length < 4 && notationCheck == false
                ) 
                {
                    parsedSubject.Cabinet = parsedSubject.Cabinet + char_;
                }
                else
                {
                    if (!upperCaseCheck)
                    {
                        parsedSubject.SubjectName = parsedSubject.SubjectName + char_;
                    }
                    if (notationCheck)
                    {
                        parsedSubject.Notation = parsedSubject.Notation + char_;
                    }
                    if (initialsCounter < 3)
                    {
                        parsedSubject.Teacher = parsedSubject.Teacher + char_;
                        if (char.IsUpper(char_))
                            initialsCounter++;
                    }
                }

                if (char_.Equals(')'))
                    notationCheck = false;
            }
            parsedSubject.Teacher += ".";
            return parsedSubject;
        }
    }
}
