using System.Reflection;
namespace easyJSON {
    public class Serializer {
        private static string WriteBool(object value) {
            switch(value) {
                case true: return "true";
                case false: return "false";
                default: throw new Exception();
            }
        }
        private static object NumConverter(object num, Type toField) {
            switch(toField.Name) {
                case "Byte":
                    return (byte)num;
                case "SByte":
                    return (sbyte)num;
                case "Int16":
                    return (short)num;
                case "UInt16":
                    return (ushort)num;
                case "Int32":
                    return (int)num;
                case "UInt32":
                    return (uint)num;
                case "Int64":
                    return (long)num;
                case "UInt64":
                    return (ulong)num;
                case "Int128":
                    return (Int128)num;
                case "UInt128":
                    return (UInt128)num;
                case "Single":
                    return (float)num;
                case "Double":
                    return (Double)num;
                case "Decimal":
                    return (decimal)num;
                case "Nullable`1":
                    return NumConverter(num, Nullable.GetUnderlyingType(toField));
                default:
                    throw new Exception();
            }
        }

        private static bool IsNum(Type toField) {
            switch(toField.Name) {
                case "Byte":
                    return true;
                case "SByte":
                    return true;
                case "Int16":
                    return true;
                case "UInt16":
                    return true;
                case "Int32":
                    return true;
                case "UInt32":
                    return true;
                case "Int64":
                    return true;
                case "UInt64":
                    return true;
                case "Int128":
                    return true;
                case "UInt128":
                    return true;
                case "Single":
                    return true;
                case "double":
                    return true;
                case "Decimal":
                    return true;
                case "Nullable`1":
                    return IsNum(Nullable.GetUnderlyingType(toField));
                default:
                    return false;
            }
        }
        private static bool IsArray(object value) {

            if(value.GetType().Name.Contains("[]")) return true;
            else return false;

        }

        private static string ReadArray(object value) {
            string result = "[";
            if(value is Array valueArray) {
                foreach(var item in valueArray) {
                    result += WriteValue(item) +',';
                }
            }
            return result.Remove(result.Length - 1) + ']';
        }

        private static string ReadObject(object obj) {
            string json = "{";
            TypedReference reference = __makeref(obj);
            foreach(var fromField in obj.GetType().GetFields()) {
                var toField = obj.GetType().GetField(
                    fromField.Name, BindingFlags.Public | BindingFlags.Instance);

                object value = toField.GetValueDirect(reference);
                if(value == null)
                    json += '\"' + toField.Name + '\"' + ':' + "null" + ',';
                else
                    json += '\"' + toField.Name + '\"' + ':' + WriteValue(value) + ',';

            }
            json = json.Remove(json.Length - 1);
            json += '}';
            return json;
        }

        private static string WriteValue(object? value, Type? type = null) {
            if(type == null) type = value.GetType();
            switch(type.Name) {
                case "Nullable`1":
                    if(value == null) return "null";
                    else return WriteValue(value, Nullable.GetUnderlyingType(value.GetType()));
                case "Boolean":
                    return WriteBool(value);
                case "String":
                    return '\"' + (string)value + '\"';
                default:
                    if(IsNum(type)) {
                        return NumConverter(value, type).ToString();
                    }
                    else if(IsArray(value)) {
                        return ReadArray(value);
                    }

                    else {
                        return ReadObject(value);
                    }
            }
        }
        public static string SerializeObject(object obj) {
            return ReadObject(obj);
        }

        public static string SerializeArray(object obj) {
            return ReadArray(obj);
        }



    }
}
