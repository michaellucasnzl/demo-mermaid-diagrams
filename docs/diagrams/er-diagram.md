# Entity Relationship Diagram

This diagram shows the database schema for the Book Catalog application, including all entities, their fields, primary keys, foreign keys, and the relationships between them.

```mermaid
erDiagram
    Author {
        guid Id PK
        string FirstName
        string LastName
        date BirthDate
        date DeathDate
        string Nationality
        string Biography
        string OpenLibraryKey
        string Website
    }

    Book {
        guid Id PK
        string Title
        string ISBN
        int PublishedYear
        string Description
        int PageCount
        string CoverUrl
        string OpenLibraryKey
        guid AuthorId FK
        guid PublisherId FK
    }

    Publisher {
        guid Id PK
        string Name
        int FoundedYear
        string Country
        string Website
    }

    Genre {
        guid Id PK
        string Name
        string Description
    }

    BookGenre {
        guid BookId PK,FK
        guid GenreId PK,FK
    }

    BookReview {
        guid Id PK
        guid BookId FK
        string ReviewerName
        int Rating
        string ReviewText
        datetime CreatedDate
    }

    Author ||--o{ Book : "writes"
    Publisher ||--o{ Book : "publishes"
    Book ||--o{ BookGenre : "categorised by"
    Genre ||--o{ BookGenre : "applied to"
    Book ||--o{ BookReview : "reviewed in"
```

## Relationship Notation

| Symbol | Meaning |
|--------|---------|
| `\|\|` | Exactly one |
| `o\|` | Zero or one |
| `o{` | Zero or many |
| `\|{` | One or many |

## Entity Descriptions

| Entity | Description |
|--------|-------------|
| **Author** | A person who has written one or more books. Enriched with data from the Open Library API. |
| **Book** | A published work, linked to an author and publisher. Cover images sourced from Open Library. |
| **Publisher** | The company responsible for publishing a book. |
| **Genre** | A literary classification (e.g. Fantasy, Children's Literature). |
| **BookGenre** | Join table enabling a many-to-many relationship between Books and Genres. |
| **BookReview** | A review of a book, including a 1–5 star rating and written text. |
