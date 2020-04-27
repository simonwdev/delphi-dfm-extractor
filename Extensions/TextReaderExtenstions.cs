using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Extensions
{
    public static class TextReaderExtensions
    {
        public static char ReadChar(this TextReader source)
        {
            var value = source.Read();

            if (value == -1)
                return '\0';

            return (char)value;
        }
    }
}
