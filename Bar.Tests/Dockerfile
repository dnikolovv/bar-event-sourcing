FROM microsoft/dotnet:2.2-sdk

WORKDIR /src

COPY . .

RUN dotnet restore Bar.sln
RUN dotnet build Bar.sln

WORKDIR /src/Bar.Tests

CMD ["dotnet", "test"]

