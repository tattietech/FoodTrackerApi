# Food Tracker API Lambda Function

This Lambda function serves as the backend API for the Food Tracker application, handling CRUD operations for food items, storage locations, households, and user management.

## Project Status

**This is an experimental project created during AWS Developer Exam studies and is not maintained or live.**

## Components

* **Function.cs** - Main Lambda function handler that processes API Gateway HTTP requests
* **Models/** - Data models for food items, storage, households, and users
* **Services/** - DynamoDB service layer for data operations
* **Interfaces/** - Service and model interfaces
* **aws-lambda-tools-defaults.json** - AWS Lambda deployment configuration

## Architecture

The function integrates with:
- **API Gateway** - HTTP API endpoints
- **DynamoDB** - NoSQL database for storing all application data
- **Cognito** - User authentication and authorization

## API Endpoints

The Lambda function handles the following resource paths:
- `/food` - Food item management
- `/storage` - Storage location management
- `/household` - Household management
- `/invite` - Household invitation system
- `/user` - User information

All requests require authentication via the `authorization` header.
