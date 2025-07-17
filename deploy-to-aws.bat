@echo off
echo AWS Deployment Script for Recipe Optimizer API

REM Replace these variables with your own values
set AWS_REGION=us-east-1
set AWS_ACCOUNT_ID=your-aws-account-id
set ECR_REPOSITORY_NAME=recipe-optimizer
set EB_APPLICATION_NAME=recipe-optimizer
set EB_ENVIRONMENT_NAME=recipe-optimizer-prod
set EB_INSTANCE_TYPE=t2.micro

echo Building Docker image...
docker build -t %ECR_REPOSITORY_NAME%:latest .

echo Authenticating with AWS ECR...
for /f "tokens=*" %%i in ('aws ecr get-login-password --region %AWS_REGION%') do (
    docker login --username AWS --password-stdin %AWS_ACCOUNT_ID%.dkr.ecr.%AWS_REGION%.amazonaws.com
)

echo Creating ECR repository if it doesn't exist...
aws ecr describe-repositories --repository-names %ECR_REPOSITORY_NAME% --region %AWS_REGION% 2>nul || (
    aws ecr create-repository --repository-name %ECR_REPOSITORY_NAME% --region %AWS_REGION%
)

echo Tagging and pushing Docker image to ECR...
docker tag %ECR_REPOSITORY_NAME%:latest %AWS_ACCOUNT_ID%.dkr.ecr.%AWS_REGION%.amazonaws.com/%ECR_REPOSITORY_NAME%:latest
docker push %AWS_ACCOUNT_ID%.dkr.ecr.%AWS_REGION%.amazonaws.com/%ECR_REPOSITORY_NAME%:latest

echo Creating Dockerrun.aws.json file...
(
echo {
echo   "AWSEBDockerrunVersion": "1",
echo   "Image": {
echo     "Name": "%AWS_ACCOUNT_ID%.dkr.ecr.%AWS_REGION%.amazonaws.com/%ECR_REPOSITORY_NAME%:latest",
echo     "Update": "true"
echo   },
echo   "Ports": [
echo     {
echo       "ContainerPort": 80,
echo       "HostPort": 80
echo     }
echo   ],
echo   "Volumes": [
echo     {
echo       "HostDirectory": "/var/app/current/postgres-data",
echo       "ContainerDirectory": "/var/lib/postgresql/data"
echo     }
echo   ],
echo   "Logging": "/var/log/nginx"
echo }
) > Dockerrun.aws.json

echo Initializing Elastic Beanstalk application...
aws elasticbeanstalk describe-applications --application-names %EB_APPLICATION_NAME% --region %AWS_REGION% >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    eb init %EB_APPLICATION_NAME% --platform "Docker" --region %AWS_REGION%
) else (
    echo Application %EB_APPLICATION_NAME% already exists.
)

echo Checking if environment exists...
aws elasticbeanstalk describe-environments --environment-names %EB_ENVIRONMENT_NAME% --region %AWS_REGION% | findstr %EB_ENVIRONMENT_NAME% >nul
if %ERRORLEVEL% NEQ 0 (
    echo Creating new Elastic Beanstalk environment...
    eb create %EB_ENVIRONMENT_NAME% --instance_type %EB_INSTANCE_TYPE% --single --timeout 20
) else (
    echo Updating existing Elastic Beanstalk environment...
    eb deploy %EB_ENVIRONMENT_NAME%
)

echo Deployment complete!
echo Your application should be available at: http://%EB_ENVIRONMENT_NAME%.%AWS_REGION%.elasticbeanstalk.com
