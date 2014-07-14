using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JA.Clizby
{
    public class Mapper<T>
    {
        public string Name { get; set; }
        public Action<string, T> Transform { get; set; }
        public Func<T, bool> Validator { get; set; }
        public bool Required { get; set; }

        public Mapper(
            string name,
            Action<string, T> transform = null,
            Func<T, bool> validator = null,
            bool required = false)
        {
            Name = name;
            Transform = transform;
            Validator = validator;
            Required = required;
        }

        public T Set(string value, T item)
        {
            Transform(value, item);
            return item;
        }

        public bool Validate(T item)
        {
            if (!typeof(T).GetProperties().Select(p => p.Name).Contains(Name))
                throw new ArgumentNullException("Missing property: " + Name);

            if (Validator != null)
                return Validator(item);

            return true;
        }
    }
}
