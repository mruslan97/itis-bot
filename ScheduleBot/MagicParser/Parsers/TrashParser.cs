using System;
using System.Collections.Generic;
using System.Text;
using MagicParser.Models;

namespace MagicParser.Parsers
{
    public class TrashParser : IParser
    {
        public ParsedSubject Parse(TmpObject input)
        {
            return new ParsedSubject();
        }
    }
}
