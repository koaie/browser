using System;
using Gtk;
using System.Threading.Tasks;

namespace F20SC_CW1
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Init lists
            Options options = new Options();
            Favorites favorites = new Favorites();
            History history = new History();

            // Init database
            using (ListContext db = new ListContext())
            {
                // If fresh load defaults, otherwise load from db
                if (db.Database.EnsureCreated())
                {
                    options.loadDefaults();
                }
                else
                {
                    options.reload();
                    favorites.reload();
                    history.reload();
                }
            }

            // Init http client
            HTTP http = new HTTP();

            // Init GUI
            Application.Init();


            var app = new Application("org.F20SC_CW1.F20SC_CW1", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            // Create a new tab window
            var win = new TabWindow(http, app, options, history, favorites);
            app.AddWindow(win);

            // Change title
            win.Title = "Browser";
            win.Show(); // Show window
            Application.Run(); // Run app
        }
    }
}
