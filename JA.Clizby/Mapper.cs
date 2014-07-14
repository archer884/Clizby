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
        protected bool _used { get; set; }

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
            _used = false;
            Name = name;
            Transform = transform;
            Validator = validator;
            Required = required;
        }

        public T Set(string value, T item)
        {
            _used = true;
            Transform(value, item);
            return item;
        }

        public bool Validate(T item)
        {
            if (Validator != null)
                return Validator(item);

            if (Required && !_used)
                throw new ArgumentNullException(
                    String.Format("Required property not set: {0}", this.Name));

            return true;
        }
    }
}
