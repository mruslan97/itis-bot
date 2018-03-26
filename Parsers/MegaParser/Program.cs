using System;
using MegaParser.Models;
using MegaParser.Parsers;
using MegaParser.Services;

namespace MegaParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var googleApi = new GoogleApiService();
            var smartSorter = new SmartSortService();
            var subjects = googleApi.SendRequest(3, 3);
            var tmp = smartSorter.Parse(subjects);
            Console.ReadLine();
        }
    }
}