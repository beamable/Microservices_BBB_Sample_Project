FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview9 AS build
WORKDIR /src
COPY microservice.csproj .
RUN dotnet restore ./microservice.csproj
COPY . .
RUN dotnet publish ./microservice.csproj -c release -o /app

WORKDIR /app
ENTRYPOINT ["dotnet", "microservice.dll"]
