using System.Collections.Generic;

public class Option : KeyValueItem<string, string>
{
    public Option()
    {
    }
    public Option(string key, string value) : base(key, value)
    {

    }
    public Option(KeyValueItem<string, string> kvi) : base(kvi)
    {

    }
}

public class Options : Map<string, string, Option>
{
    public List<Option> loadDefaults()
    {

        List<Option> defaults = new List<Option>() {
                new Option("homePage","https://www.hw.ac.uk")
                };
        return this.add(defaults);
    }
}