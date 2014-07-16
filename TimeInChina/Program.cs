using JA.Clizby;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeInChina
{
    class Config 
    { 
        public TimeSpan? DinnerTime { get; set; } 
    }

    class DinnerTimeMapper : IMapper<Config>
    {
        public string Name
        {
            get { return "DinnerTime"; }
        }

        public void Set(Config target, string value)
        {
            target.DinnerTime = TimeSpan.Parse(value);

            if (target.DinnerTime.Value < TimeSpan.FromHours(14))
                target.DinnerTime = target.DinnerTime.Value.Add(TimeSpan.FromHours(12));
            
            /* If dinner time was before 2:00, they must be using a 12 hour clock instead
             * of a 24 hour clock, so we'll just kindly add twelve hours for them.
             * 
             * This will never, ever produce unintended results. Absolutely not.
             * */
        }

        public bool Validate(Config target)
        {
            if (target.DinnerTime.HasValue)
                return target.DinnerTime.Value >= TimeSpan.FromHours(14)     // dinner can't be before 2:00 PM
                    && target.DinnerTime.Value <= TimeSpan.FromHours(22);    // dinner can't be after 10:00 PM

            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var config = new OptionReader<Config>(new DinnerTimeMapper()).Parse(args);

            if (config.DinnerTime.HasValue && DateTime.Now.Hour == config.DinnerTime.Value.Hours)
                Console.WriteLine("Dinner time!");

            else Console.WriteLine("The current time in China is: {0}", DateTime.UtcNow.AddHours(8).TimeOfDay);
        }
    }
}