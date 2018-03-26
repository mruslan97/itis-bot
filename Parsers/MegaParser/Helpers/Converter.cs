using System;
using System.Collections.Generic;
using System.Text;

namespace MegaParser.Helpers
{
    public class Converter
    {
        public static int NormalizeGroupNumber(int course)
        {
            switch (course)
            {
                case 1:
                    return 7;
                case 2:
                    return 6;
                case 3:
                    return 5;
                case 4:
                    return 4;
                    default:
                        throw new ArgumentOutOfRangeException();
            }
        }
    }
}
