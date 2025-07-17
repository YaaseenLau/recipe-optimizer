FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RecipeOptimizer.API/RecipeOptimizer.API.csproj", "RecipeOptimizer.API/"]
COPY ["RecipeOptimizer.Core/RecipeOptimizer.Core.csproj", "RecipeOptimizer.Core/"]
COPY ["RecipeOptimizer.Infrastructure/RecipeOptimizer.Infrastructure.csproj", "RecipeOptimizer.Infrastructure/"]
RUN dotnet restore "RecipeOptimizer.API/RecipeOptimizer.API.csproj"
COPY . .
WORKDIR "/src/RecipeOptimizer.API"
RUN dotnet build "RecipeOptimizer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RecipeOptimizer.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecipeOptimizer.API.dll"]
