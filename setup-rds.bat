@echo off
echo Setting up AWS RDS PostgreSQL database for Recipe Optimizer

REM Replace these variables with your own values
set AWS_REGION=us-east-1
set DB_INSTANCE_IDENTIFIER=recipe-optimizer-db
set DB_INSTANCE_CLASS=db.t3.micro
set DB_ENGINE=postgres
set DB_ENGINE_VERSION=14.6
set DB_NAME=recipeoptimizer
set DB_USERNAME=postgres
set DB_PASSWORD=YourStrongPassword
set DB_ALLOCATED_STORAGE=20
set DB_SECURITY_GROUP=recipe-optimizer-sg

echo Creating security group for database access...
aws ec2 create-security-group --group-name %DB_SECURITY_GROUP% --description "Security group for Recipe Optimizer RDS" --region %AWS_REGION%

echo Adding inbound rule to allow PostgreSQL traffic...
aws ec2 authorize-security-group-ingress --group-name %DB_SECURITY_GROUP% --protocol tcp --port 5432 --cidr 0.0.0.0/0 --region %AWS_REGION%

echo Creating RDS PostgreSQL database...
aws rds create-db-instance ^
  --db-instance-identifier %DB_INSTANCE_IDENTIFIER% ^
  --db-instance-class %DB_INSTANCE_CLASS% ^
  --engine %DB_ENGINE% ^
  --engine-version %DB_ENGINE_VERSION% ^
  --master-username %DB_USERNAME% ^
  --master-user-password %DB_PASSWORD% ^
  --allocated-storage %DB_ALLOCATED_STORAGE% ^
  --db-name %DB_NAME% ^
  --vpc-security-group-ids %DB_SECURITY_GROUP% ^
  --publicly-accessible ^
  --region %AWS_REGION%

echo Waiting for database to be available...
aws rds wait db-instance-available --db-instance-identifier %DB_INSTANCE_IDENTIFIER% --region %AWS_REGION%

echo Getting database endpoint information...
aws rds describe-db-instances --db-instance-identifier %DB_INSTANCE_IDENTIFIER% --query "DBInstances[0].Endpoint" --region %AWS_REGION%

echo Database setup complete!
echo Please update your Elastic Beanstalk environment variables with the RDS connection information.
echo You can do this through the AWS Management Console or by using the AWS CLI.
