FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY Bar.Web/Bar.Web.csproj Bar.Web/
COPY Bar.Domain/Bar.Domain.csproj Bar.Domain/
COPY Bar.CQRS/Bar.CQRS.csproj Bar.CQRS/
COPY Bar.Data/Bar.Data.csproj Bar.Data/
RUN dotnet restore Bar.Web/Bar.Web.csproj
COPY . .
WORKDIR /src/Bar.Web
RUN dotnet build Bar.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Bar.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Bar.Web.dll"]
