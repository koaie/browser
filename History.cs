using System;
using System.Linq;

using Gtk;
using UI = Gtk.Builder.ObjectAttribute;


/// <summary>
/// Class <c>History</c> models to a url map.
/// </summary>
public class History : Map<string, string, Url>
{

}

public class HistoryWindow : Window
{
    History history;
    TabWindow tab; // Caller window
    [UI] ListStore listStore; // UI table
    TreeIter selectedRow; // Currently selected row

    public HistoryWindow(TabWindow tab, History history) : this(new Builder("History.glade"))
    {
        // Set caller window
        this.tab = tab;
        this.history = history;
    }

    // Link gtk builder to glade
    private HistoryWindow(Builder builder) : base(builder.GetRawOwnedObject("HistoryWindow"))
    {
        builder.Autoconnect(this); // Link history glade to builder
        this.ShowAll(); // Show widgets
    }

    private void onDraw(object sender, DrawnArgs a)
    {
        if (history.Count == 0)
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
            // Convert date time string to unix time
            var time = listStore.GetValue(first, 1).ToString();
            var dateTime = DateTime.Parse(time);
            var timeOffset = new DateTimeOffset(dateTime).ToUniversalTime();
            var epoch = timeOffset.ToUnixTimeSeconds();
            var newestStoreTime = Int64.Parse(epoch.ToString());

            var newestDb = history[history.Count - 1];
            var newestDbTime = Int64.Parse(newestDb.key);

            if (newestStoreTime < newestDbTime)
            {
                this.CreateModel();
            }
        }
    }

    // On history window exit
    protected override bool OnDeleteEvent(Gdk.Event evt)
    {
        this.Destroy();
        return true;
    }

    private ListStore CreateModel()
    {
        // Get all History instances by ascending order 
        var res = history.get();
        res.Reverse();
        if (res.Any())
        {
            listStore.Clear();
            // Populate store by descending order
            foreach (Url h in res)
            {
                try
                {
                    var time = DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(h.key));
                    listStore.AppendValues(h.value, time.ToString());
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
        row.GetSelected(out model, out this.selectedRow);
    }

    private void deleteHistoryActivate(object sender, EventArgs a)
    {
        if (!listStore.IterIsValid(selectedRow))
        {
            return;
        }
        try
        {
            // Convert datetime string to epoch time
            var time = listStore.GetValue(selectedRow, 1).ToString();
            var dateTime = DateTime.Parse(time);
            var timeOffset = new DateTimeOffset(dateTime).ToUniversalTime();
            var epoch = timeOffset.ToUnixTimeSeconds();

            history.delete(epoch.ToString());
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

    private async void visitHistoryNode(object sender, EventArgs a)
    {
        if (!listStore.IterIsValid(selectedRow))
        {
            return;
        }
        var url = listStore.GetValue(selectedRow, 0).ToString();
        try
        {
            await this.tab.loadUrlWithHistory(url);
        }
        catch (ErrorOccurred e)
        {
            Console.WriteLine(e);
        }
        catch (NullReferenceException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
        this.Destroy();
    }
}