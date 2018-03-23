using System;
using System.Collections.Generic;
using System.Text;
using MegaParser.Models;

namespace MegaParser.Parsers
{
    public interface IParser
    {
        ParsedSubject Parse(string input);
    }
}
