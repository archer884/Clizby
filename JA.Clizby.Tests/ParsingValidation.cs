using System;
using System.Diagnostics;
using Xunit;

namespace JA.Clizby.Tests
{
    public class ParsingValidation
    {
        [Fact]
        public void Test001_BasicGenericParsing()
        {
            var args = new[] { "/name", "Bob", "--trueFalse", "false" };

            var reader = new OptionReader<Options>();
            var options = reader.Parse(args);

            Assert.False(options.TrueFalse);
            Assert.Equal("Bob", options.Name);
        }

        [Fact]
        public void Test002_AdvancedGenericParsing()
        {
            var args = new[] { "/name", "Bob", "--trueFalse", "false" };
            var nameMapper = new Mapper<Options, string>(o => o.Name, required: true)
            {
                Transform = value => value.ToUpper(),
                Validator = value => !String.IsNullOrWhiteSpace(value),
            };

            var reader = new OptionReader<Options>(nameMapper);
            var options = reader.Parse(args);

            Assert.Equal("BOB", options.Name);
            Assert.Equal(false, options.TrueFalse);
        }

        [Fact]
        public void Test003_MissingRequiredPropertyThrowsError()
        {
            var args = new[] { "/name", "Bob" };
            var booleanMapper = new Mapper<Options, bool>(o => o.TrueFalse, required: true);

            var reader = new OptionReader<Options>(booleanMapper);

            Assert.Throws<ArgumentNullException>(() =>
            {
                var options = reader.Parse(args);
            });
        }

        [Fact]
        public void Test004_BasicPositionalArgumentsWork()
        {
            var args = new[] { "Bob", "false" };
            var options = new OptionReader<Options>().Parse(args);

            Assert.Equal("Bob", options.Name);
            Assert.Equal(false, options.TrueFalse);
        }

        [Fact]
        public void Test005_AdvancedPositionalArgumentsWork()
        {
            var args = new[] { "Bob", "false", "--optionalValue", "16" };
            var nameCapitalizer = new Mapper<Options, string>(o => o.Name, required: true, transform: value => value.ToUpper());
            var boolInverter = new Mapper<Options, bool>(o => o.TrueFalse, required: true, transform: value => !bool.Parse(value));
            var optionDoubler = new Mapper<Options, int>(o => o.OptionalValue, required: false, transform: value => int.Parse(value) * 2);

            var options = new OptionReader<Options>(
                nameCapitalizer, 
                boolInverter, 
                optionDoubler).Parse(args);
            
            Assert.NotEqual("Bob", options.Name);
            Assert.Equal("BOB", options.Name);
            Assert.Equal(true, options.TrueFalse);
            Assert.Equal(32, options.OptionalValue);
        }

        [Fact]
        public void Test006_ShorthandArgumentsWork()
        {
            var args = new[] { "-n", "Bob" };
            var options = new OptionReader<Options>().Parse(args);

            Assert.Equal("Bob", options.Name);
        }

        [Fact]
        public void Test007_AliasedArgumentsWork()
        {
            var args = new[] { "-n", "Bob" };
            var reader = new OptionReader<Options>();
            reader.Aliases.Add("n", "name");
            var options = reader.Parse(args);

            Assert.Equal("Bob", options.Name);
        }

        [Fact]
        public void Test008_CustomMapperClassesWork()
        {
            var args = new[] { "-n", "Bob" };
            var options = new OptionReader<Options>(new OptionsCustomNameMapper()).Parse(args);

            Assert.Equal(OptionsCustomNameMapper.CorrectName, options.Name);
        }

        [Fact]
        public void Test009_ValidationFailuresPropogate()
        {
            var args = new[] { "-n", "Bob" };
            
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var options = new OptionReader<Options>(new Mapper<Options, string>(
                    o => o.Name,
                    value => value.ToUpper(),
                    value => value.Length > 3,
                    true)).Parse(args);
            });

            Debug.WriteLine(exception.Message);
        }

        #region Classes
        public class Options
        {
            public string Name { get; set; }
            public bool TrueFalse { get; set; }
            public int OptionalValue { get; set; }
        }

        public class OptionsCustomNameMapper : IMapper<Options>
        {
            public const string CorrectName = "Maximus Hardcorion!"; // pronounced "hard-core-ee-on"; the exclamation point is silent

            public string Name
            {
                get { return "Name"; }
            }

            public void Set(Options target, string value)
            {
                target.Name = "Maximus Hardcorion!";
            }

            public bool Validate(Options target)
            {
                return target.Name == "Maximus Hardcorion!";
            }
        } 
        #endregion
    }
}
