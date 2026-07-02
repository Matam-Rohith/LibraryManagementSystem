# AWS ECS Fargate Deployment Guide

This guide walks you through deploying the Library Management System to AWS ECS Fargate.

## Prerequisites
- AWS CLI configured (`aws configure`)
- Docker installed
- An AWS account

## Architecture
```
Internet → ALB (port 80/443) → ECS Fargate (port 8080)
                                    ↓              ↓
                              RDS PostgreSQL    MongoDB Atlas
                              (ap-south-1)     (free tier)
```

## Step 1: Create ECR Repository
```bash
aws ecr create-repository --repository-name library-management-system --region ap-south-1
```

## Step 2: Build & Push Docker Image
```bash
# Authenticate
aws ecr get-login-password --region ap-south-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com

# Build & tag
docker build -t library-management-system .
docker tag library-management-system:latest YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com/library-management-system:latest

# Push
docker push YOUR_ACCOUNT.dkr.ecr.ap-south-1.amazonaws.com/library-management-system:latest
```

## Step 3: Create RDS PostgreSQL
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

## Step 4: Store Secrets in AWS Secrets Manager
```bash
aws secretsmanager create-secret --name library/db-connection \
  --secret-string "Host=YOUR_RDS_ENDPOINT;Database=LibraryDb;Username=postgres;Password=YOUR_PASSWORD"

aws secretsmanager create-secret --name library/mongo-connection \
  --secret-string "YOUR_MONGODB_ATLAS_CONNECTION_STRING"

aws secretsmanager create-secret --name library/jwt-key \
  --secret-string "YOUR_SUPER_SECURE_32_CHAR_JWT_KEY"
```

## Step 5: Register ECS Task Definition
```bash
# Update task-definition.json with your account ID first
aws ecs register-task-definition --cli-input-json file://aws/task-definition.json --region ap-south-1
```

## Step 6: Create ECS Cluster & Service
```bash
# Create cluster
aws ecs create-cluster --cluster-name library-cluster --region ap-south-1

# Create service (update subnet/security group IDs)
aws ecs create-service \
  --cluster library-cluster \
  --service-name library-api \
  --task-definition library-management-system \
  --desired-count 1 \
  --launch-type FARGATE \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-XXXXX],securityGroups=[sg-XXXXX],assignPublicIp=ENABLED}" \
  --region ap-south-1
```

## GitHub Actions Auto-Deploy
Add these secrets to your GitHub repository:
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `AWS_REGION` = `ap-south-1`
- `ECR_REPOSITORY` = your ECR URI

## Free Tier Alternative: Render.com
The existing `render.yaml` in the repo deploys automatically from GitHub.
Just connect your repo at https://render.com — no credit card needed.

## MongoDB
Use [MongoDB Atlas Free Tier](https://www.mongodb.com/cloud/atlas) (512MB free) for production activity logs.
