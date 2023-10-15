FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app
COPY ./NLayerApp.Core/*.csproj ./NLayerApp.Core/
COPY ./NLayerApp.Repository/*.csproj ./NLayerApp.Repository/
COPY ./NLayerApp.Service/*.csproj ./NLayerApp.Service/
COPY ./NLayerApp.API/*.csproj ./NLayerApp.API/
COPY *.sln .
RUN dotnet Restore
COPY . .
RUN dotnet publish ./NLayerApp.API/*.csproj -o /publish/

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet","NLayerApp.API.dll"]

