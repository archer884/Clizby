using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JA.Clizby
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
    public class Alias : Attribute
    {
        public IEnumerable<string> Names { get; set; }

        public Alias(params string[] names)
        {
            Names = names;
        }
    }
}
