using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
public class TabWindow : Window
{
    // Ui elements
    [UI] private Entry urlBar;
    [UI] private TextView htmlBox;
    [UI] private Button goForward;
    [UI] private Button goBack;
    [UI] private Button refresh;


    // Cache and DB connections
    Favorites favorites;
    History history;
    Application app;
    Options ops;

    // Http client
    HTTP http;

    private bool init = false; // Is html box drawn
    private int index = 0; // Current cache location, 0 is the newest local page

    public string Url
    {
        get { return urlBar.Text; }
    }

    // Local pages visited history cache
    List<Request> UrlWindowHistory = new List<Request>();

    /// <summary>
    /// constructor, Inits Cache, DB, Http client, and builder.
    /// </summary>
    public TabWindow(HTTP http, Application app, Options ops, History history, Favorites favorites) : this(new Builder("TabWindow.glade"))
    {
        this.app = app;
        this.http = http;
        this.ops = ops;
        this.history = history;
        this.favorites = favorites;
    }

    /// <summary>
    /// constructor builds UI.
    /// </summary>
    private TabWindow(Builder builder) : base(builder.GetRawOwnedObject("TabWindow"))
    {
        builder.Autoconnect(this);
        checkNavButtons();
        DeleteEvent += Window_DeleteEvent;
    }

    /// <summary>
    /// method  <c>Window_DeleteEvent</c> Quits the application upon a delete signal.
    /// </summary>
    private void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
    }

    /// <summary>
    /// method  <c>onDraw</c> vists home page on htmlbox draw
    /// </summary>
    private async void onDraw(object sender, DrawnArgs a)
    {
        if (!init)
        {
            this.init = !init;
            try
            {

                String homePage = ops["homePage"];
                var req = await loadUrl(homePage);
                UrlWindowHistory.Add(req);
                checkNavButtons();
            }
            catch (EmptyUrl e)
            {
                this.display(e.Message, "Invalid or empty URL");
            }
            catch (ErrorOccurred e)
            {
                // Expected
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    /// method  <c>getTitle</c> gets a title from html body.
    /// </summary>
    private string getTitle(string body)
    {
        Regex reg = new Regex(@"<title>(.*)<\/title>");
        // Get first group result from matches
        string res = reg.Match(body).Groups[1].ToString();
        return res;
    }

    /// <summary>
    /// method  <c>display</c> displays html and sets window title.
    /// </summary>
    private void display(string body, string title)
    {
        this.Title = title;
        htmlBox.Buffer.Text = body;
    }

    /// <summary>
    /// method  <c>display</c> displays html and sets window title from request object.
    /// </summary>
    private async void display(HttpResponseMessage res)
    {
        var body = await res.Content.ReadAsStringAsync();
        var title = getTitle(body);
        var statusCode = (int)res.StatusCode;
        this.Title = statusCode + " " + title;
        htmlBox.Buffer.Text = body;
    }



    /// <summary>
    /// method  <c>loadUrl</c> Loads a URI.
    /// </summary>
    internal async Task<Request> loadUrl(string url)
    {
        if (url is null)
        {
            throw new EmptyUrl();
        }
        try
        {
            urlBar.Text = url;
            Request req = await http.request(url);
            if (req.Response is null)
            {
                throw new EmptyResponse();
            }
            this.display(req.Response);
            return req;
        }
        // When invalid characters appear
        catch (UriFormatException e)
        {
            this.display(e.Message, "Uri Format");
        }
        // When URI syntax is not valid
        catch (InvalidOperationException e)
        {
            this.display(e.Message, "Invalid Operation");
        }
        // When any other request issue appears
        catch (HttpRequestException e)
        {
            Type type = e.GetBaseException().GetType();
            // When hostname can not be resolved
            if (type == typeof(System.Net.Sockets.SocketException))
            {
                this.display(e.Message, "Invalid Operation");
            }
            // When HTTP error codes appear
            else
            {
                var title = e.Message.Substring(48, 3);
                var body = e.Message.Substring(48);
                this.display(body, title);
            }
        }
        catch (NotSupportedException e)
        {
            this.display(e.Message, "Not Supported");
        }

        // Default case
        throw new ErrorOccurred();
    }

    /// <summary>
    /// method  <c>loadUrl</c> Loads a request.
    /// </summary>
    private Request loadUrl(Request req)
    {
        if (req.Response is not null)
        {
            this.urlBar.Text = req.value;
            this.display(req.Response);
            return req;
        }
        throw new EmptyResponse();
    }


    /// <summary>
    /// method  <c>localUrlLoad</c> Loads a request based on index.
    /// </summary>
    private void localUrlLoad()
    {
        try
        {
            Request req = UrlWindowHistory.ElementAt(UrlWindowHistory.Count - 1 + this.index);
            loadUrl(req);
            checkNavButtons();
        }
        catch (EmptyResponse e)
        {
            this.display(e.Message, "Empty response");
        }
    }

    /// <summary>
    /// method  <c>loadUrlWithHistory</c> Loads a string URI and saves output to cache and database.
    /// </summary>
    internal async Task loadUrlWithHistory(string url)
    {
        try
        {
            // Load url 
            Request req = await loadUrl(url);

            // Save to cache and db
            this.history.add(req);

            // Handles window url cache 
            this.UrlWindowHistory.RemoveRange(0, Math.Abs(index));
            this.index = 0;
            UrlWindowHistory.Add(req);

            // Refresh UI elements
            checkNavButtons();
        }
        catch (ArgumentNullException e)
        {
            this.display(e.Message, "Null Argument");
        }
        catch (EmptyUrl e)
        {
            this.display(e.Message, "Empty Url");
        }
        catch (EmptyResponse e)
        {
            this.display(e.Message, "Empty Response");
        }
    }

    /// <summary>
    /// method  <c>checkNavButtons</c> Tracks index location in cache; disables and enables UI elements in relation to cache index
    /// </summary>
    public void checkNavButtons()
    {
        if (this.index == 0) // newest page
        {
            goForward.Sensitive = false;
        }
        else // index cannot be positive therefore 
        {
            goForward.Sensitive = true;
        }

        if (UrlWindowHistory.Count < 1) // If zero pages are loaded
        {
            refresh.Sensitive = false;
            goBack.Sensitive = false;
        }
        else if (Math.Abs(this.index) + 1 == UrlWindowHistory.Count) // Check if we are on the oldest page
        {
            refresh.Sensitive = true;
            goBack.Sensitive = false;
        }
        else // We are in between newest and oldest
        {
            refresh.Sensitive = true;
            goBack.Sensitive = true;
        }
    }

    // Ui handlers
    private async void homePress(object sender, EventArgs a)
    {
        String homePage = ops.get("homePage");
        try
        {
            await loadUrlWithHistory(homePage);
        }
        catch (ErrorOccurred e)
        {
            // incorrect URI or broken DB connection
            Console.WriteLine(e);
        }
    }
    private void historyPress(object sender, EventArgs a)
    {
        Window history = new HistoryWindow(this, this.history);
        app.AddWindow(history);
    }

    private void favsPress(object sender, EventArgs a)
    {
        Window favsWin = new FavoritesWindow(this, favorites);
        app.AddWindow(favsWin);
    }

    private void optionsPress(object sender, EventArgs a)
    {
        Window opsWin = new OptionsWindow(this.ops);
        app.AddWindow(opsWin);
    }
    private void back(object sender, EventArgs a)
    {
        this.index -= 1; // Decrease index
        localUrlLoad();
    }


    private void forward(object sender, EventArgs a)
    {
        this.index += 1; // Increase index
        localUrlLoad();
    }

    /// <summary>
    /// method  <c>refreshView</c> Refresh page of current index location
    /// </summary>
    private async void refreshView(object sender, EventArgs a)
    {
        try
        {
            String localUrl = UrlWindowHistory.ElementAt(UrlWindowHistory.Count - 1 + this.index).value;
            await loadUrl(localUrl);
        }
        // Big problems!
        catch (ArgumentOutOfRangeException e)
        {
            this.display(e.Message, "Out Of Range");
        }
        catch (EmptyUrl e)
        {

            this.display(e.Message, "Empty Url");
        }
    }

    private async void urlBar_icon_press(object sender, IconPressArgs a)
    {
        try
        {
            await loadUrlWithHistory(urlBar.Text);
        }
        catch (ErrorOccurred e)
        {
            // incorrect URI or broken DB connection
            Console.WriteLine(e);
        }

    }
    private async void urlBar_activate(object sender, EventArgs a)
    {
        try
        {
            await loadUrlWithHistory(urlBar.Text);
        }
        catch (ErrorOccurred e)
        {
            // incorrect URI or broken DB connection
            Console.WriteLine(e);
        }
    }
}
