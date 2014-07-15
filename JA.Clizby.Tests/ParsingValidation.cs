using System;
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
            var nameMapper = new Mapper<Options>("Name", required: true)
            {
                Transform = (value, item) => item.Name = value.ToUpper(),
                Validator = item => !string.IsNullOrWhiteSpace(item.Name),
            };

            var reader = new OptionReader<Options>(new[] { nameMapper });
            var options = reader.Parse(args);

            Assert.Equal("BOB", options.Name);
            Assert.Equal(false, options.TrueFalse);
        }

        [Fact]
        public void Test003_MissingRequiredPropertyThrowsError()
        {
            var args = new[] { "/name", "Bob", "--trueFalse", "false" };
            var dateMapper = new Mapper<Options>("Date", required: true);

            var reader = new OptionReader<Options>(new[] { dateMapper });

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
            var nameCapitalizer = new Mapper<Options>("Name", required: true, transform: (i, o) => o.Name = i.ToUpper());
            var boolInverter = new Mapper<Options>("TrueFalse", required: true, transform: (i, o) => o.TrueFalse = !bool.Parse(i));
            var optionDoubler = new Mapper<Options>("OptionalValue", required: false, transform: (i, o) => o.OptionalValue = int.Parse(i) * 2);

            var options = new OptionReader<Options>(new[] 
            { 
                nameCapitalizer, 
                boolInverter, 
                optionDoubler,
            }).Parse(args);
            
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

        public class Options
        {
            public string Name { get; set; }
            public bool TrueFalse { get; set; }
            public int OptionalValue { get; set; }
        }
    }
}
