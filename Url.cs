/// <summary>
/// Class <c>Url</c> represents a model of a url key value item of types string string,
/// the key is unix time
/// the value is uri string
/// </summary>
public class Url : KeyValueItem<string, string>
{
    public Url()
    {
    }

    public Url(string key, string value) : base(key, value)
    {

    }
    public Url(KeyValueItem<string, string> kvi) : base(kvi)
    {

    }
}