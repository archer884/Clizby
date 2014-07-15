using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JA.Clizby
{
    public interface IMapper<T>
    {
        string Name { get; }
        void Set<T>(T target, string value);
        bool Validate(T target);
    }

    public class Mapper<T, TProperty> : IMapper<T>
    {
        bool _hasBeenSet { get; set; }

        public Expression<Func<T, TProperty>> PropertyAccessor { get; set; }
        public Func<string, TProperty> Transform { get; set; }
        public Func<TProperty, bool> Validator { get; set; }
        public bool Required { get; set; }

        private PropertyInfo Property
        {
            get { return (PropertyInfo)((MemberExpression)PropertyAccessor.Body).Member; }
        }

        public string Name
        {
            get { return Property.Name; }
        }

        public Mapper(
            Expression<Func<T, TProperty>> propertyAccessor,
            Func<string, TProperty> transform = null,
            Func<TProperty, bool> validator = null,
            bool required = false)
        {
            PropertyAccessor = propertyAccessor;
            Transform = transform;
            Validator = validator;
            Required = required;
            _hasBeenSet = false;
        }

        public void Set<T>(T target, string value)
        {
            Property.SetValue(target, 
                Transform == null ? TypeDescriptor.GetConverter(typeof(TProperty)).ConvertFromString(value) : Transform(value));

            _hasBeenSet = true;
        }

        public bool Validate(T target)
        {
            if (Required && !_hasBeenSet)
                throw new ArgumentNullException(
                    String.Format("Required property not set: {0}", Property.Name));

            if (Validator != null)
                return Validator((TProperty)Property.GetValue(target));

            return true;
        }
    }
}
