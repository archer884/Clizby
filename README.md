Clizby
======
Clizby is all about easily-configured command-line argument parsing (and pretty much nothing else). Although I guess it's also about Psych, which was an awesome show that you should watch sometime. Bonus points if you already know who Clizby is.

Stable builds are [available on Nuget](https://www.nuget.org/packages/Clizby), but the latest version is always right here.

- Version 1.0.2: Added an attribute called Alias that you can stick on your fields and properties in order to avoid my (heinous) Alias api.
    - You know, the one that consisted basically of creating a dictionary where the keys were aliases and all the values were the same damn property name?
    - To be honest, it still works that way internally, but the OptionReader(T) is now perfectly capable of building its own dictionary.

- Version 1.0.1.1: Lots of changes for this and other earlier versions. I can't remember them all (probably), so here are the highlights...
    - Fixed nuget package so that it no longer makes you download a pointless testing library.
    - Added new constructor to make creating OptionReaders with mappers a little less annoying.
    - Added IMapper(T) interface so that you can use a totally custom mapper class if you want.
    - Fixed/update documentation on advanced usage (the tests were right, but the example in this document was wack!).
    - Modified the built-in mapper class so that it now asks you to provide a selector expression instead of simply the name of a property (which is just honestly way cooler, in my opinion).
    - Changed transform and validation functions used by the provided mapper class so that they accept function rather than action delegates (because that just feels cleaner to me).

> Note: Those last two above were breaking changes. Sorry. No way around that. :(

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
        [Alias("Nombre")]
        public int Name { get; set; }
        
        [Alias("Greet", "Salutate")]
        public bool SayHello { get; set; }
    }
    
...then when you actually run the app, any of the following will work fine:

- `app.exe -Name John -SayHello` or `app.exe -Name John -Greet true`
- `app.exe -Nombre John -Salutate false`
- `app.exe John true` or `app.exe John false`
- `app.exe John -SayHello false`

Boolean properties are treated as flags (and presumed `true`) unless a value of `false` is also provided. Positional vs. named argument parsing is similar to what you find in C# itself, so positional arguments must come first, but named arguments can be included afterward. 

For a clearer picture of what is supported, you can take a peek at the test suite. The `args` variable in each test is exactly what your application will receive in the Main method, so it's a pretty good analog for real usage.

## Advanced usage ##

Clizby uses a strongly-typed, generic Parse(T) method to do most of its magic. Reflection allows us to see the name and type of each property. We then use a default type converter to transform text into data in order to build up an options object. This works pretty well in a lot of circumstances, but it is also possible to do additional processing on inputs before using them.

### Argument Processing ###

Just as an arbitrary example, what if you want your users to type `false` as an argument (for better comprehension), but your application (because you're insane) will only understand values of `0` or `1`?

    var boolConverter = new Mapper<Options, bool>(
        options => options.BooleanFlag, 
        value => bool.Parse(value) ? 1 : 0);
        
    var options = new OptionReader<Options>(boolConverter).Parse(args);
    
> Note: If you look in the tests I've uploaded here, you'll see me using named arguments to create these mappers. That isn't required; the order is "property selector", "transform", "validator", "required", and you can just stick them in order, but--honestly--it's a little easier to maintain if you have them labeled, isn't it?

In this sample, we create a new Mapper, an object used by Clizby to map command line arguments to values on the output object. The Mapper object has other uses (the most basic being that of marking a given property as required), but in this case we use it to transform a boolean value into an integer by means of a lambda.

### Enumerable Arguments ###

The majority of my use cases involve single options and flags, but I do have one project that requires me to call an application like so: `app.exe -arg value1 -arg value2 -arg value3`, which is a use case that--at first blush--Clizby does not seem to support. Now, it *is* true that Clizby doesn't have a built-in mapper type for enumerable arguments, but I'm still using Clizby to handle this effort at work by creating a mapper class to handle the appropriate properties. 

This class is taken from the HelloGuys sample project you can view on this repository:

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
    
It's a pretty simple class: the only thing it contains other than the bare minimum required to implement the IMapper(T) interface is the `IList(T) _names`, which it uses to store all the names that come through as arguments when the arguments are being read. `_names` is then set as the backing store for the `.Names` property on the `Config` object. This pattern saved me at last a couple hours vs. trying to add the same functionality into the base `OptionReader(T)`. :)

## Roadmap ##
Clizby is, at least for my purposes, functionally complete (you'll notice that I totally gave it the version number 1.0 already). It's actually even being used right in some of my own projects (Nuget makes that a lot easier. :) ) But that doesn't mean I'm *quite* finished yet. My future plans include...

- Moar interfaces. Why should you use my objects when you can spend a lot of time making your own?
    - I'm totally serious about this: I feel like a good concept for an IEnumerableMapper(T) has begun to take shape and that will probably be my next update.

- Better handling for aliases and shorthand property names. Currently, these are... Slightly more case-sensitive than other property names.
    - Still looking into this. The Alias property itself is part one of my solution for this, but I need to do a little more research around "case sensitive."

> ### Parting thoughts ###
> This is still a personal project and pretty much 100% of all development on it is driven by dogfooding. Of course, my needs are limited to things that *I* want to do, so if there's something missing and you just can't believe I haven't added it yet, feel free to tell me. :)
>
> Also, for an example of some heavy dogfooding, check out [HashServer](https://github.com/archer884/HashServer), a project designed to help me stop wasting room on my file server (and, apparently, as a test of just how far I can push Clizby's design without totally screwing everything up). The cool part is [here](https://github.com/archer884/HashServer/blob/master/HashServer/HashServerConfig.cs).
