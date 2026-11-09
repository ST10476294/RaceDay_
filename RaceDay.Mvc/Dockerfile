# Stage 1: build with the full SDK. It is not included in the final image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copying the project file first lets Docker reuse the restored packages when only code changes.
COPY RaceDay.Mvc.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

# Stage 2: run on the smaller ASP.NET Core runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The base image provides this non-root user and listens on port 8080 by default.
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
