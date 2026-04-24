/*
 * MiniJSON – a minimal JSON serializer / deserializer for Unity.
 * Original by Calvin Rien (darktable), MIT License.
 * Trimmed for WordConnect project – only Deserialize is needed.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniJSON
{
    public static class Json
    {
        public static object Deserialize(string json)
        {
            if (json == null) return null;
            return Parser.Parse(json);
        }

        sealed class Parser : IDisposable
        {
            const string WORD_BREAK = "{}[],:\"";

            StringReader json;

            Parser(string jsonString) { json = new StringReader(jsonString); }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                    return instance.ParseValue();
            }

            public void Dispose() { json.Dispose(); }

            Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();
                json.Read(); // {
                while (true)
                {
                    switch (NextToken)
                    {
                        case TOKEN.NONE:   return null;
                        case TOKEN.CURLY_CLOSE: return table;
                        default:
                            string name = ParseString();
                            if (name == null) return null;
                            if (NextToken != TOKEN.COLON) return null;
                            json.Read();
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            List<object> ParseArray()
            {
                var array = new List<object>();
                json.Read(); // [
                bool parsing = true;
                while (parsing)
                {
                    TOKEN nextToken = NextToken;
                    switch (nextToken)
                    {
                        case TOKEN.NONE:         return null;
                        case TOKEN.SQUARED_CLOSE: parsing = false; break;
                        default:
                            array.Add(ParseByToken(nextToken));
                            break;
                    }
                }
                return array;
            }

            object ParseValue()    => ParseByToken(NextToken);

            object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.STRING:       return ParseString();
                    case TOKEN.NUMBER:       return ParseNumber();
                    case TOKEN.CURLY_OPEN:   return ParseObject();
                    case TOKEN.SQUARED_OPEN: return ParseArray();
                    case TOKEN.TRUE:         return true;
                    case TOKEN.FALSE:        return false;
                    case TOKEN.NULL:         return null;
                    default:                 return null;
                }
            }

            string ParseString()
            {
                var s = new StringBuilder();
                json.Read(); // "
                while (true)
                {
                    if (json.Peek() == -1) return null;
                    char c = NextChar;
                    switch (c)
                    {
                        case '"':  return s.ToString();
                        case '\\':
                            if (json.Peek() == -1) return null;
                            char escaped = NextChar;
                            switch (escaped)
                            {
                                case '"':  s.Append('"');  break;
                                case '\\': s.Append('\\'); break;
                                case '/':  s.Append('/');  break;
                                case 'n':  s.Append('\n'); break;
                                case 'r':  s.Append('\r'); break;
                                case 't':  s.Append('\t'); break;
                                case 'u':
                                    var hex = new StringBuilder();
                                    for (int i = 0; i < 4; i++)
                                        hex.Append(NextChar);
                                    s.Append((char)Convert.ToInt32(hex.ToString(), 16));
                                    break;
                            }
                            break;
                        default: s.Append(c); break;
                    }
                }
            }

            object ParseNumber()
            {
                string number = NextWord;
                if (number.IndexOf('.') == -1)
                {
                    if (long.TryParse(number, out long parsed))
                        return parsed;
                }
                if (double.TryParse(number,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out double d))
                    return d;
                return 0;
            }

            void EatWhitespace()
            {
                while (!IsEndOfStream)
                {
                    if (!char.IsWhiteSpace((char)json.Peek())) break;
                    json.Read();
                }
            }

            char NextChar => (char)json.Read();

            bool IsEndOfStream => json.Peek() == -1;

            string NextWord
            {
                get
                {
                    var word = new StringBuilder();
                    while (!IsEndOfStream && !IsWordBreak((char)json.Peek()))
                        word.Append(NextChar);
                    return word.ToString();
                }
            }

            bool IsWordBreak(char c) =>
                char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;

            TOKEN NextToken
            {
                get
                {
                    EatWhitespace();
                    if (IsEndOfStream) return TOKEN.NONE;
                    switch ((char)json.Peek())
                    {
                        case '{': return TOKEN.CURLY_OPEN;
                        case '}': json.Read(); return TOKEN.CURLY_CLOSE;
                        case '[': return TOKEN.SQUARED_OPEN;
                        case ']': json.Read(); return TOKEN.SQUARED_CLOSE;
                        case ',': json.Read(); return TOKEN.COMMA;
                        case '"': return TOKEN.STRING;
                        case ':': return TOKEN.COLON;
                        case '0': case '1': case '2': case '3':
                        case '4': case '5': case '6': case '7':
                        case '8': case '9': case '-': return TOKEN.NUMBER;
                        default:
                            string word = NextWord;
                            return word switch
                            {
                                "false" => TOKEN.FALSE,
                                "true"  => TOKEN.TRUE,
                                "null"  => TOKEN.NULL,
                                _       => TOKEN.NONE
                            };
                    }
                }
            }

            enum TOKEN
            {
                NONE, CURLY_OPEN, CURLY_CLOSE, SQUARED_OPEN, SQUARED_CLOSE,
                COLON, COMMA, STRING, NUMBER, TRUE, FALSE, NULL
            }
        }
    }
}
