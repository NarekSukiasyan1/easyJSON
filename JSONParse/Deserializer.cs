using System.Text;
using easyJSON.jsonValue;
using System.Globalization;

namespace easyJSON {
    public class Deserializer {

        private TextReader? reader;
        private jsonObject? result = new();
        private jsonObjectArray? ArrayResult;

        private string key = "";
        private char Peek() {
            return UnicodeEncoding.Unicode.GetString(BitConverter.GetBytes(reader.Peek()))[0];
        }
        private char Read() {
            return UnicodeEncoding.Unicode.GetString(BitConverter.GetBytes(reader.Read()))[0];
        }
        private void Skip() {
            reader.Read();
        }


        private void DeserializeArrayOfObjects() {
            while(true) {
                SkipWhitespaces();
                string SubObject = "";
                Stack<char> OpenBrackets = new();
                Stack<char> CloseBrackets = new();
                while(true) {
                    char ch = Peek();
                    if(ch == '{') {
                        OpenBrackets.Push(ch);
                        SubObject += Read();
                    }
                    else if(ch == '[') {
                        OpenBrackets.Push(ch);
                        SubObject += Read();
                    }
                    else if(ch == '}') {
                        CloseBrackets.Push(ch);
                        if(OpenBrackets.Peek() == '{' && CloseBrackets.Peek() == '}') {
                            OpenBrackets.Pop();
                            CloseBrackets.Pop();
                        }
                        SubObject += Read();
                        if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                            break;
                        }
                        else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                            throw new Exception();
                        }
                        else { continue; }
                    }
                    else if(ch == ']') {
                        CloseBrackets.Push(ch);
                        if(OpenBrackets.Peek() == '[' && CloseBrackets.Peek() == ']') {
                            OpenBrackets.Pop();
                            CloseBrackets.Pop();
                        }
                        SubObject += Read();
                        if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                            break;
                        }
                        else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                            throw new Exception();
                        }
                        else { continue; }
                    }
                    else {
                        SubObject += Read();
                    }

                }
                ArrayResult.array.Add((jsonObject)jsonParser.Deserialize(SubObject, true));

                SkipWhitespaces();

                if(Peek() == ']') {
                    break;
                }
                else if(Peek() == ',') {
                    Skip();
                    continue;
                }
                else {
                    throw new Exception();
                }
            }
        }
        private void DeserializeArrayOfValues() {
            List<object> values = new List<object>();
            while(true) {
                SkipWhitespaces();
                ReadValue();
                values.Add(result.values[key]);
                ArrayResult.array.Add(result.values[key]);
                SkipWhitespaces();
                if(Peek() == ',') {
                    Skip();
                    continue;
                }
                else { break; }
            }

        }
        public jsonObjectArray DeserializeArray(StringReader sreader) {
            if(reader == null) {
                reader = sreader;
            }
            ArrayResult = new();
            Skip();

            SkipWhitespaces();
            if(Peek() == '{') {
                DeserializeArrayOfObjects();
            }
            else {
                DeserializeArrayOfValues();
                ArrayResult.isPrimitive = true;
            }

            return ArrayResult;
        }

        private bool IsValue() {
            switch(Peek()) {
                case '\"': return true;
                case 't': return true;
                case 'f': return true;
                case 'n': return true;
                default:
                    return IsNum(Peek());
            }
        }

        public jsonObject Deserialize(StringReader sreader) {
            result = new();
            if(reader == null) {
                reader = sreader;
            }
            if(reader.Peek() == -1) {
                throw new Exception();
            }
            SkipWhitespaces();
            if(Peek() == '{') {
                ReadObject();
                SkipWhitespaces();
                if(Peek() == '}') {
                    Skip();
                }
                else { throw new Exception(); }
            }
            else if(IsValue()) {
                key = "Single Object";
                ReadValue();
            }
            else { throw new Exception(); }
            return result;
        }
        private void SkipWhitespaces() {
            while(true) {
                switch(Peek()) {
                    case ' ':
                        Skip();
                        continue;
                    case '\t':
                        Skip();
                        continue;
                    case '\n':
                        Skip();
                        continue;
                    case '\r':
                        Skip();
                        continue;
                    case '\0':
                        Skip();
                        continue;
                    default:
                        return;
                }
            }
        }
        private void ReadKey() {
            bool isQoutation = false;
            if(Peek() == '\"') {
                isQoutation = true;
                Skip();
            }
            while(true) {
                char ch = Read();
                if(ch == '\"' && isQoutation) {
                    SkipWhitespaces();
                    if(Peek() == ':') { Skip(); break; }
                    else throw new Exception();
                }
                else if(ch == ':') {
                    break;
                }
                else {
                    key += ch;
                }
            }
            result.values.Add(key, null);
            SkipWhitespaces();
            ReadValue();
            key = "";
        }
        private void ReadString() {
            Read();
            string value = "";
            while(true) {
                if(Peek() == '\"') {
                    Read();
                    break;
                }
                value += Read();
            }
            result.values[key] = value;
        }
        private void ReadBool() {
            if(Read() == 't') {
                for(int i = 1; i < 4; i++) {
                    if(Read() == "true"[i]) continue;
                    else throw new Exception();
                }
                result.values[key] = true;
            }
            else {
                for(int i = 1; i < 5; i++) {
                    if(Read() == "false"[i]) continue;
                    else throw new Exception();
                }
                result.values[key] = false;
            }
        }
        private void ReadNull() {
            Skip();
            for(int i = 1; i < 4; i++) {
                if(Read() == "null"[i]) continue;
                else throw new Exception();
            }
            result.values[key] = null;

        }
        private bool IsNum(char ch) {
            switch(ch) {
                case '0': return true;
                case '1': return true;
                case '2': return true;
                case '3': return true;
                case '4': return true;
                case '5': return true;
                case '6': return true;
                case '7': return true;
                case '8': return true;
                case '9': return true;
                case '-': return true;
                default: return false;

            }
        }
        private bool IsDot(char ch) {
            switch(ch) {

                case '.': return true;
                default: return false;
            }
        }
        private void ReadNum() {
            bool IsFloatingPointNum = false;
            string value = "";
            value += Read();
            char ch;
            while(true) {
                ch = Peek();
                if(IsNum(ch)) {
                    value += Read();
                }
                else if(IsDot(ch)) {
                    IsFloatingPointNum = true;
                    value += Read();
                }
                else {
                    if(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",") {
                        value = value.Replace('.', ',');
                    }
                    if(Peek() == 'E' || Peek() == 'e') {
                        value += Read();
                        if(Peek() == '-' || Peek() == '+') {
                            value += Read();
                            while(true) {
                                ch = Peek();
                                if(IsNum(ch)) {
                                    value += Read();
                                }
                                else {
                                    break;
                                }
                            }
                            int IndexOfE = value.ToUpper().IndexOf('E');
                            double a = double.Parse(value.Substring(0, IndexOfE));
                            if(value[IndexOfE + 1] == '+') {
                                result.values[key] = a * Math.Pow(10, double.Parse(value.Substring(IndexOfE + 2)));
                            }
                            else if(value[IndexOfE + 1] == '-') {
                                result.values[key] = a * Math.Pow(10, -double.Parse(value.Substring(IndexOfE + 2)));
                            }
                            else {
                                throw new Exception();
                            }
                        }
                        else {
                            throw new Exception();
                        }
                    }
                    else {
                        if(IsFloatingPointNum) {
                            result.values[key] = Double.Parse(value);
                        }
                        else {
                            result.values[key] = Int128.Parse(value);
                        }
                    }
                    break;
                }
            }
        }
        private void ReadValue() {
            SkipWhitespaces();
            char ch = Peek();
            switch(ch) {
                case '{':
                    ReadSubObject();
                    return;
                case '\"':
                    ReadString();
                    return;
                case 't':
                case 'f':
                    ReadBool();
                    return;
                case 'n':
                    ReadNull();
                    return;
                case '[':
                    ReadArray();
                    return;
                default:
                    if(IsNum(ch)) {
                        ReadNum();
                        return;
                    }
                    else { throw new Exception(); }
            }
        }




        private void ReadArray() {
            string array = "";
            array += Read();
            SkipWhitespaces();
            Stack<char> OpenBrackets = new();
            OpenBrackets.Push('[');
            Stack<char> CloseBrackets = new();
            while(true) {
                SkipWhitespaces();
                char ch = Peek();
                if(ch == '{') {
                    OpenBrackets.Push(ch);
                    array += Read();
                }
                else if(ch == '[') {
                    OpenBrackets.Push(ch);
                    array += Read();
                }
                else if(ch == '}') {
                    CloseBrackets.Push(ch);
                    if(OpenBrackets.Peek() == '{' && CloseBrackets.Peek() == '}') {
                        OpenBrackets.Pop();
                        CloseBrackets.Pop();
                    }
                    array += Read();
                    if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                        break;
                    }
                    else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                        throw new Exception();
                    }
                    else { continue; }
                }
                else if(ch == ']') {
                    CloseBrackets.Push(ch);
                    if(OpenBrackets.Peek() == '[' && CloseBrackets.Peek() == ']') {
                        OpenBrackets.Pop();
                        CloseBrackets.Pop();
                    }
                    array += Read();
                    if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                        break;
                    }
                    else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                        throw new Exception();
                    }
                    else { continue; }
                }
                else {
                    array += Read();
                }
            }
            result.values[key] = jsonParser.Deserialize(array, true);
        }

        private void ReadSubObject() {
            Stack<char> OpenBrackets = new();
            OpenBrackets.Push('{');
            Stack<char> CloseBrackets = new();
            string SubObject = "";
            SubObject += Read();
            while(true) {
                char ch = Peek();
                if(ch == '{') {
                    OpenBrackets.Push(ch);
                    SubObject += Read();
                }
                else if(ch == '[') {
                    OpenBrackets.Push(ch);
                    SubObject += Read();
                }
                else if(ch == '}') {
                    CloseBrackets.Push(ch);
                    if(OpenBrackets.Peek() == '{' && CloseBrackets.Peek() == '}') {
                        OpenBrackets.Pop();
                        CloseBrackets.Pop();
                    }
                    SubObject += Read();
                    if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                        break;
                    }
                    else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                        throw new Exception();
                    }
                    else { continue; }
                }
                else if(ch == ']') {
                    CloseBrackets.Push(ch);
                    if(OpenBrackets.Peek() == '[' && CloseBrackets.Peek() == ']') {
                        OpenBrackets.Pop();
                        CloseBrackets.Pop();
                    }
                    SubObject += Read();
                    if(OpenBrackets.Count == 0 && CloseBrackets.Count == 0) {
                        break;
                    }
                    else if(OpenBrackets.Count == 0 && CloseBrackets.Count != 0) {
                        throw new Exception();
                    }
                    else { continue; }
                }
                else {
                    SubObject += Read();
                }
            }
            result.values[key] = jsonParser.Deserialize(SubObject, true);
        }

        private void ReadObject() {
            Skip();
            while(true) {
                SkipWhitespaces();
                ReadKey();
                if(Peek() == ',') {
                    Read();
                    continue;
                }
                return;
            }
        }
    }
}
