using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class MiniJson
{
    public static object Deserialize(string json)
    {
        if (json == null) return null;
        return Parser.Parse(json);
    }

    private class Parser : IDisposable
    {
        private const string WORD_BREAK = "{}[],:\"";
        private StringReader _json;

        private Parser(string json)
        {
            _json = new StringReader(json);
        }

        public static object Parse(string json)
        {
            using (Parser parser = new Parser(json))
            {
                return parser.ParseValue();
            }
        }

        public void Dispose()
        {
            _json.Dispose();
            _json = null;
        }

        private Dictionary<string, object> ParseObject()
        {
            Dictionary<string, object> table = new Dictionary<string, object>();
            _json.Read();
            while (true)
            {
                TOKEN nextToken = NextToken;
                if (nextToken == TOKEN.NONE) return null;
                if (nextToken == TOKEN.CURLY_CLOSE) return table;
                if (nextToken != TOKEN.STRING) return null;
                string name = ParseString();
                if (NextToken != TOKEN.COLON) return null;
                _json.Read();
                table[name] = ParseValue();
                switch (NextToken)
                {
                    case TOKEN.COMMA:
                        _json.Read();
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        _json.Read();
                        return table;
                    default:
                        return null;
                }
            }
        }

        private List<object> ParseArray()
        {
            List<object> array = new List<object>();
            _json.Read();
            bool parsing = true;
            while (parsing)
            {
                TOKEN nextToken = NextToken;
                if (nextToken == TOKEN.NONE) return null;
                if (nextToken == TOKEN.SQUARE_CLOSE)
                {
                    _json.Read();
                    break;
                }
                object value = ParseValue();
                array.Add(value);
                switch (NextToken)
                {
                    case TOKEN.COMMA:
                        _json.Read();
                        break;
                    case TOKEN.SQUARE_CLOSE:
                        _json.Read();
                        parsing = false;
                        break;
                    default:
                        return null;
                }
            }
            return array;
        }

        private object ParseValue()
        {
            TOKEN nextToken = NextToken;
            switch (nextToken)
            {
                case TOKEN.STRING: return ParseString();
                case TOKEN.NUMBER: return ParseNumber();
                case TOKEN.CURLY_OPEN: return ParseObject();
                case TOKEN.SQUARE_OPEN: return ParseArray();
                case TOKEN.TRUE: _json.Read(); _json.Read(); _json.Read(); _json.Read(); return true;
                case TOKEN.FALSE: _json.Read(); _json.Read(); _json.Read(); _json.Read(); _json.Read(); return false;
                case TOKEN.NULL: _json.Read(); _json.Read(); _json.Read(); _json.Read(); return null;
                default: return null;
            }
        }

        private string ParseString()
        {
            StringBuilder s = new StringBuilder();
            char c;
            _json.Read();
            bool parsing = true;
            while (parsing)
            {
                if (_json.Peek() == -1) break;
                c = NextChar;
                if (c == '"') { parsing = false; break; }
                if (c == '\\')
                {
                    if (_json.Peek() == -1) parsing = false;
                    c = NextChar;
                    if (c == '"') s.Append('"');
                    else if (c == '\\') s.Append('\\');
                    else if (c == '/') s.Append('/');
                    else if (c == 'b') s.Append('\b');
                    else if (c == 'f') s.Append('\f');
                    else if (c == 'n') s.Append('\n');
                    else if (c == 'r') s.Append('\r');
                    else if (c == 't') s.Append('\t');
                    else if (c == 'u')
                    {
                        char[] hex = new char[4];
                        for (int i = 0; i < 4; i++) hex[i] = NextChar;
                        s.Append((char)Convert.ToInt32(new string(hex), 16));
                    }
                }
                else s.Append(c);
            }
            return s.ToString();
        }

        private object ParseNumber()
        {
            string number = NextWord;
            if (number.IndexOf('.') != -1 || number.IndexOf('e') != -1 || number.IndexOf('E') != -1)
            {
                double parsedDouble;
                double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedDouble);
                return parsedDouble;
            }
            long parsedInt;
            long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedInt);
            return parsedInt;
        }

        private void EatWhitespace()
        {
            while (_json.Peek() != -1)
            {
                char c = (char)_json.Peek();
                if (char.IsWhiteSpace(c)) { _json.Read(); continue; }
                break;
            }
        }

        private char NextChar { get { return (char)_json.Read(); } }
        private string NextWord
        {
            get
            {
                StringBuilder word = new StringBuilder();
                while (_json.Peek() != -1 && !IsWordBreak((char)_json.Peek()))
                {
                    word.Append(NextChar);
                }
                return word.ToString();
            }
        }

        private TOKEN NextToken
        {
            get
            {
                EatWhitespace();
                if (_json.Peek() == -1) return TOKEN.NONE;
                char c = (char)_json.Peek();
                if (c == '{') return TOKEN.CURLY_OPEN;
                if (c == '}') { _json.Read(); return TOKEN.CURLY_CLOSE; }
                if (c == '[') return TOKEN.SQUARE_OPEN;
                if (c == ']') { _json.Read(); return TOKEN.SQUARE_CLOSE; }
                if (c == ',') return TOKEN.COMMA;
                if (c == '"') return TOKEN.STRING;
                if (c == ':') return TOKEN.COLON;
                if (c == '-' || char.IsDigit(c)) return TOKEN.NUMBER;
                string word = NextWord;
                if (word == "false") return TOKEN.FALSE;
                if (word == "true") return TOKEN.TRUE;
                if (word == "null") return TOKEN.NULL;
                return TOKEN.NONE;
            }
        }

        private static bool IsWordBreak(char c)
        {
            return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
        }

        private enum TOKEN
        {
            NONE, CURLY_OPEN, CURLY_CLOSE, SQUARE_OPEN, SQUARE_CLOSE,
            COLON, COMMA, STRING, NUMBER, TRUE, FALSE, NULL
        }
    }

    private class StringReader : IDisposable
    {
        private readonly string _s;
        private int _pos;

        public StringReader(string s)
        {
            _s = s;
            _pos = 0;
        }

        public int Peek()
        {
            if (_pos >= _s.Length) return -1;
            return _s[_pos];
        }

        public int Read()
        {
            if (_pos >= _s.Length) return -1;
            return _s[_pos++];
        }

        public void Dispose() { }
    }
}
