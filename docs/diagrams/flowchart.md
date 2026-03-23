# Application Flowchart

This flowchart shows two distinct flows: the **API startup** inside Docker, and the **interactive Console client** running on the developer's local machine.

```mermaid
graph TD
    subgraph DOCKER["Docker Compose - always running"]
        UP([docker compose up]) --> DB[(PostgreSQL\ncontainer)]
        DB --> HC{Health Check\npg_isready?}
        HC -->|failing| WAIT[Wait 5s and retry]
        WAIT --> HC
        HC -->|healthy| APISTART[BookCatalog.Presentation.Api\ncontainer starts]
        APISTART --> DI[Configure\nDependency Injection]
        DI --> MIG[Apply EF Core\nMigrations]
        MIG --> CHKSEEDED{Already\nseeded?}
        CHKSEEDED -->|Yes| LISTEN
        CHKSEEDED -->|No| PUBS[Seed Publishers\nand Genres]
        PUBS --> AUTHORS[Seed Authors\nfetch bios from Open Library]
        AUTHORS --> BOOKS[Seed Books\nfetch covers from Open Library]
        BOOKS --> REVIEWS[Seed Reviews]
        REVIEWS --> LISTEN([API Listening\nlocalhost:5000])
    end

    subgraph CLIENT["Console Client - local machine"]
        CON([dotnet run]) --> CONN{Connect to\nlocalhost:5000}
        CONN -->|unreachable| ERR([Error - start Docker first])
        CONN -->|ok| MENU[Interactive Menu]
        MENU --> OPT1[1 - List Authors]
        MENU --> OPT2[2 - List Books]
        MENU --> OPT3[3 - Add Author]
        MENU --> OPT4[4 - Add Book]
        MENU --> OPT5[5 - Add Review]
        MENU --> OPT6[6 - Re-seed]
        MENU --> OPTB[B - View in Browser]
    end

    OPT1 -->|GET /api/authors| LISTEN
    OPT2 -->|GET /api/books| LISTEN
    OPT3 -->|POST /api/authors| LISTEN
    OPT4 -->|POST /api/books| LISTEN
    OPT5 -->|POST /api/reviews| LISTEN
    OPT6 -->|POST /api/catalog/seed| LISTEN
    OPTB -->|GET /| BROWSER([Browser shows\nlive HTML catalog])
    LISTEN --> BROWSER

    style DOCKER fill:#1e3a5f,color:#fff
    style CLIENT fill:#1a472a,color:#fff
    style LISTEN fill:#2e7d32,color:#fff
    style ERR fill:#7f1d1d,color:#fff
    style BROWSER fill:#7a3b1e,color:#fff
```

## Flow Description

**Docker side (API service)**
1. Docker Compose starts PostgreSQL and polls `pg_isready` every 5 s.
2. Once healthy, the `BookCatalog.Presentation.Api` container starts, applies EF migrations, and seeds the database (idempotent — no-op if already seeded).
3. The API then **stays running** indefinitely, listening on port 5000.

**Console side (local client)**
1. Run `dotnet run` from the `BookCatalog.Presentation.Console` project.
2. The client verifies API connectivity, then presents an interactive menu.
3. Any add/list action sends an HTTP request to the running API.
4. Press **B** to open `http://localhost:5000` in the browser and see the live catalog page.

