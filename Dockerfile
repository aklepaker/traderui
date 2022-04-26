# Build API
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
COPY . /traderui
WORKDIR /traderui
RUN dotnet restore Server/traderui.Server.csproj
RUN dotnet publish Server/traderui.Server.csproj -r linux-x64 -c Release -o out --self-contained true -p:PublishTrimmed=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /traderui
COPY --from=build-env /traderui/out .
ENTRYPOINT ["dotnet", "traderui.dll", "--urls", "http://0.0.0.0:5000"]
