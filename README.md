# Recipe Optimizer

A full-stack web application that optimizes recipe combinations to feed the maximum number of people with available ingredients. Built with .NET 8 and vanilla JavaScript.

## Overview

The Recipe Optimizer solves the common problem of meal planning with limited ingredients. It manages your pantry inventory, stores recipes with their requirements, and calculates optimal recipe combinations to maximize the number of people you can feed.

**Key Capabilities:**
- Ingredient inventory management with quantity tracking
- Recipe storage with serving sizes and ingredient requirements  
- Optimization algorithm that maximizes people fed given constraints
- Real-time UI updates with intelligent sorting

## Architecture

**Backend (.NET 8 Web API)**
- `RecipeOptimizer.API` - Controllers and static file serving
- `RecipeOptimizer.Core` - Domain models and business logic
- `RecipeOptimizer.Infrastructure` - Entity Framework data access
- `RecipeOptimizer.Tests` - Unit and integration tests

**Frontend (JavaScript + Bootstrap)**
- Responsive dark theme UI with modal-based interactions
- Event-driven architecture with efficient DOM manipulation
- Real-time data updates without page refreshes

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- PostgreSQL (via Docker)

### Setup

```bash
# Clone and navigate to project
git clone <repository-url>
cd Assessment

# Start database
docker-compose up -d db

# Restore dependencies and apply migrations
dotnet restore
dotnet ef database update --project RecipeOptimizer.Infrastructure --startup-project RecipeOptimizer.API

# Run application
dotnet run --project RecipeOptimizer.API
```

**Access Points:**
- Application: https://localhost:7001
- API Documentation: https://localhost:7001/swagger

### Alternative: Full Docker Setup
```bash
docker-compose up --build
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## API Endpoints

**Ingredients**
- `GET /api/ingredients` - List all ingredients
- `POST /api/ingredients` - Create ingredient
- `PUT /api/ingredients/{id}` - Update ingredient
- `DELETE /api/ingredients/{id}` - Delete ingredient

**Recipes**
- `GET /api/recipes` - List all recipes
- `POST /api/recipes` - Create recipe
- `PUT /api/recipes/{id}` - Update recipe
- `DELETE /api/recipes/{id}` - Delete recipe
- `POST /api/recipes/optimize` - Get optimal recipe combinations

Complete API documentation available at `/swagger` when running.

## Configuration

Database connection and other settings are configured via `appsettings.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=RecipeOptimizer;Username=postgres;Password=password"
  }
}
```

## License

MIT License - see LICENSE file for details.
