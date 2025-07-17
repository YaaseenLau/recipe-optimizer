#!/bin/bash

# AWS Deployment Script for Recipe Optimizer API
# Replace these variables with your own values
AWS_REGION="us-east-1"
AWS_ACCOUNT_ID="your-aws-account-id"  # Replace with your actual AWS account ID
ECR_REPOSITORY_NAME="recipe-optimizer"
EB_APPLICATION_NAME="recipe-optimizer"
EB_ENVIRONMENT_NAME="recipe-optimizer-prod"
EB_INSTANCE_TYPE="t2.micro"

# Step 1: Build the Docker image
echo "Building Docker image..."
docker build -t $ECR_REPOSITORY_NAME:latest .

# Step 2: Authenticate Docker to ECR
echo "Authenticating with AWS ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com

# Step 3: Create ECR repository if it doesn't exist
echo "Creating ECR repository if it doesn't exist..."
aws ecr describe-repositories --repository-names $ECR_REPOSITORY_NAME --region $AWS_REGION || \
aws ecr create-repository --repository-name $ECR_REPOSITORY_NAME --region $AWS_REGION

# Step 4: Tag and push the Docker image to ECR
echo "Tagging and pushing Docker image to ECR..."
docker tag $ECR_REPOSITORY_NAME:latest $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPOSITORY_NAME:latest
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPOSITORY_NAME:latest

# Step 5: Create Dockerrun.aws.json file for Elastic Beanstalk
echo "Creating Dockerrun.aws.json file..."
cat > Dockerrun.aws.json << EOF
{
  "AWSEBDockerrunVersion": "1",
  "Image": {
    "Name": "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPOSITORY_NAME:latest",
    "Update": "true"
  },
  "Ports": [
    {
      "ContainerPort": 80,
      "HostPort": 80
    }
  ],
  "Volumes": [
    {
      "HostDirectory": "/var/app/current/postgres-data",
      "ContainerDirectory": "/var/lib/postgresql/data"
    }
  ],
  "Logging": "/var/log/nginx"
}
EOF

# Step 6: Initialize Elastic Beanstalk application (if not already done)
echo "Initializing Elastic Beanstalk application..."
if ! aws elasticbeanstalk describe-applications --application-names $EB_APPLICATION_NAME --region $AWS_REGION > /dev/null 2>&1; then
  eb init $EB_APPLICATION_NAME --platform "Docker" --region $AWS_REGION
else
  echo "Application $EB_APPLICATION_NAME already exists."
fi

# Step 7: Create or update Elastic Beanstalk environment
echo "Checking if environment exists..."
if ! aws elasticbeanstalk describe-environments --environment-names $EB_ENVIRONMENT_NAME --region $AWS_REGION | grep -q $EB_ENVIRONMENT_NAME; then
  echo "Creating new Elastic Beanstalk environment..."
  eb create $EB_ENVIRONMENT_NAME --instance_type $EB_INSTANCE_TYPE --single --timeout 20
else
  echo "Updating existing Elastic Beanstalk environment..."
  eb deploy $EB_ENVIRONMENT_NAME
fi

# Step 8: Create RDS PostgreSQL database if needed
# Uncomment and configure if you want to create an RDS instance
# echo "Creating RDS PostgreSQL database..."
# aws rds create-db-instance \
#   --db-instance-identifier recipe-optimizer-db \
#   --db-instance-class db.t3.micro \
#   --engine postgres \
#   --master-username postgres \
#   --master-user-password "YourStrongPassword" \
#   --allocated-storage 20 \
#   --region $AWS_REGION

echo "Deployment complete!"
echo "Your application should be available at: http://$EB_ENVIRONMENT_NAME.$AWS_REGION.elasticbeanstalk.com"
