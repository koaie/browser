using System.ComponentModel.DataAnnotations;

/// <summary>
/// Class <c>KeyValueItem</c> models a key value based objet, where the key is the primary key in entityframework.
/// </summary>
public class KeyValueItem<K, V>
{
    [Key]
    public K key { get; set; }
    public V value { get; set; }
    public KeyValueItem()
    {

    }
    public KeyValueItem(KeyValueItem<K, V> kvi)
    {
        this.key = kvi.key;
        this.value = kvi.value;
    }
    public KeyValueItem(K key, V value)
    {
        this.key = key;
        this.value = value;
    }
}
