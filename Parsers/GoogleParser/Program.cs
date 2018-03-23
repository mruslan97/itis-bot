using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Util;

namespace GoogleParser
{
    class Program
    {
        static void Main()
        {
            var googleService = new GoogleApiService();
            var response = googleService.SendRequest(3, 5);

            var tmp = response.ValueRanges[0].Values
                .Zip(response.ValueRanges[1].Values, (x, y) => new {Subject = y, Time = x})
                .Where(x => x.Subject.Count > 0)
                .ToList().Select(x => new Lesson()
                {
                    Time = (string) x.Time.FirstOrDefault(),
                    Subject = (string) x.Subject.FirstOrDefault()
                });
            Console.WriteLine(tmp.FirstOrDefault().Subject);
            Console.ReadLine();
        }
    }

    class Lesson
    {
        public string Time { get; set; }
        public string Subject { get; set; }
    }
}