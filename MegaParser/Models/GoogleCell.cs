using System;
using System.Collections.Generic;
using System.Text;

namespace MegaParser.Models
{
    public class GoogleCell
    {
        public string Text { get; set; }
        public string Time { get; set; }

        public GoogleCell(string text, string time)
        {
            Text = text;
            Time = time;
        }

        public GoogleCell()
        {

        }
    }
}
