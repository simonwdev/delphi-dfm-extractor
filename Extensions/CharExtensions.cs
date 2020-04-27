using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Extensions
{
    public static class CharExtension
    {
        public static bool IsWithinRange(this char source, char lower, char upper)
        {
            return source >= lower && source <= upper;
        }
    }
}
