
using System;
using System.Linq;
using System.Collections.Generic;


/// <summary>
/// Class <c>Map</c> models a map of key value item of generic types,
/// handles map db <c>Add</c>, <c>Delete</c>, and <c>Update</c> functions.
/// </summary>
public class Map<K, V, C> where C : KeyValueItem<K, V>, new()
{
    protected IDictionary<K, V> map = new Dictionary<K, V>();

    /// <summary>
    /// Index based on key
    /// </summary>
    public V this[K key]
    {
        get { return this.get(key); }
        set { this.update(key, value); }
    }

    /// <summary>
    /// Index based on integer
    /// </summary>
    public C this[int i]
    {
        get { return this.get(i); }
        set { this.update(this.get(i).key, value.value); }
    }

    /// <summary>
    /// Property <c>Count</c> returns amount of elements in map
    /// </summary>
    public int Count
    {
        get { return map.Count(); }
    }

    /// <summary>
    /// Method <c>contains</c> checks if key exists in map
    /// </summary>
    /// <param name="key">key to check</param>
    public bool contains(K key)
    {
        if (this.map.ContainsKey(key))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Method <c>toList</c> converts the map to a list
    /// </summary>
    protected List<C> toList()
    {
        List<C> temp = new List<C>();
        foreach (KeyValuePair<K, V> e in map)
        {
            C c = new C();
            c.key = e.Key;
            c.value = e.Value;
            temp.Add(c);
        }
        return temp;
    }

    /// <summary>
    /// Method <c>toList</c> converts the map to a list
    /// </summary>
    /// <param name="m">map to convert</param>
    protected List<C> toList(IDictionary<K, V> m)
    {
        List<C> temp = new List<C>();
        foreach (KeyValuePair<K, V> e in m)
        {
            C c = new C();
            c.key = e.Key;
            c.value = e.Value;
            temp.Add(c);
        }
        return temp;
    }

    /// <summary>
    /// Method <c>get</c> returns ordered list of map by key
    /// </summary>
    /// <returns>
    /// <c>List{C}</c> ordered list of map by key
    /// </returns>
    public List<C> get()
    {
        List<C> temp = new List<C>();
        var m = this.map.OrderBy(c => c.Key).ToList();
        foreach (KeyValuePair<K, V> e in m)
        {
            C c = new C();
            c.key = e.Key;
            c.value = e.Value;
            temp.Add(c);
        }
        return temp;
    }

    /// <summary>
    /// Method <c>get</c> gets a value from key in map
    /// </summary>
    /// <param name="key">key to get value from</param>
    /// <returns>
    /// <c>V</c> key's value
    /// </returns>
    public V get(K key)
    {
        if (!map.ContainsKey(key))
        {
            // No Key found;
            throw new KeyNotFound();
        }
        return map[key];
    }

    /// <summary>
    /// Method <c>get</c> gets an index from map
    /// </summary>
    /// <param name="i">index of item</param>
    /// <returns>
    /// <c>C</c> object's value
    /// </returns>
    public C get(int i)
    {
        if (this.Count == 0)
        {
            throw new IndexOutOfRangeException();
        }

        // Find first instance of option instance
        var m = map.OrderBy(c => c.Key);
        var v = m.ElementAt(i);
        C c = new C();
        c.key = v.Key;
        c.value = v.Value;
        return c;

        throw new KeyNotFound();
    }

    /// <summary>
    /// Method <c>mapAdd</c> adds to map
    /// </summary>
    /// <param name="c"><c>C</c> object</param>
    /// <returns>
    /// <c>C</c> object added
    /// </returns>
    protected C mapAdd(C c)
    {
        map.Add(c.key, c.value);
        return c;
    }

    /// <summary>
    /// Method <c>mapAdd</c> adds to map
    /// </summary>
    /// <returns>
    /// <c>C</c> Added object
    /// </returns>
    protected C mapAdd(K key, V value)
    {
        C c = new C();
        c.key = key;
        c.value = value;
        map.Add(key, value);

        return c;
    }

    /// <summary>
    /// Method <c>mapAdd</c> adds list to map
    /// </summary>
    /// <returns>
    /// <c>List{C}</c> Added list of C objects
    /// </returns>
    protected List<C> mapAdd(List<C> list)
    {
        foreach (C c in list)
        {
            map.Add(c.key, c.value);
        }
        return this.toList();
    }

    /// <summary>
    /// Method <c>reload</c> reload map with info from db
    /// </summary>
    /// <returns>
    /// <c>List{C}</c> loaded list of C objects
    /// </returns>
    public List<C> reload()
    {
        try
        {
            using (ListContext db = new ListContext())
            {
                this.map.Clear();
                // Get all Option instance
                var res = db.Set<C>().OrderBy(o => o.key).ToList();

                // Populate option cache
                foreach (C c in res)
                {
                    map.Add(c.key, c.value);
                }
            }
        }
        catch (ArgumentNullException e)
        {
            // Null argument
            Console.WriteLine(e.Message);
        }
        return this.toList();
    }

    /// <summary>
    /// Method <c>add</c> adds a list to map and db
    /// </summary>
    /// <returns>
    /// <c>List{C}</c> Added list
    /// </returns>
    public List<C> add(List<C> list)
    {
        try
        {
            using (ListContext db = new ListContext())
            {
                db.Set<C>().AddRange(list);
                db.SaveChanges();
                return this.mapAdd(list);
            }
        }
        catch (ArgumentNullException e)
        {
            // Null argument
            Console.WriteLine(e.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException e)
        {
            // Update original values from the database
            var entry = e.Entries.Single();
            entry.OriginalValues.SetValues(entry.GetDatabaseValues());
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        {
            // Unable to save to DB
            Console.WriteLine(e.Message);
        }
        // Reload database
        return this.reload();
    }

    /// <summary>
    /// Method <c>add</c> adds a item to map and DB
    /// </summary>
    /// <returns>
    /// <c>C</c> Added element
    /// </returns>
    public C add(C c)
    {
        try
        {
            using (ListContext db = new ListContext())
            {
                db.Set<C>().Add(c);
                db.SaveChanges();
                return this.mapAdd(c);
            }
        }
        catch (ArgumentNullException e)
        {
            // Null argument
            Console.WriteLine(e.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        {
            // Unable to save to DB
            Console.WriteLine(e.Message);
        }
        throw new ErrorOccurred();
    }

    /// <summary>
    /// Method <c>update</c> updates an item in map and DB
    /// </summary>
    /// <returns>
    /// <c>C</c> Updated element
    /// </returns>
    public C update(K key, V value)
    {
        if (!map.ContainsKey(key))
        {
            // No Key found;
            throw new KeyNotFound();
        }
        try
        {
            using (ListContext db = new ListContext())
            {
                // Find first instance of option instance
                var c = db.Set<C>().First(o => o.key.Equals(key));

                // Update entity
                map[key] = value;
                c.value = value;

                // Save updated entity
                db.SaveChanges();

                return c;
            }
        }
        catch (ArgumentNullException e)
        {
            // Null argument
            Console.WriteLine(e.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        {
            // Unable to save to DB
            Console.WriteLine(e.Message);
        }
        throw new ErrorOccurred();
    }

    /// <summary>
    /// Method <c>delete</c> deletes an item in map and DB
    /// </summary>
    /// <returns>
    /// <c>C</c> Deleted element
    /// </returns>
    public C delete(K key)
    {
        if (!map.ContainsKey(key))
        {
            // No Key found;
            throw new KeyNotFound();
        }
        try
        {
            using (ListContext db = new ListContext())
            {
                // Find first instance of option instance
                var c = db.Set<C>().First(o => o.key.Equals(key));
                db.Set<C>().Remove(c);

                // Save updated entity
                db.SaveChanges();
                this.map.Remove(key);
                return c;
            }
        }
        catch (ArgumentNullException e)
        {
            // Null argument
            Console.WriteLine(e.Message);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
        {
            // Unable to save to DB
            Console.WriteLine(e.Message);
        }
        throw new ErrorOccurred();
    }
}