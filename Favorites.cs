public class Favorite : KeyValueItem<string, string>
{
    public Favorite()
    {
    }
    public Favorite(string key, string value) : base(key, value)
    {

    }
    public Favorite(KeyValueItem<string, string> kvi) : base(kvi)
    {

    }
}

public class Favorites : Map<string, string, Favorite>
{
}