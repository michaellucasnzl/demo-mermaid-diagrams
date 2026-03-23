# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookCatalog.Presentation/BookCatalog.Presentation.Console/BookCatalog.Presentation.Console.csproj", "BookCatalog.Presentation/BookCatalog.Presentation.Console/"]

RUN dotnet restore "BookCatalog.Presentation/BookCatalog.Presentation.Console/BookCatalog.Presentation.Console.csproj"

COPY . .

RUN dotnet publish "BookCatalog.Presentation/BookCatalog.Presentation.Console/BookCatalog.Presentation.Console.csproj" \
    -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookCatalog.Presentation.Console.dll"]
