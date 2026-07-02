# AWS ECS Fargate Deployment

Steps to deploy the Library Management System on AWS ECS Fargate.

## Requirements
- AWS CLI configured (`aws configure`)
- Docker installed
- An AWS account

## Architecture

```
Internet -> ALB (port 80/443) -> ECS Fargate (port 8080)
                                      |              |
                               RDS PostgreSQL    MongoDB Atlas
```

## Step 1: Create ECR Repository

```bash
aws ecr create-repository --repository-name library-management-system --region ap-south-1
```

## Step 2: Build and Push Docker Image

```bash
aws ecr get-login-password --region ap-south-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com

docker build -t library-management-system .
docker tag library-management-system:latest YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com/library-management-system:latest
docker push YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com/library-management-system:latest
```

## Step 3: Create RDS PostgreSQL Instance

```bash
aws rds create-db-instance \
  --db-instance-identifier library-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --master-username postgres \
  --master-user-password YOUR_SECURE_PASSWORD \
  --allocated-storage 20 \
  --region ap-south-1
```

## Step 4: Store Secrets

```bash
aws secretsmanager create-secret --name library/db-connection \
  --secret-string "Host=YOUR_RDS_ENDPOINT;Database=LibraryDb;Username=postgres;Password=YOUR_PASSWORD"

aws secretsmanager create-secret --name library/mongo-connection \
  --secret-string "YOUR_MONGODB_ATLAS_CONNECTION_STRING"

aws secretsmanager create-secret --name library/jwt-key \
  --secret-string "YOUR_JWT_KEY_MIN_32_CHARS"
```

## Step 5: Register ECS Task Definition

Update `aws/task-definition.json` with your AWS account ID, then run:

```bash
aws ecs register-task-definition --cli-input-json file://aws/task-definition.json --region ap-south-1
```

## Step 6: Create ECS Cluster and Service

```bash
aws ecs create-cluster --cluster-name library-cluster --region ap-south-1

aws ecs create-service \
  --cluster library-cluster \
  --service-name library-api \
  --task-definition library-management-system \
  --desired-count 1 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-XXXXX],securityGroups=[sg-XXXXX],assignPublicIp=ENABLED}" \
  --region ap-south-1
```

## GitHub Secrets for CI/CD

Add these to your GitHub repository settings under Actions secrets:
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION` — set to `ap-south-1`
- `ECR_REPOSITORY` — your ECR repository URI

## Alternative: Render.com

The `render.yaml` in the repo root deploys automatically when you connect the repo at https://render.com.

## MongoDB

Use [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) free tier (512 MB) for the activity logs database.
