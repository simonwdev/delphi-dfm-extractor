using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using DfmExtractor.Commands;
using DfmExtractor.Delphi;
using DfmExtractor.Extensions;

namespace DfmExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var lexer = new DfmLexer(File.OpenText(@"C:\CCare\Development\Misc\AboutBoxFrm.dfm"));
            
            var parser = new DfmParser(lexer);

            var form = parser.ReadObject();

            var forms = form.Children
                .Flatten(a => a.Children)
                .SelectMany(a => a.Properties)
                .Where(a => a.Name.Contains("SQL"))
                .ToArray();

            foreach (var prop in forms)
            {
                //Console.WriteLine(prop.);
                //Console.WriteLine(prop.Value);
            }

            Console.WriteLine("Processing DFM files...");

            new UpdateDfmFolder().Execute();

            Console.WriteLine("Processing complete. Enjoy your DFM files.");
        }
    }
}
