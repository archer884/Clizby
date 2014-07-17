using JA.Clizby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloGuys
{
    class Config
    {
        public IEnumerable<string> Names { get; set; }
        public bool Greet { get; set; }
    }

    class ConfigNameMapper : IMapper<Config>
    {
        private IList<string> _names = new List<string>();

        public string Name { get { return "Names"; } }
        
        public void Set(Config target, string value)
        {
            if (target.Names == null)
                target.Names = _names;

            _names.Add(value);
        }

        public bool Validate(Config target)
        {
            return target.Names != null
                && target.Names.Any();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var config = ReadConfiguration(args);

            if (config.Greet)
                foreach (var name in config.Names)
                    Console.WriteLine("Hey, {0}!", name);
        }

        static Config ReadConfiguration(string[] args)
        {
            return new OptionReader<Config>(new ConfigNameMapper())
            {
                Aliases = new Dictionary<string, string>()
                {
                    { "n", "names" },
                    { "name", "names" },
                }
            }.Parse(args);
        }
    }
}
