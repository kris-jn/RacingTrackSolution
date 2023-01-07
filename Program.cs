using System;
using System.IO;
using System.Linq;

namespace RacingTrackSolution2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            do
            {
                ITrackService trackService = new TrackService();

                Console.WriteLine("Input File path:");
                var filePath = Console.ReadLine();
                var strList = File.ReadAllLines(filePath);

                var result = trackService.ExecuteRequest(strList);
                foreach (var item in result)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("\n\n Want to Input another document?[y/n]");
            }
            while (Console.ReadKey().Key == ConsoleKey.Y);
        }
    }
}
