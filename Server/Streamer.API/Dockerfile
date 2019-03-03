# Build Streamer.API
FROM microsoft/dotnet:sdk AS build-env-api
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app

COPY --from=build-env-api /app/out .
COPY /Songs ./Songs

ENTRYPOINT ["dotnet", "Streamer.API.dll"]