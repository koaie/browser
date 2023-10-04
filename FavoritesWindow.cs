using System;
using System.Linq;

using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

public class FavoritesWindow : Window
{
    private Favorites favs;
    private TabWindow tab; // Caller window
    [UI] ListStore listStore = null; // UI table
    [UI] private Entry urlEntry = null;
    [UI] private Entry aliasEntry = null;
    private TreeIter selectedRow; // Currently selected row

    private bool init = false;

    public FavoritesWindow(TabWindow tab, Favorites favs) : this(new Builder("Favorites.glade"))
    {
        this.favs = favs;
        this.tab = tab;
    }

    // Link gtk builder to glade
    private FavoritesWindow(Builder builder) : base(builder.GetRawOwnedObject("FavoritesWindow"))
    {
        builder.Autoconnect(this); // Link history glade to builder
        this.ShowAll(); // Show widgets
    }

    private void onTreeDraw(object sender, DrawnArgs a)
    {
        if (favs.Count == 0)
        {
            return;
        }
        TreeIter first;
        listStore.GetIterFirst(out first);

        if (!listStore.IterIsValid(first))
        {
            this.CreateModel(); // Populate store
        }
        else
        {
            var newestStoreUrl = listStore.GetValue(first, 0);
            var newestDb = favs[favs.Count - 1];

            if (!newestStoreUrl.ToString().Equals(newestDb.key))
            {
                this.CreateModel();
            }
        }
    }

    private void onDraw(object sender, DrawnArgs a)
    {
        if (!init)
        {
            this.init = !init;
            urlEntry.Text = tab.Url;
        }
    }

    // On history window exit
    protected override bool OnDeleteEvent(Gdk.Event evt)
    {
        this.Destroy();
        return true;
    }

    private delegate SelectionGetHandler selectionHandler();

    private ListStore CreateModel()
    {
        // Get all History instances by ascending order 
        var res = favs.get();
        if (res.Any())
        {
            res.Reverse();
            listStore.Clear();
            // Populate store by descending order
            foreach (Favorite f in res)
            {
                try
                {
                    listStore.AppendValues(f.key, f.value);
                }
                catch (FormatException)
                {
                    // Invalid database format
                    MessageDialog md = new MessageDialog(this,
 DialogFlags.DestroyWithParent, MessageType.Error,
 ButtonsType.Close, "Invalid database format, please reinstall.");
                    md.Run();
                    md.Destroy();
                }
            }
        }
        return listStore;
    }

    protected async void visitUrl(object sender, RowActivatedArgs a)
    {
        try
        {
            await this.tab.loadUrlWithHistory(listStore.GetValue(selectedRow, 0).ToString());
        }
        catch (ErrorOccurred e)
        {
            Console.WriteLine(e);
        }
        this.Destroy();
    }

    protected void setSelected(object sender, EventArgs a)
    {
        TreeSelection row = (sender as TreeView).Selection;
        ITreeModel model;

        // Set selectedRow;
        if (row.GetSelected(out model, out this.selectedRow))
        {
            this.urlEntry.Text = listStore.GetValue(selectedRow, 0).ToString();
            this.aliasEntry.Text = listStore.GetValue(selectedRow, 1).ToString();
        }
    }

    private void deleteActivate(object sender, EventArgs a)
    {
        try
        {

            // Find first instance of History instance
            var key = listStore.GetValue(selectedRow, 0).ToString();

            // Remove from DB and save changes
            favs.delete(key);

            // Remove row from store.
            listStore.Remove(ref selectedRow);
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private async void visitNodeUrl(object sender, EventArgs a)
    {
        var url = listStore.GetValue(selectedRow, 0).ToString();
        try
        {
            await this.tab.loadUrlWithHistory(url);
        }
        catch (ErrorOccurred e)
        {
            Console.WriteLine(e);
        }
        this.Destroy();
    }

    private void addNodeUrl(object sender, EventArgs a)
    {
        var key = urlEntry.Text;
        var alias = aliasEntry.Text;

        if (favs.contains(key) || string.IsNullOrEmpty(alias))
        {
            return;
        }
        var fav = new Favorite(key, alias);
        favs.add(fav);
        this.CreateModel();
    }

    private void editNodeFav(object sender, EventArgs a)
    {
        if (!listStore.IterIsValid(selectedRow))
        {
            return;
        }
        var url = listStore.GetValue(selectedRow, 0).ToString();
        if (!favs.contains(url))
        {
            return;
        }
        var alias = aliasEntry.Text;
        favs.update(url, alias);
        this.CreateModel();
    }
}