using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JA.Clizby
{
    public interface IMapper<T>
    {
        string Name { get; }
        void Set(T target, string value);
        bool Validate(T target);
    }

    public abstract class MapperBase<T, TProperty> : IMapper<T>
    {
        protected bool _hasBeenSet = false;
        public string Name { get { return Property.Name; } }
        protected PropertyInfo Property { get { return (PropertyInfo)((MemberExpression)PropertyAccessor.Body).Member; } }

        public bool Required { get; set; }
        public Expression<Func<T, TProperty>> PropertyAccessor { get; set; }

        public MapperBase(
            bool required,
            Expression<Func<T, TProperty>> propertyAccessor)
        {
            Required = required;
            PropertyAccessor = propertyAccessor;
        }

        public abstract void Set(T target, string value);
        public abstract bool Validate(T target);

        protected static object ParseValue<TObject>(Func<string, TObject> parse, string value)
        {
            if (parse == null)
            {
                return TypeDescriptor.GetConverter(typeof(TObject)).ConvertFromString(value);
            }
            else
            {
                return parse(value);
            }
        }
    }

    public class EnumerableMapper<T, TProperty, TElement> : MapperBase<T, TProperty>
    {
        List<TElement> _values = new List<TElement>(); 
        public Func<string, TElement> Transform { get; set; }
        public Func<TElement, bool> Validator { get; set; }

        public EnumerableMapper(
            Expression<Func<T, TProperty>> propertyAccessor,
            Func<string, TElement> transform = null,
            Func<TElement, bool> validator = null,
            bool required = false)
            : base(required, propertyAccessor)
        {
            PropertyAccessor = propertyAccessor;
            Transform = transform;
            Validator = validator;
        }

        public override void Set(T target, string value)
        {
            // The _propertyValues list is initialized when the mapper
            // is created, but the property on the target object is not
            // set until either Set or Validate is called.
            if (Property.GetValue(target) == null)
                Property.SetValue(target, _values);

            // can't use `as` because the enduser may not be using a class.
            _values.Add((TElement)ParseValue(Transform, value));
        }

        public override bool Validate(T target)
        {
            if (Required && !_hasBeenSet)
                throw new ArgumentNullException(
                    String.Format("Required property not set: {0}", Property.Name));

            if (Validator != null)
                return _values.All(value => Validator(value));

            return true;
        }
    }

    public class Mapper<T, TProperty> : MapperBase<T, TProperty>
    {
        public Func<string, TProperty> Transform { get; set; }
        public Func<TProperty, bool> Validator { get; set; }

        public Mapper(
            Expression<Func<T, TProperty>> propertyAccessor,
            Func<string, TProperty> transform = null,
            Func<TProperty, bool> validator = null,
            bool required = false)
            : base(required, propertyAccessor)
        {
            PropertyAccessor = propertyAccessor;
            Transform = transform;
            Validator = validator;
            Required = required;
            _hasBeenSet = false;
        }

        public override void Set(T target, string value)
        {
            Property.SetValue(target, ParseValue(Transform, value));
            _hasBeenSet = true;
        }

        public override bool Validate(T target)
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
