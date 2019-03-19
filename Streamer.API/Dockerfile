FROM microsoft/dotnet:sdk AS build
WORKDIR /app

COPY *.sln ./
COPY */*.csproj ./
COPY csproj_copy.sh ./
RUN bash ./csproj_copy.sh

RUN dotnet restore

COPY . ./

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.2-alpine3.8 as final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet", "Streamer.API.dll"]
