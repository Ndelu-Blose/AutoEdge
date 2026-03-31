# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY AutoEdge.sln ./
COPY AutoEdge.csproj ./
COPY AutoEdge.Tests/AutoEdge.Tests.csproj AutoEdge.Tests/

RUN dotnet restore "AutoEdge.sln"

# Copy everything else
COPY . .

# Publish app
RUN dotnet publish "AutoEdge.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AutoEdge.dll"]
