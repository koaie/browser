using System;
using System.Linq;

using Gtk;
using UI = Gtk.Builder.ObjectAttribute;


public class OptionsWindow : Window
{
    private Options ops; // Options
    [UI] private Entry homePageEntry;

    private bool init = false; // Is home page entry initiated


    public OptionsWindow(Options ops) : this(new Builder("OptionsWindow.glade"))
    {
        this.ops = ops;
    }

    private OptionsWindow(Builder builder) : base(builder.GetRawOwnedObject("OptionsWindow"))
    {
        builder.Autoconnect(this); // Link options glade to builder
        this.ShowAll(); // Show all widgets
    }

    /// <summary>
    /// Sets homepage on first draw of home page entry box
    /// </summary>
    private void onDraw(object sender, DrawnArgs a)
    {
        if (!init)
        {
            this.init = !init;
            homePageEntry.Text = ops["homePage"];
        }
    }

    protected override bool OnDeleteEvent(Gdk.Event evt)
    {
        this.Destroy();
        return true;
    }

    private void onSave(object sender, EventArgs a)
    {
        ops.update("homePage", homePageEntry.Text);
        this.Destroy();
    }
}