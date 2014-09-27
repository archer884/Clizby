using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [Fact]
        public void Test010_EnumerableArgumentsWork()
        {
            var args = new[] { "-n", "Max", "-n", "Tom", "-greet" };
            var options = new OptionReader<HelloWorldOptions>(new HelloWorldOptionsCustomNameMapper())
            {
                Aliases = new Dictionary<string, string>() 
                { 
                    { "name", "names" },
                    { "n", "names" },
                }
            }.Parse(args);

            Assert.Equal(2, options.Names.Count());
            Assert.Equal("Max", options.Names.First());
            Assert.Equal("Tom", options.Names.Last());
            Assert.True(options.Greet);
        }

        [Fact]
        public void Test011_BooleanArgumentsFollowedByArgumentTokensWork()
        {
            var args = new[] { "-n", "Max", "-name", "Tom", "-greet", "-names", "Bob" };
            HelloWorldOptions options = null;

            Assert.DoesNotThrow(() =>
            {
                options = new OptionReader<HelloWorldOptions>(new HelloWorldOptionsCustomNameMapper())
                {
                    Aliases = new Dictionary<string, string>() 
                    { 
                        { "name", "names" },
                        { "n", "names" },
                    }
                }.Parse(args);
            });

            Assert.Equal(3, options.Names.Count());
            Assert.True(new[] { "Max", "Tom", "Bob" }.SequenceEqual(options.Names));
            Assert.True(options.Greet);
        }

        [Fact]
        public void Test012_OptionsAliasesWork()
        {
            var args = new[] { "-n", "Max", "-name", "Tom", "-groot", "-names", "Bob" };
            HelloWorldOptionsWithAliases options = null;
            OptionReader<HelloWorldOptionsWithAliases> reader = new OptionReader<HelloWorldOptionsWithAliases>(new HelloWorldOptionsWithAliasesCustomNameMapper());

            Assert.DoesNotThrow(() =>
            {
                options = reader.Parse(args);
            });

            Assert.Equal(3, reader.Aliases.Count);
            Assert.Equal(3, options.Names.Count());
            Assert.True(new[] { "Max", "Tom", "Bob" }.SequenceEqual(options.Names));
            Assert.True(options.Greet);
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

        public class HelloWorldOptions
        {
            public virtual IEnumerable<string> Names { get; set; }
            public virtual bool Greet { get; set; }
        }

        public class HelloWorldOptionsCustomNameMapper : IMapper<HelloWorldOptions>
        {
            private IList<string> _names = new List<string>();

            public string Name { get { return "Names"; } }

            public void Set(HelloWorldOptions target, string value)
            {
                if (target.Names == null)
                    target.Names = _names;

                _names.Add(value);
            }

            public bool Validate(HelloWorldOptions target)
            {
                return target.Names != null
                    && target.Names.Any();
            }
        }

        public class HelloWorldOptionsWithAliases
        {
            [Alias("name", "n")]
            public IEnumerable<string> Names { get; set; }

            [Alias("Groot")]
            public bool Greet { get; set; }
        }

        public class HelloWorldOptionsWithAliasesCustomNameMapper : IMapper<HelloWorldOptionsWithAliases>
        {
            private IList<string> _names = new List<string>();

            public string Name { get { return "Names"; } }

            public void Set(HelloWorldOptionsWithAliases target, string value)
            {
                if (target.Names == null)
                    target.Names = _names;

                _names.Add(value);
            }

            public bool Validate(HelloWorldOptionsWithAliases target)
            {
                return target.Names != null
                    && target.Names.Any();
            }
        }
        #endregion
    }
}
