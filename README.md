Clizby
======
Clizby is all about easily-configured command-line argument parsing (and pretty much nothing else). Although I guess it's also about Psych, which was an awesome show that you should watch sometime. Bonus points if you already know who Clizby is.

Also available from Nuget: https://www.nuget.org/packages/Clizby

- Version 1.0.1.1: Lots of changes for this and other earlier versions. I can't remember them all (probably), so here are the highlights...
    - Fixed nuget package so that it no longer makes you download a pointless testing library.
    - Added new constructor to make creating OptionReaders with mappers a little less annoying.
    - Added IMapper(T) interface so that you can use a totally custom mapper class if you want.
    - Fixed/update documentation on advanced usage (the tests were right, but the example in this document was wack!).
    - Modified the built-in mapper class so that it now asks you to provide a selector expression instead of simply the name of a property (which is just honestly way cooler, in my opinion).
    - Changed transform and validation functions used by the provided mapper class so that they accept function rather than action delegates (because that just feels cleaner to me).

> These last two are breaking changes. Sorry. No way around that.

- Version 1.0.0.1: Finally got the nuget package working (and tested it on one of my own projects). Pretty cool, huh?

## Usage ##

Clizby is designed primarily to make the developer's job as simple as possible without making the end user's job too much harder as a result. In version 1.0, Clizby understands...

- Positional arguments, e.g. `app.exe arg1 arg2`
- Named arguments, e.g. `app.exe --port 123`
- Shorthand arguments, e.g. `app.exe -p 123`

> Note: Clizby does not care how many `--dashes` you put in front of an argument. Not even a little bit. Never. Seriously. `-p` and `--p` are exactly the same. Kk?

To get the most out of Clizby, create a simple POCO containing your command line options as properties, like...

    public class Options
    {
        public int Name { get; set; }
        public bool SayHello { get; set; }
    }
    
...then when you actually run the app, any of the following will work fine:

- `app.exe -Name John -SayHello` or `app.exe -Name John -SayHello true`
- `app.exe -Name John -SayHello false`
- `app.exe John true` or `app.exe John false`
- `app.exe John -SayHello false`

Boolean properties are treated as flags (and presumed `true`) unless a value of `false` is also provided. Positional vs. named argument parsing is similar to what you find in C# itself, so positional arguments must come first, but named arguments can be included afterward. 

For a clearer picture of what is supported, you can take a peek at the test suite. The `args` variable in each test is exactly what your application will receive in the Main method, so it's a pretty good analog for real usage.

## Advanced usage ##

Clizby uses a strongly-typed, generic Parse(T) method to do most of its magic. Reflection allows us to see the name and type of each property. We then use a default type converter to transform text into data in order to build up an options object. This works pretty well in a lot of circumstances, but it is also possible to do additional processing on inputs before using them.

Just as an arbitrary example, what if you want your users to type `false` as an argument (for better comprehension), but your application (because you're insane) will only understand values of `0` or `1`?

    var boolConverter = new Mapper<Options, bool>(
        "BooleanFlag", 
        value => bool.Parse(value) ? 1 : 0);
        
    var options = new OptionReader<Options>(boolConverter).Parse(args);

In this sample, we create a new Mapper, an object used by Clizby to map command line arguments to values on the output object. The Mapper object has other uses (the most basic being that of marking a given property as required), but in this case we use it to transform a boolean value into an integer by means of a lambda.

## Roadmap ##
Clizby is, at least for my purposes, functionally complete (you'll notice that I totally gave it the version number 1.0 already). It's actually even being used right in some of my own projects (Nuget makes that a lot easier. :) ) But that doesn't mean I'm *quite* finished yet. My future plans include...

- Moar interfaces. Why should you use my objects when you can spend a lot of time making your own?
- Better handling for aliases and shorthand property names. Currently, these are... Slightly more case-sensitive than other property names.