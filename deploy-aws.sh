#!/bin/bash

# AWS Deployment Script for Recipe Optimizer

# Prerequisites:
# - AWS CLI installed and configured
# - AWS Elastic Beanstalk CLI installed
# - Docker installed

echo "Building Docker image..."
docker build -t recipe-optimizer .

echo "Logging in to AWS ECR..."
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

echo "Creating ECR repository if it doesn't exist..."
aws ecr create-repository --repository-name recipe-optimizer --region us-east-1 || true

echo "Tagging Docker image..."
docker tag recipe-optimizer:latest YOUR_AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/recipe-optimizer:latest

echo "Pushing Docker image to ECR..."
docker push YOUR_AWS_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/recipe-optimizer:latest

echo "Initializing Elastic Beanstalk application..."
eb init recipe-optimizer --platform docker --region us-east-1

echo "Creating Elastic Beanstalk environment..."
eb create recipe-optimizer-env --instance_type t2.micro --single --timeout 20

echo "Deployment complete!"
echo "Your application should be available at: http://recipe-optimizer-env.elasticbeanstalk.com"
