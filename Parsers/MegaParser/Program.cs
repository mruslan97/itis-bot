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
            var subjects = googleApi.SendRequest(3, 5);
            Console.ReadLine();
        }
    }
}