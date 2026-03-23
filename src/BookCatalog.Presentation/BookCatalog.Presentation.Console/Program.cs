using System.Net.Http.Json;
using System.Text.Json;

var apiBase = args.FirstOrDefault() ?? "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(apiBase) };
var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// ── Header ──────────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine();
Console.WriteLine("  ╔══════════════════════════════════════════╗");
Console.WriteLine("  ║         Book Catalog Client              ║");
Console.WriteLine($"  ║  API : {apiBase,-34}║");
Console.WriteLine("  ╚══════════════════════════════════════════╝");
Console.ResetColor();

// ── Connectivity check ───────────────────────────────────────────────────────
try
{
    var ping = await http.GetAsync("/api/authors");
    ping.EnsureSuccessStatusCode();
    Info($"Connected to API at {apiBase}");
}
catch
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n  ERROR: Cannot reach API at {apiBase}");
    Console.WriteLine("  Make sure 'docker compose up' is running.");
    Console.ResetColor();
    return;
}

// ── Menu loop ────────────────────────────────────────────────────────────────
while (true)
{
    Console.WriteLine();
    Divider();
    Console.WriteLine("  [1] List Authors");
    Console.WriteLine("  [2] List Books");
    Console.WriteLine("  [3] Add Author");
    Console.WriteLine("  [4] Add Book");
    Console.WriteLine("  [5] Add Review");
    Console.WriteLine("  [6] Re-seed (if DB is empty)");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  [B] View catalog in browser  →  {apiBase}");
    Console.ResetColor();
    Console.WriteLine("  [0] Exit");
    Divider();
    Console.Write("  > ");

    switch (Console.ReadLine()?.Trim().ToUpperInvariant())
    {
        case "1": await ListAuthors(); break;
        case "2": await ListBooks();   break;
        case "3": await AddAuthor();   break;
        case "4": await AddBook();     break;
        case "5": await AddReview();   break;
        case "6": await Reseed();      break;
        case "B": OpenBrowser(apiBase); break;
        case "0": Console.WriteLine("\n  Goodbye!"); return;
        default:  Warn("Unknown option."); break;
    }
}

// ── Menu actions ─────────────────────────────────────────────────────────────

async Task ListAuthors()
{
    var authors = await FetchAuthors();
    if (authors.Length == 0) { Info("No authors found."); return; }
    Console.WriteLine();
    Console.WriteLine($"  {"#",-4}{"Name",-28}{"Born",-6}{"Died",-6}{"Nationality",-20}Books");
    Divider();
    for (var i = 0; i < authors.Length; i++)
    {
        var a = authors[i];
        Console.WriteLine($"  {i + 1,-4}{a.FullName,-28}{a.BirthYear,-6}{(a.DeathYear?.ToString() ?? "—"),-6}{a.Nationality,-20}{a.BookCount}");
    }
}

async Task ListBooks()
{
    var books = await FetchBooks();
    if (books.Length == 0) { Info("No books found."); return; }
    Console.WriteLine();
    Console.WriteLine($"  {"#",-4}{"Title",-36}{"Author",-22}{"Year",-6}Rating");
    Divider();
    for (var i = 0; i < books.Length; i++)
    {
        var b = books[i];
        var rating = b.AverageRating.HasValue
            ? $"{Stars(b.AverageRating.Value)} ({b.AverageRating.Value:F1})"
            : "—";
        Console.WriteLine($"  {i + 1,-4}{Clip(b.Title, 35),-36}{Clip(b.AuthorName, 21),-22}{b.PublishedYear,-6}{rating}");
    }
}

async Task AddAuthor()
{
    Console.WriteLine("\n  Add Author — Open Library Search");
    Divider();
    var query = Ask("  Search keyword (name or partial name)");
    Info("Searching Open Library...");

    var results = await SearchOlAuthors(query);

    OlAuthorResult? picked = null;
    if (results.Length > 0)
    {
        Console.WriteLine();
        for (var i = 0; i < results.Length; i++)
        {
            var r = results[i];
            var died = r.DeathDate != null ? $"–{r.DeathDate}" : string.Empty;
            var born = r.BirthDate != null ? $" ({r.BirthDate}{died})" : string.Empty;
            Console.WriteLine($"    [{i + 1}] {r.Name}{born}  —  {r.WorkCount} works");
        }
        Console.WriteLine($"    [M] Enter manually");
        Console.WriteLine();

        var choice = Ask("  Pick a number or M");
        if (choice.Equals("M", StringComparison.OrdinalIgnoreCase))
            picked = null;
        else if (int.TryParse(choice, out var idx) && idx >= 1 && idx <= results.Length)
            picked = results[idx - 1];
        else
        {
            Warn("Invalid choice.");
            return;
        }
    }
    else
    {
        Info("No results found. Falling back to manual entry.");
    }

    // Pre-fill from OL result or prompt for everything
    string firstName, lastName, nationality, bio;
    int birthYear;

    if (picked != null)
    {
        // Split name best-effort: last token = last name
        var parts = picked.Name.Trim().Split(' ');
        firstName = string.Join(" ", parts[..^1]);
        lastName  = parts[^1];
        bio       = string.Empty;

        _ = int.TryParse(picked.BirthDate?.Split(' ').FirstOrDefault(p => p.Length == 4 && int.TryParse(p, out _)), out birthYear);

        Console.WriteLine();
        Console.WriteLine($"  Pre-filled from Open Library:");
        Console.WriteLine($"    First name  : {firstName}");
        Console.WriteLine($"    Last name   : {lastName}");
        Console.WriteLine($"    Birth year  : {(birthYear > 0 ? birthYear.ToString() : "(unknown)")}");
        Console.WriteLine();

        firstName   = AskWithDefault("  First name", firstName);
        lastName    = AskWithDefault("  Last name",  lastName);
        var yearStr = AskWithDefault("  Birth year", birthYear > 0 ? birthYear.ToString() : "");
        if (!int.TryParse(yearStr, out birthYear)) { Warn("Invalid birth year."); return; }
        nationality = Ask("  Nationality");
        bio         = Ask("  Biography (Enter to skip)", optional: true);
    }
    else
    {
        firstName    = Ask("  First name");
        lastName     = Ask("  Last name");
        var yearStr  = Ask("  Birth year");
        nationality  = Ask("  Nationality");
        bio          = Ask("  Biography (Enter to skip)", optional: true);
        if (!int.TryParse(yearStr, out birthYear)) { Warn("Invalid birth year."); return; }
    }

    var resp = await http.PostAsJsonAsync("/api/authors",
        new { firstName, lastName, birthYear, nationality, biography = bio });

    if (resp.IsSuccessStatusCode)
    {
        var result = await resp.Content.ReadFromJsonAsync<IdResult>(jsonOpts);
        Ok($"Author '{firstName} {lastName}' added.  ID: {result?.Id}");
    }
    else
        Warn($"Failed ({(int)resp.StatusCode}): {await resp.Content.ReadAsStringAsync()}");
}

async Task AddBook()
{
    var authors = await FetchAuthors();
    if (authors.Length == 0) { Warn("No authors yet. Add an author first."); return; }

    Console.WriteLine("\n  Add Book — Open Library Search");
    Divider();
    var query = Ask("  Search keyword (title, author, or both)");
    Info("Searching Open Library...");

    var results = await SearchOlBooks(query);

    OlBookResult? picked = null;
    if (results.Length > 0)
    {
        Console.WriteLine();
        for (var i = 0; i < results.Length; i++)
        {
            var r = results[i];
            var yearLabel  = r.FirstPublishYear.HasValue ? $" ({r.FirstPublishYear})" : string.Empty;
            var pagesLabel = r.PageCount.HasValue        ? $", {r.PageCount} pp"      : string.Empty;
            Console.WriteLine($"    [{i + 1}] {r.Title}{yearLabel}  by {r.AuthorName ?? "unknown"}{pagesLabel}");
        }
        Console.WriteLine($"    [M] Enter manually");
        Console.WriteLine();

        var choice = Ask("  Pick a number or M");
        if (choice.Equals("M", StringComparison.OrdinalIgnoreCase))
            picked = null;
        else if (int.TryParse(choice, out var idx) && idx >= 1 && idx <= results.Length)
            picked = results[idx - 1];
        else
        {
            Warn("Invalid choice.");
            return;
        }
    }
    else
    {
        Info("No results found. Falling back to manual entry.");
    }

    // Show author list for selection
    Console.WriteLine();
    Console.WriteLine("  Authors in catalog:");
    for (var i = 0; i < authors.Length; i++)
        Console.WriteLine($"    [{i + 1}] {authors[i].FullName}");

    // Pre-suggest author based on OL result authorName if possible
    var suggestedAuthorIdx = picked?.AuthorName != null
        ? Array.FindIndex(authors, a =>
            a.FullName.Contains(picked.AuthorName, StringComparison.OrdinalIgnoreCase))
        : -1;
    var authorPrompt = suggestedAuthorIdx >= 0
        ? $"  Author number (suggested: {suggestedAuthorIdx + 1})"
        : "  Author number";
    var authorIdxStr = AskWithDefault(authorPrompt,
        suggestedAuthorIdx >= 0 ? (suggestedAuthorIdx + 1).ToString() : "");

    if (!int.TryParse(authorIdxStr, out var authorIdx) || authorIdx < 1 || authorIdx > authors.Length)
    { Warn("Invalid author number."); return; }
    var authorId = authors[authorIdx - 1].Id;

    // Pre-fill fields
    string title, isbn, publisher, genreStr;
    int year, pages;

    if (picked != null)
    {
        Console.WriteLine();
        Console.WriteLine("  Pre-filled from Open Library:");
        Console.WriteLine($"    Title       : {picked.Title}");
        Console.WriteLine($"    Year        : {picked.FirstPublishYear?.ToString() ?? "(unknown)"}");
        Console.WriteLine($"    Pages       : {picked.PageCount?.ToString() ?? "(unknown)"}");
        Console.WriteLine($"    Publisher   : {picked.Publisher ?? "(unknown)"}");
        Console.WriteLine($"    ISBN        : {picked.Isbn ?? "(none)"}");
        Console.WriteLine($"    Subjects    : {picked.SubjectsDisplay ?? "(none)"}");
        Console.WriteLine();

        title     = AskWithDefault("  Title",          picked.Title);
        var yearS = AskWithDefault("  Published year", picked.FirstPublishYear?.ToString() ?? "");
        var pagS  = AskWithDefault("  Page count",     picked.PageCount?.ToString() ?? "");
        publisher = AskWithDefault("  Publisher",      picked.Publisher ?? "");
        isbn      = AskWithDefault("  ISBN",           picked.Isbn ?? "");
        genreStr  = AskWithDefault("  Genres (comma-separated)", picked.SubjectsDisplay ?? "");

        if (!int.TryParse(yearS, out year) || !int.TryParse(pagS, out pages))
        { Warn("Invalid year or page count."); return; }
    }
    else
    {
        title     = Ask("  Title");
        isbn      = Ask("  ISBN (Enter to skip)", optional: true);
        var yearS = Ask("  Published year");
        var pagS  = Ask("  Page count");
        publisher = Ask("  Publisher name");
        genreStr  = Ask("  Genres (comma-separated, e.g. Fantasy, Adventure)");
        if (!int.TryParse(yearS, out year) || !int.TryParse(pagS, out pages))
        { Warn("Invalid year or page count."); return; }
    }

    var genres = genreStr.Split(',').Select(g => g.Trim()).Where(g => g.Length > 0).ToList();
    var resp = await http.PostAsJsonAsync("/api/books",
        new { title, isbn = isbn ?? string.Empty, publishedYear = year, pageCount = pages,
              authorId, publisherName = publisher, genres });

    if (resp.IsSuccessStatusCode)
    {
        var result = await resp.Content.ReadFromJsonAsync<IdResult>(jsonOpts);
        Ok($"Book '{title}' added.  ID: {result?.Id}");
    }
    else
        Warn($"Failed ({(int)resp.StatusCode}): {await resp.Content.ReadAsStringAsync()}");
}

async Task AddReview()
{
    var books = await FetchBooks();
    if (books.Length == 0) { Warn("No books available."); return; }

    Console.WriteLine("\n  Add Review");
    Divider();
    Console.WriteLine("  Books:");
    for (var i = 0; i < books.Length; i++)
        Console.WriteLine($"    [{i + 1}] {books[i].Title}  ({books[i].AuthorName})");

    var idxStr = Ask("  Book number");
    if (!int.TryParse(idxStr, out var idx) || idx < 1 || idx > books.Length)
    { Warn("Invalid book number."); return; }

    var bookId       = books[idx - 1].Id;
    var reviewerName = Ask("  Your name");
    var ratingStr    = Ask("  Rating (1–5)");
    var reviewText   = Ask("  Review text");

    if (!int.TryParse(ratingStr, out var rating) || rating < 1 || rating > 5)
    { Warn("Rating must be 1–5."); return; }

    var resp = await http.PostAsJsonAsync($"/api/reviews/{bookId}",
        new { reviewerName, rating, reviewText });

    if (resp.IsSuccessStatusCode)
        Ok("Review submitted!");
    else
        Warn($"Failed ({(int)resp.StatusCode}): {await resp.Content.ReadAsStringAsync()}");
}

async Task Reseed()
{
    var resp = await http.PostAsync("/api/catalog/seed", null);
    if (resp.IsSuccessStatusCode)
        Ok("Seed check complete. (Data is only seeded when the database is empty.)");
    else
        Warn($"Failed: {resp.StatusCode}");
}

void OpenBrowser(string url)
{
    Info($"Opening: {url}");
    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
    catch { Info("Could not open browser automatically. Navigate there manually."); }
}

// ── Data helpers ─────────────────────────────────────────────────────────────

async Task<ApiAuthor[]> FetchAuthors()
{
    try { return await http.GetFromJsonAsync<ApiAuthor[]>("/api/authors", jsonOpts) ?? []; }
    catch { Warn("Could not fetch authors."); return []; }
}

async Task<ApiBook[]> FetchBooks()
{
    try { return await http.GetFromJsonAsync<ApiBook[]>("/api/books", jsonOpts) ?? []; }
    catch { Warn("Could not fetch books."); return []; }
}

async Task<OlAuthorResult[]> SearchOlAuthors(string q)
{
    try
    {
        var encoded = Uri.EscapeDataString(q);
        return await http.GetFromJsonAsync<OlAuthorResult[]>($"/api/search/authors?q={encoded}", jsonOpts) ?? [];
    }
    catch { return []; }
}

async Task<OlBookResult[]> SearchOlBooks(string q)
{
    try
    {
        var encoded = Uri.EscapeDataString(q);
        return await http.GetFromJsonAsync<OlBookResult[]>($"/api/search/books?q={encoded}", jsonOpts) ?? [];
    }
    catch { return []; }
}

// ── UI helpers ───────────────────────────────────────────────────────────────

static string Ask(string label, bool optional = false)
{
    while (true)
    {
        Console.Write($"{label}: ");
        var v = Console.ReadLine()?.Trim() ?? string.Empty;
        if (optional || v.Length > 0) return v;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  (this field is required)");
        Console.ResetColor();
    }
}

static string AskWithDefault(string label, string defaultValue)
{
    Console.Write(string.IsNullOrEmpty(defaultValue)
        ? $"{label}: "
        : $"{label} [{defaultValue}]: ");
    var v = Console.ReadLine()?.Trim() ?? string.Empty;
    return v.Length > 0 ? v : defaultValue;
}

static void Ok(string msg)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✓ {msg}");
    Console.ResetColor();
}

static void Info(string msg)   => Console.WriteLine($"  {msg}");
static void Divider()          => Console.WriteLine("  " + new string('─', 60));

static void Warn(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n  ! {msg}");
    Console.ResetColor();
}

static string Stars(double r)
{
    var full = (int)Math.Round(r);
    return new string('★', full) + new string('☆', 5 - full);
}

static string Clip(string s, int max) =>
    s.Length <= max ? s : s[..(max - 1)] + "…";

// ── Local response types ──────────────────────────────────────────────────────

record ApiAuthor(Guid Id, string FullName, string FirstName, string LastName,
                 int BirthYear, int? DeathYear, string Nationality, string Biography, int BookCount);

record ApiBook(Guid Id, string Title, Guid AuthorId, string AuthorName, string Publisher,
               string ISBN, int PublishedYear, int PageCount, string? CoverUrl,
               List<string> Genres, double? AverageRating, int ReviewCount);

record IdResult(Guid Id, string? FullName, string? Title);

record OlAuthorResult(string Key, string Name, string? BirthDate, string? DeathDate,
                      string? TopWork, int WorkCount);

record OlBookResult(string Key, string Title, string? AuthorName, int? FirstPublishYear,
                    int? PageCount, string? Publisher, string? Isbn, List<string>? Subjects)
{
    public string? SubjectsDisplay => Subjects is { Count: > 0 }
        ? string.Join(", ", Subjects.Take(3))
        : null;
}

