using System;
using System.ComponentModel;

namespace JA.Clizby
{
    public class Parameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
        
        public Parameter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public object Read(Type type)
        {
            return Read(value => TypeDescriptor.GetConverter(type).ConvertFromString(value), type);
        }

        public T Read<T>()
        {
            return Read(value => (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value), null);
        }

        public T Read<T>(Func<string, T> converter)
        {
            return Read(converter, null);
        }

        private T Read<T>(Func<string, T> converter, Type type = null)
        {
            string readValue = Value;
            
            Type targetType = type ?? typeof(T);
            if (targetType == typeof(bool) && (String.IsNullOrWhiteSpace(Value) || Value.StartsWith("-") || Value.StartsWith("/")))
                readValue = "true";

            return converter(readValue);
        }
    }
}
