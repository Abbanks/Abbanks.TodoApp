FROM mcr.microsoft.com/dotnet/sdk:9.0.100 AS build
WORKDIR /src

COPY NuGet.config ./
COPY *.sln ./

COPY src/Abbanks.TodoApp.API/*.csproj ./src/Abbanks.TodoApp.API/
COPY src/Abbanks.TodoApp.Core/*.csproj ./src/Abbanks.TodoApp.Core/
COPY src/Abbanks.TodoApp.Application/*.csproj ./src/Abbanks.TodoApp.Application/
COPY src/Abbanks.TodoApp.Infrastructure/*.csproj ./src/Abbanks.TodoApp.Infrastructure/
COPY tests/Abbanks.TodoApp.UnitTests/*.csproj ./tests/Abbanks.TodoApp.UnitTests/
COPY tests/Abbanks.TodoApp.IntegrationTests/*.csproj ./tests/Abbanks.TodoApp.IntegrationTests/

RUN dotnet restore --configfile ./NuGet.config

COPY src/. ./src/
COPY tests/. ./tests/

FROM build AS test
WORKDIR /src
RUN dotnet test --no-restore

FROM build AS publish
WORKDIR /src/src/Abbanks.TodoApp.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "Abbanks.TodoApp.API.dll"]