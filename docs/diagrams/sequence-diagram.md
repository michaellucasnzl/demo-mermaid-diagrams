# Sequence Diagram

This sequence diagram shows all component interactions: Docker startup, database seeding (including Open Library API calls), and an interactive console session where the developer lists books, adds a new one, and views the live catalog.

```mermaid
sequenceDiagram
    actor Dev as Developer
    participant DC  as Docker Compose
    participant PG  as PostgreSQL
    participant API as BookCatalog.Presentation.Api
    participant CTX as AppDbContext
    participant CS  as CatalogService
    participant SED as DatabaseSeeder
    participant OLS as OpenLibraryService
    participant OLA as Open Library API
    participant CON as Console Client
    participant BR  as Browser

    Dev->>DC: docker compose up --build

    DC->>PG: Start postgres:16-alpine
    loop Every 5 seconds until healthy
        DC->>PG: pg_isready health check
        PG-->>DC: healthy / not yet
    end

    DC->>API: Start BookCatalog.Presentation.Api (port 8080→5000)
    API->>CTX: context.Database.MigrateAsync()
    CTX->>PG: Apply pending migrations
    PG-->>CTX: Schema ready

    API->>CS: SeedAsync()
    CS->>SED: SeedAsync()
    SED->>PG: Authors.Any()?
    PG-->>SED: 0 rows

    alt Database is empty — seed it
        SED->>PG: INSERT publishers, genres

        loop Tolkien · Lewis · Dahl
            SED->>OLS: GetAuthorAsync(olKey)
            OLS->>OLA: GET /authors/{key}.json
            alt API success
                OLA-->>OLS: Author JSON
                OLS-->>SED: Biography & metadata
            else Timeout / error
                OLA-->>OLS: —
                OLS-->>SED: null → use fallback
            end
        end

        loop 4 selected books
            SED->>OLS: GetBookAsync(olKey)
            OLS->>OLA: GET /works/{key}.json
            alt API success
                OLA-->>OLS: Book JSON
                OLS-->>SED: Description & cover URL
            else Timeout / error
                OLA-->>OLS: —
                OLS-->>SED: null → use fallback
            end
        end

        SED->>PG: INSERT authors, books, bookGenres, reviews
        PG-->>SED: 3 authors · 10 books · 20 reviews saved
    else Already seeded
        SED-->>CS: no-op
    end

    Note over API: 🟢 Listening on :8080 (→ localhost:5000)

    Dev->>CON: dotnet run (local machine)
    CON->>API: GET /api/authors (connectivity check)
    API-->>CON: 200 OK

    Note over Dev,CON: Developer chooses [2] List Books

    CON->>API: GET /api/books
    API->>CS: GetBooksAsync()
    CS->>PG: SELECT books + author, publisher, genres, reviews
    PG-->>CS: 10 book rows
    CS-->>API: IEnumerable~Book~
    API-->>CON: 200 JSON array
    CON-->>Dev: Prints numbered table of books

    Note over Dev,CON: Developer chooses [5] Add Review

    Dev->>CON: Picks book #3, enters name + rating + text
    CON->>API: POST /api/reviews/{bookId}
    API->>CS: AddReviewAsync(bookId, request)
    CS->>PG: SELECT book by id
    PG-->>CS: Book found
    CS->>PG: INSERT BookReview
    PG-->>CS: Saved
    CS-->>API: BookReview
    API-->>CON: 201 Created
    CON-->>Dev: ✓ Review submitted!

    Note over Dev,CON: Developer presses [B] to open catalog

    CON-->>Dev: Displays URL hint
    Dev->>BR: Opens http://localhost:5000
    BR->>API: GET /
    API->>CS: GetCatalogHtmlAsync()
    CS->>PG: SELECT authors + books + reviews
    PG-->>CS: All rows (now includes new review)
    CS-->>API: HTML string
    API-->>BR: 200 text/html
    BR-->>Dev: Live catalog page with updated data
```

## Participants

| Participant | Role |
|-------------|------|
| **Docker Compose** | Orchestrates container startup order and health checks |
| **PostgreSQL** | Relational database — all data lives here |
| **BookCatalog.Presentation.Api** | ASP.NET Core minimal API — the long-running microservice |
| **AppDbContext** | EF Core context — translates LINQ queries to SQL |
| **CatalogService** | Application layer — all business logic |
| **DatabaseSeeder** | Infrastructure — seeds initial data via Open Library API |
| **OpenLibraryService** | Infrastructure — HTTP client for Open Library metadata |
| **Open Library API** | External FOSS API (openlibrary.org) |
| **Console Client** | Thin interactive client — runs locally, sends HTTP requests |
| **Browser** | Views the live HTML catalog at `http://localhost:5000` |

