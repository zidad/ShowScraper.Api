FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /ShowScraper.Api
COPY ["ShowScraper.Api.csproj", "../ShowScraper.Api/"]
RUN dotnet restore "../ShowScraper.Api/ShowScraper.Api.csproj"
COPY . .
WORKDIR "/ShowScraper.Api"
RUN dotnet build "ShowScraper.Api.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ShowScraper.Api.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN ls
ENTRYPOINT ["dotnet", "ShowScraper.Api.dll"]