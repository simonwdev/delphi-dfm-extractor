using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Delphi
{
    public static class DfmTokens
    {
        public const char Eof = (char)0;
        public const char Symbol = (char)1;
        public const char String = (char)2;
        public const char Integer = (char)3;
        public const char Float = (char)4;
        public const char WideString = (char)5;
    }
}
