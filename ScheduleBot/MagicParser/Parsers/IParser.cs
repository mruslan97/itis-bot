using System;
using System.Collections.Generic;
using System.Text;
using MagicParser.Models;

namespace MagicParser.Parsers
{
    public interface IParser
    {
        ParsedSubject Parse(TmpObject input);
    }
}
