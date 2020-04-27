using DfmExtractor.Commands;
using System;

namespace DfmExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Processing DFM files...");

            new UpdateDfmFolder().Execute();

            Console.WriteLine("Processing complete. Enjoy your DFM files.");
        }
    }
}
