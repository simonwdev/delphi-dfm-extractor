using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfmExtractor.Extensions;

namespace DfmExtractor.Delphi
{
    public sealed class DfmLexer
    {
        private readonly TextReader _textReader;
        private readonly StringBuilder _tokenCache;
        private char _current;

        public int LineNumber { get; private set; }
        public int ColumnStart { get; private set; }
        public int ColumnEnd { get; private set; }
        public char Token { get; private set; }

        private void SkipBlanks()
        {
            while (true)
            {
                if (_current == '\0')
                {
                    return;
                }

                if (_current == '\n')
                {
                    LineNumber++;
                    ColumnEnd = -1;
                }
                else if (_current >= 33 && _current <= 255)
                {
                    return;
                }

                _current = _textReader.ReadChar();
                ColumnEnd++;
            }
        }

        public DfmLexer(TextReader textReader)
        {
            _textReader = textReader;
            _tokenCache = new StringBuilder();

            _current = textReader.ReadChar();

            ColumnStart = 0;
            ColumnEnd = 0;

            LineNumber = 1;
        }

        public char NextToken()
        {
            SkipBlanks();

            _tokenCache.Clear();

            Token = DfmTokens.Eof;
            ColumnStart = ColumnEnd + 1;

            if (_current.IsWithinRange('a', 'z') || _current.IsWithinRange('A', 'Z') || _current == '_')
            {
                _tokenCache.Append(_current);

                _current = _textReader.ReadChar();
                ColumnEnd++;

                while (_current.IsWithinRange('a', 'z') || _current.IsWithinRange('A', 'Z') || _current.IsWithinRange('0', '9') || _current == '_')
                {
                    _tokenCache.Append(_current);

                    _current = _textReader.ReadChar();
                    ColumnEnd++;
                }

                Token = DfmTokens.Symbol;
            }
            else if (_current == '#' || _current == '\'')
            {
                while (true)
                {
                    if (_current == '#')
                    {
                        _current = _textReader.ReadChar();
                        ColumnEnd++;

                        var i = 0;
                        while (_current.IsWithinRange('0', '9'))
                        {
                            i = i * 10 + (_current - '0');

                            _current = _textReader.ReadChar();
                            ColumnEnd++;
                        }

                        if (i > 127)
                        {
                            _tokenCache.Append((char)i);
                        }
                    }
                    else if (_current == '\'')
                    {
                        _current = _textReader.ReadChar();
                        ColumnEnd++;

                        while (true)
                        {
                            if (_current == '\0' || _current == '\r' || _current == '\n')
                            {
                                throw new ApplicationException("Invalid string.");
                            }
                            else if (_current == '\'')
                            {
                                _current = _textReader.ReadChar();
                                ColumnEnd++;

                                if (_current != '\t')
                                    break;
                            }
                            else
                            {
                                _tokenCache.Append(_current);
                            }

                            _current = _textReader.ReadChar();
                            ColumnEnd++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                Token = DfmTokens.String;
            }
            else if (_current == '$')
            {
                _tokenCache.Append(_current);

                _current = _textReader.ReadChar();
                ColumnEnd++;

                while (_current.IsWithinRange('0', '9'))
                {
                    _tokenCache.Append(_current);

                    _current = _textReader.ReadChar();
                    ColumnEnd++;
                }

                Token = DfmTokens.Integer;
            }
            else if (_current == '-' || _current.IsWithinRange('0', '9'))
            {
                _tokenCache.Append(_current);

                _current = _textReader.ReadChar();
                ColumnEnd++;

                while (_current.IsWithinRange('0', '9'))
                {
                    _tokenCache.Append(_current);

                    _current = _textReader.ReadChar();
                    ColumnEnd++;
                }

                Token = DfmTokens.Integer;

                while (_current.IsWithinRange('0', '9') || _current == '.' || _current == 'e' || _current == 'E' || _current == '+' || _current == '-')
                {
                    _current = _textReader.ReadChar();
                    ColumnEnd++;

                    Token = DfmTokens.Float;
                }

                if (_current == 'c' || _current == 'C' || _current == 'd' || _current == 'D' || _current == 's' || _current == 'S')
                {
                    _current = _textReader.ReadChar();
                    ColumnEnd++;

                    Token = DfmTokens.Float;

                    // FLOAT TYPE
                }
                else
                {
                    // FLOAT TYPE
                }
            }
            else
            {
                Token = _current;

                _tokenCache.Append(_current);

                _current = _textReader.ReadChar();
                ColumnEnd++;
            }

            return Token;
        }

        public string NextAsComponentIdentifier()
        {
            CheckToken(DfmTokens.Symbol);

            while (_current == '.')
            {
                _tokenCache.Append(_current);

                _current = _textReader.ReadChar();
                ColumnEnd++;

                if (!_current.IsWithinRange('a', 'z') && !_current.IsWithinRange('A', 'Z') && !_current.IsWithinRange('0', '9') && _current != '_')
                {
                    throw new ApplicationException("Invalid identifier.");
                }

                _tokenCache.Append(_current);

                do
                {
                    _tokenCache.Append(_current);

                    _current = _textReader.ReadChar();
                    ColumnEnd++;

                } while (_current.IsWithinRange('a', 'z') || _current.IsWithinRange('A', 'Z') || _current.IsWithinRange('0', '9') || _current == '_');
            }

            return _tokenCache.ToString();
        }
        public void NextTokenUntil(char token)
        {
            var readToken = NextToken();

            while (readToken != token)
            {
                readToken = NextToken();
            }
        }

        public void NextTokenUntilNested(char startToken, char endToken)
        {
            var endCount = 1;
            var readToken = NextToken();

            if (readToken == endToken)
                endCount--;

            while (endCount > 0)
            {
                readToken = NextToken();

                if (readToken == startToken)
                    endCount++;

                if (readToken == endToken)
                    endCount--;
            }
        }
        public bool TokenSymbolIs(string symbolValue)
        {
            return Token == DfmTokens.Symbol && _tokenCache.ToString()
                .Equals(symbolValue, StringComparison.InvariantCultureIgnoreCase);
        }
        public void CheckToken(char token)
        {
            if (Token != token)
                throw new ApplicationException($"Expected '{token}' but found '{Token}'.");
        }
        public string TokenName()
        {
            switch (Token)
            {
                case DfmTokens.Eof:
                    return "Float";
                case DfmTokens.Symbol:
                    return "Symbol";
                case DfmTokens.String:
                    return "String";
                case DfmTokens.Integer:
                    return "Integer";
                case DfmTokens.Float:
                    return "Float";
                case DfmTokens.WideString:
                    return "WideString";
                default:
                    return Token.ToString();
            }
        }
        public string TokenString()
        {
            return _tokenCache.ToString();
        }

        public override string ToString()
        {
            return $"L{LineNumber}C{ColumnStart}-{ColumnEnd}: {TokenName()} = '{TokenString()}'";
        }
    }
}
