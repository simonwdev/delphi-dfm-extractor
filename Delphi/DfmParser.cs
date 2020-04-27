using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfmExtractor.Delphi
{
    public sealed class DfmParser
    {
        private readonly DfmLexer _dfmLexer;

        private string CombineString()
        {
            var builder = new StringBuilder();
            builder.Append(_dfmLexer.TokenString());

            while (_dfmLexer.NextToken() == '+')
            {
                _dfmLexer.NextToken();

                if (_dfmLexer.Token != DfmTokens.String && _dfmLexer.Token != DfmTokens.WideString)
                    throw new ApplicationException("Invalid string join.");

                builder.Append(_dfmLexer.TokenString());
            }


            builder.AppendLine();

            return builder.ToString();
        }

        public DfmParser(DfmLexer dfmLexer)
        {
            _dfmLexer = dfmLexer;
            _dfmLexer.NextToken();
        }

        public DfmObject ReadObject()
        {
            var result = new DfmObject();

            // Inherited, Inline, Object
            _dfmLexer.CheckToken(DfmTokens.Symbol);

            _dfmLexer.NextToken();

            ReadHeader(result);

            while (!_dfmLexer.TokenSymbolIs("END") && !_dfmLexer.TokenSymbolIs("OBJECT") && !_dfmLexer.TokenSymbolIs("INHERITED") && !_dfmLexer.TokenSymbolIs("INLINE"))
            {
                result.Properties.Add(ReadProperty());
            }

            while (!_dfmLexer.TokenSymbolIs("END"))
            {
                result.Children.Add(ReadObject());
            }

            _dfmLexer.NextToken();

            return result;
        }

        public void ReadHeader(DfmObject dfmObject)
        {
            _dfmLexer.CheckToken(DfmTokens.Symbol);

            var className = _dfmLexer.TokenString();
            var objectName = string.Empty;

            if (_dfmLexer.NextToken() == ':')
            {
                _dfmLexer.NextToken();
                _dfmLexer.CheckToken(DfmTokens.Symbol);

                objectName = className;
                className = _dfmLexer.TokenString();

                _dfmLexer.NextToken();
            }

            if (_dfmLexer.Token == '[')
            {
                _dfmLexer.NextTokenUntil(']');
                _dfmLexer.NextToken();
            }

            dfmObject.ClassName = className;
            dfmObject.ObjectName = objectName;
        }

        public DfmProperty ReadProperty()
        {
            var result = new DfmProperty();

            _dfmLexer.CheckToken(DfmTokens.Symbol);

            result.Name = _dfmLexer.TokenString();

            _dfmLexer.NextToken();

            while (_dfmLexer.Token == '.')
            {
                _dfmLexer.NextToken();
                _dfmLexer.CheckToken(DfmTokens.Symbol);

                result.Name = result.Name + '.' + _dfmLexer.TokenString();

                _dfmLexer.NextToken();
            }

            _dfmLexer.CheckToken('=');
            _dfmLexer.NextToken();

            result.Value = ReadValue();

            return result;
        }

        public string ReadValue()
        {
            var result = string.Empty;

            if (_dfmLexer.Token == DfmTokens.String || _dfmLexer.Token == DfmTokens.WideString)
            {
                result = CombineString();
            }
            else
            {
                switch (_dfmLexer.Token)
                {
                    case DfmTokens.Symbol:
                        result = _dfmLexer.NextAsComponentIdentifier();
                        if (result != "true" && result != "false")
                            result = _dfmLexer.NextAsComponentIdentifier();
                        break;
                    case '(':
                        _dfmLexer.NextToken();

                        while (_dfmLexer.Token != ')')
                        {
                            result += ReadValue();
                        }
                        break;
                    case DfmTokens.Integer:
                        break;
                    case DfmTokens.Float:
                        break;
                    case '[':
                        _dfmLexer.NextTokenUntil(']');
                        break;
                    case '{':
                        _dfmLexer.NextTokenUntil('}');
                        break;
                    case '<':
                        _dfmLexer.NextTokenUntilNested('<', '>');
                        break;
                    default:
                        throw new ApplicationException("Invalid property symbol.");
                }

                _dfmLexer.NextToken();
            }

            return result;
        }
    }
}
