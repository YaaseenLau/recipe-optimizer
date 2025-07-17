# Recipe Optimizer API

A .NET 7 backend API for optimizing recipe combinations to feed the maximum number of people with available ingredients.

## Project Structure

- **RecipeOptimizer.API**: Web API project with controllers and endpoints
- **RecipeOptimizer.Core**: Core domain models, interfaces, and business logic
- **RecipeOptimizer.Infrastructure**: Data access, repositories, and database context
- **RecipeOptimizer.Tests**: Unit and integration tests

## Features

- Ingredient management (CRUD operations)
- Recipe management (CRUD operations)
- Recipe optimization algorithm to maximize people fed
- Swagger UI for API documentation
- Docker containerization
- AWS deployment support

## Local Development Setup

### Prerequisites

- .NET 7 SDK
- Docker Desktop
- PostgreSQL (via Docker)

### Running Locally

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/recipe-optimizer.git
   cd recipe-optimizer
   ```

2. Start PostgreSQL using Docker:
   ```
   docker-compose up -d db
   ```

3. Apply database migrations:
   ```
   dotnet ef database update --project RecipeOptimizer.Infrastructure --startup-project RecipeOptimizer.API
   ```

4. Run the API:
   ```
   dotnet run --project RecipeOptimizer.API
   ```

5. Access the API:
   - API: https://localhost:7001
   - Swagger UI: https://localhost:7001/swagger

## Deploying to AWS via GitHub

### Prerequisites

- GitHub account
- AWS account with appropriate permissions
- AWS CLI installed and configured

### Step 1: Push to GitHub

1. Create a new repository on GitHub
2. Push your code to GitHub:
   ```
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/yourusername/recipe-optimizer.git
   git push -u origin main
   ```

### Step 2: Set Up GitHub Secrets

Add the following secrets to your GitHub repository:

1. Go to your GitHub repository → Settings → Secrets and variables → Actions
2. Add the following secrets:
   - `AWS_ACCESS_KEY_ID`: Your AWS access key
   - `AWS_SECRET_ACCESS_KEY`: Your AWS secret key
   - `AWS_REGION`: Your preferred AWS region (e.g., `us-east-1`)

### Step 3: Set Up AWS Resources

#### Create an ECR Repository

```bash
aws ecr create-repository --repository-name recipe-optimizer --region your-region
```

#### Create an RDS PostgreSQL Database

```bash
aws rds create-db-instance \
  --db-instance-identifier recipe-optimizer-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --engine-version 14.6 \
  --master-username postgres \
  --master-user-password YourStrongPassword \
  --allocated-storage 20 \
  --db-name recipeoptimizer \
  --publicly-accessible \
  --region your-region
```

#### Create an Elastic Beanstalk Application and Environment

```bash
eb init recipe-optimizer --platform docker --region your-region
eb create recipe-optimizer-prod --instance_type t2.micro --single
```

### Step 4: Configure Environment Variables

In the AWS Elastic Beanstalk Console:

1. Go to your environment → Configuration → Software
2. Add environment properties:
   - `RDS_HOSTNAME`: Your RDS endpoint
   - `RDS_PORT`: 5432
   - `RDS_DB_NAME`: recipeoptimizer
   - `RDS_USERNAME`: postgres
   - `RDS_PASSWORD`: YourStrongPassword

### Step 5: Trigger Deployment

Push changes to your main branch or manually trigger the GitHub Actions workflow:

1. Go to your GitHub repository → Actions
2. Select the "Deploy to AWS Elastic Beanstalk" workflow
3. Click "Run workflow"

## Connecting Angular Frontend

Update your Angular environment configuration to point to your Elastic Beanstalk endpoint:

```typescript
// environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://your-eb-environment-url.elasticbeanstalk.com'
};
```

## API Documentation

API documentation is available via Swagger UI at `/swagger` when the application is running.

## License

[MIT](LICENSE)
