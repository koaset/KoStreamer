FROM microsoft/dotnet:sdk AS build

COPY *.sln ./
COPY */*.csproj ./
COPY csproj_copy.sh ./
RUN bash ./csproj_copy.sh

RUN dotnet restore

COPY . ./

FROM build AS publish
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.2-alpine3.8 as final
COPY --from=publish /out .
COPY /wait-for.sh .

CMD ["dotnet", "DatabaseUpgrade.dll"]
