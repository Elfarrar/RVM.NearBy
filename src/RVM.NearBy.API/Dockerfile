FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/RVM.NearBy.API/RVM.NearBy.API.csproj", "src/RVM.NearBy.API/"]
COPY ["src/RVM.NearBy.Infrastructure/RVM.NearBy.Infrastructure.csproj", "src/RVM.NearBy.Infrastructure/"]
COPY ["src/RVM.NearBy.Domain/RVM.NearBy.Domain.csproj", "src/RVM.NearBy.Domain/"]
RUN dotnet restore "src/RVM.NearBy.API/RVM.NearBy.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.NearBy.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RVM.NearBy.API.dll"]
