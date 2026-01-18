# Controllers

## Overview
Controllers are thin adapters that accept HTTP requests, validate inputs (model binding + FluentValidation), and forward to application services. They should not contain domain logic.

## AuthController
- `POST /api/v1/auth/register` - Accepts `RegisterUserRequest` and returns `RegisterUserResponse` wrapped in `ApiResponse<T>` with 201 Created.

## Logging
Controllers log high-level events such as attempts and successful operations. Sensitive information (passwords) must not be logged.
