using System;
using System.Linq;
using GoogleParser;
using MegaParser.Models;
using MegaParser.Parsers;

namespace MegaParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var testString = "Алгоритмы и структуры данных Ференец А.А. 1304( 21.03 на 12.04 в 11.50 в 1301)";
            var parser = new SeminarParser();
            parser.Parse(testString);
            var googleService = new GoogleApiService();
            var response = googleService.SendRequest(3, 5);

            var testSeminar = parser.Parse(testString);
            Console.WriteLine($"Time {testSeminar.Time} Subject {testSeminar.SubjectName} Cabinet {testSeminar.Cabinet} " +
                              $"Teacher {testSeminar.Teacher} Notation {testSeminar.Notation}");
            
            Console.ReadLine();
        }
    }
}
