using easyJSON.jsonValue;
using System.Reflection;

namespace easyJSON {
    public static class jsonParser {
        private static string SkipWhiteSpaces(string json) {
            int i = 0;
            while(true) {
                switch(json[i]) {
                    case ' ':
                        i++;
                        continue;
                    case '\t':
                        i++;
                        continue;
                    case '\n':
                        i++;
                        continue;
                    case '\r':
                        i++;
                        continue;
                    case '\0':
                        i++;
                        continue;
                    default:
                        return json.Substring(i);
                }
            }
        }
        public static object NumConverter(Int128 num, Type toField) {
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
                    return num;
                case "UInt128":
                    return (UInt128)num;
                case "Single":
                    return (float)num;
                case "Double":
                    return num;
                case "Decimal":
                    return (decimal)num;
                case "Nullable`1":
                    return NumConverter(num, Nullable.GetUnderlyingType(toField));
                default:
                    throw new Exception();
            }
        }
        public static object NumConverter(Double num, Type toField) {
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
                    return num;
                case "Decimal":
                    return (decimal)num;
                case "Nullable`1":
                    return NumConverter(num, Nullable.GetUnderlyingType(toField));
                default:
                    throw new Exception();
            }
        }
        private static void SetValues(FieldInfo toField, string TypeName, string key, jsonObject value, TypedReference reference) {
            if(TypeName == "Nullable`1") {
                if(value.values[key] == null) {
                    toField.SetValueDirect(reference, null);
                }
                else {
                    SetValues(toField, Nullable.GetUnderlyingType(toField.FieldType).Name, key, value, reference);
                }
            }
            else if(value.values[key] == null) {
                throw new Exception();
            }
            else if(value.values[key].GetType().Name == "Int128") {
                Int128 num = (Int128)value.values[key];
                if(TypeName == "Boolean") {
                    if(num == 0) {
                        toField.SetValueDirect(reference, false);
                    }
                    else {
                        toField.SetValueDirect(reference, true);
                    }
                }
                else {
                    toField.SetValueDirect(reference, NumConverter(num, toField.FieldType));
                }
            }
            else if(value.values[key].GetType().Name == "Double") {
                double num = (double)value.values[key];
                if(TypeName == "Boolean") {
                    if(num == 0) {
                        toField.SetValueDirect(reference, false);
                    }
                    else {
                        toField.SetValueDirect(reference, true);
                    }
                }
                else {
                    toField.SetValueDirect(reference, NumConverter(num, toField.FieldType));
                }
            }
            else if(value.values[key].GetType().Name == "jsonObject") {
                var instance = Activator.CreateInstance(toField.FieldType);
                foreach(var fromField in instance.GetType().GetFields()) {
                    var toFieldsField = instance.GetType().GetField(
                        fromField.Name, BindingFlags.Public | BindingFlags.Instance);

                    SetValues(toFieldsField, instance, (jsonObject)value.values[key]);
                }
                toField.SetValueDirect(reference, instance);
            }
            else if(value.values[key].GetType().Name == "jsonObjectArray") {
                Type t = toField.FieldType;
                Type t2 = t.GetElementType();
                if(t2 == null) {
                    throw new Exception();
                }
                jsonObjectArray array = (jsonObjectArray)value.values[key];
                var valueArray = Array.CreateInstance(t2, array.array.Count);
                if(array.isPrimitive) {
                    for(int i = 0; i < array.array.Count; i++) {
                        if(t2.Name == "String") valueArray.SetValue("", i);
                        else valueArray.SetValue(Activator.CreateInstance(t2), i);
                        var item = array.array[i];
                        if(item == null) {
                            valueArray.SetValue(null, i);
                        }
                        else {
                            switch(item.GetType().Name) {
                                case "Int128":
                                    valueArray.SetValue(NumConverter((Int128)item, t2), i);
                                    continue;
                                case "Double":
                                    valueArray.SetValue(NumConverter((Double)item, t2), i);
                                    continue;
                                case "Boolean":
                                    valueArray.SetValue((bool)item, i);
                                    continue;
                                case "String":
                                    valueArray.SetValue((string)item, i);
                                    continue;
                                default: throw new Exception();
                            }
                        }
                    }
                }
                else {
                    for(int i = 0; i < array.array.Count; i++) {
                        valueArray.SetValue(Activator.CreateInstance(t2), i);
                        foreach(var fromField in valueArray.GetType().GetElementType().GetFields()) {
                            var toFieldsField = valueArray.GetType().GetField(
                                fromField.Name, BindingFlags.Public | BindingFlags.Instance);

                            SetValues(fromField, valueArray.GetValue(i), (jsonObject)array.array[i]);
                        }
                    }
                }
                toField.SetValueDirect(reference, valueArray);
            }
            else {
                toField.SetValueDirect(reference, value.values[key]);
            }
        }
        static void SetValues(FieldInfo toField, object instance, jsonObject value) {
            if(toField != null) {
                foreach(string key in value.values.Keys) {
                    if(key.ToUpper() == toField.Name.ToUpper()) {
                        TypedReference reference = __makeref(instance);

                        SetValues(toField, toField.FieldType.Name, key, value, reference);
                    }
                }
            }
        }
        public static T ParseObject<T>(string json) {
            jsonObject value = (jsonObject)Deserialize(json, true);
            T instance = (T)Activator.CreateInstance(typeof(T));
            foreach(var fromField in instance.GetType().GetFields()) {

                var toField = instance.GetType().GetField(
                    fromField.Name, BindingFlags.Public | BindingFlags.Instance);

                SetValues(toField, instance, value);
            }
            return instance;
        }

        private static object ParseArrayOfObjects(jsonObjectArray value, Type type) {
            Type t2 = type.GetElementType();
            var valueArray = Array.CreateInstance(t2, value.array.Count);
            for(int i = 0; i < value.array.Count; i++) {
                valueArray.SetValue(Activator.CreateInstance(t2), i);
                foreach(var fromField in valueArray.GetType().GetElementType().GetFields()) {
                    var toField = valueArray.GetType().GetField(
                        fromField.Name, BindingFlags.Public | BindingFlags.Instance);

                    SetValues(fromField, valueArray.GetValue(i), (jsonObject)value.array[i]);
                }
            }
            return valueArray;
        }

        public static object ParseArray<T>(string json) {
            jsonObjectArray value = (jsonObjectArray)Deserialize(json, true);
            if(value.array[0].GetType().Name == "jsonObject") {
                return ParseArrayOfObjects(value, typeof(T));
            }
            else {
                return ParseArrayOfObjects(value, typeof(T));
            }
        }

        public static T Deserialize<T>(string json) {
            json = SkipWhiteSpaces(json);
            if(json[0] == '[') {
                if(typeof(T).IsArray) {
                    return (T)ParseArray<T>(json);
                }
                throw new Exception();
            }
            else if(json[0] == '{') {
                return ParseObject<T>(json);
            }
            else {
                if(typeof(T).IsPrimitive) {
                    return (T)Deserialize(json, true);
                }
                throw new Exception();
            }
        }
        //public static object ParseValue(string json) {


        //}
        public static object Deserialize(string json, bool IsSkipedWS = false) {
            if(!IsSkipedWS) json = SkipWhiteSpaces(json);
            Deserializer serializer = new();
            if(json[0] == '[') {
                return (jsonObjectArray)serializer.DeserializeArray(new StringReader(json));
            }
            else if(json[0] == '{') {
                return (jsonObject)serializer.Deserialize(new StringReader(json));
            }
            else {
                var result = (jsonObject)serializer.Deserialize(new StringReader(json));
                return result.values["Single Object"];
            }
        }

        public static string Serialize(object obj) {
            Type type = obj.GetType();
            string json;
            if(type.IsArray) {
                json = Serializer.SerializeArray(obj);
            }
            else {
                json = Serializer.SerializeObject(obj);
            }


            return json;
        }
    }
}