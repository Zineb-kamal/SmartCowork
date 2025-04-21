FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SmartCowork.ApiGateway.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SmartCowork.ApiGateway.dll"]