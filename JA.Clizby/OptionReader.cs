using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JA.Clizby
{
    public class OptionReader<T> where T : new()
    {
        public IDictionary<string, string> Aliases { get; set; }
        public IDictionary<string, IMapper<T>> Mappers { get; set; }
        public IDictionary<string, PropertyInfo> Properties { get; set; }

        public OptionReader()
        {
            Aliases = new Dictionary<string, string>();
            Mappers = new Dictionary<string, IMapper<T>>();
        }

        /// <summary>
        /// Creates a new OptionReader the provided mappers.
        /// </summary>
        public OptionReader(IEnumerable<IMapper<T>> mappers)
            : this()
        {
            Mappers = mappers.ToDictionary(m => m.Name);
        }

        /// <summary>
        /// Creates a new OptionReader the provided mappers.
        /// </summary>
        public OptionReader(params IMapper<T>[] mappers)
            : this()
        {
            Mappers = mappers.ToDictionary(m => m.Name);
        }

        /// <summary>
        /// Parses command line options, returning a new instance of T
        /// containing the processed arguments.
        /// </summary>
        /// <typeparam name="T">Type to be created.</typeparam>
        /// <param name="argsCollection">Arguments to be processed.</param>
        /// <returns>Instance of T</returns>
        public T Parse(IEnumerable<string> argsCollection)
        {
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name);
            var options = ApplyPositionalArguments(new T(), argsCollection);

            var setProperty = false;
            var propertyName = (string)null;

            foreach (var value in argsCollection.Select(GetOptionMapping))
            {
                if (setProperty && properties.ContainsKey(propertyName))
                {
                    if (Mappers.ContainsKey(propertyName))
                        Mappers[propertyName].Set(options, value);

                    else properties[propertyName].SetValue(options, TypeDescriptor.GetConverter(properties[propertyName].PropertyType).ConvertFromString(value));

                    setProperty = false;
                    continue;
                }

                if (value.StartsWith("-") || value.StartsWith("/"))
                {
                    setProperty = true;
                    propertyName = GetPropertyName(value);

                    // If the next thing to come up is FALSE, this will be reset
                    if (properties[propertyName].PropertyType == typeof(bool))
                        properties[propertyName].SetValue(options, true);

                    continue;
                }
            }

            return Validate(options);
        }

        private T ApplyPositionalArguments(T options, IEnumerable<string> argsCollection)
        {
            var positionalArguments = argsCollection.TakeWhile(arg => !arg.StartsWith("/") && !arg.StartsWith("-")).ToList();
            var positionalProperties = typeof(T).GetProperties();

            for (int i = 0; i < positionalArguments.Count; i++)
            {
                if (Mappers.ContainsKey(positionalProperties[i].Name))
                    Mappers[positionalProperties[i].Name].Set(options, positionalArguments[i]);

                else positionalProperties[i].SetValue(options, TypeDescriptor.GetConverter(positionalProperties[i].PropertyType).ConvertFromString(positionalArguments[i]));
            }

            return options;
        }

        private T Validate(T options)
        {
            var validationResults = Mappers.Values.Select(mapper => new
            {
                Mapper = mapper.Name,
                Result = mapper.Validate(options),
            }).ToList();

            if (validationResults.Any(mapper => !mapper.Result))
                throw new ArgumentException(
                    String.Format("The following mapper(s) failed to validate: \n{0}",
                        String.Join(Environment.NewLine, validationResults.Select(result => result.Mapper))));

            return options;
        }

        private string GetPropertyName(string propertyKey)
        {
            var key = propertyKey.TrimStart(new[] { '-', '/' });
            return char.ToUpper(key[0]) + key.Substring(1);
        }

        private string GetOptionMapping(string option)
        {
            if (option.StartsWith("-") || option.StartsWith("/"))
            {
                var trimmedOption = option.TrimStart(new[] { '-', '/' });

                if (Aliases.ContainsKey(trimmedOption))
                    return option.Replace(trimmedOption, Aliases[trimmedOption]);

                var similarKey = typeof(T).GetProperties().SingleOrDefault(property => property.Name.StartsWith(trimmedOption.Substring(0, 1), StringComparison.OrdinalIgnoreCase));
                if (similarKey != null)
                    return option.Replace(trimmedOption, similarKey.Name);
            }
            return option;
        }
    }
}
