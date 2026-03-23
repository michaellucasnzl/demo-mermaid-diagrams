# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookCatalog.Presentation/BookCatalog.Presentation.Api/BookCatalog.Presentation.Api.csproj", "BookCatalog.Presentation/BookCatalog.Presentation.Api/"]
COPY ["BookCatalog.Infrastructure/BookCatalog.Infrastructure.csproj", "BookCatalog.Infrastructure/"]
COPY ["BookCatalog.Application/BookCatalog.Application.csproj", "BookCatalog.Application/"]
COPY ["BookCatalog.Domain/BookCatalog.Domain.csproj", "BookCatalog.Domain/"]

RUN dotnet restore "BookCatalog.Presentation/BookCatalog.Presentation.Api/BookCatalog.Presentation.Api.csproj"

COPY . .

RUN dotnet publish "BookCatalog.Presentation/BookCatalog.Presentation.Api/BookCatalog.Presentation.Api.csproj" \
    -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "BookCatalog.Presentation.Api.dll"]
