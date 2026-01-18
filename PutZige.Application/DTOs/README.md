# DTOs

## Pattern
DTOs are simple immutable records or classes that represent the shape of data crossing application boundaries (API ? Application).

## Structure
- `Common/ApiResponse.cs` - wrapper for all API responses with success flag, message, errors and timestamp
- `Auth/` - authentication related DTOs such as `RegisterUserRequest`, `RegisterUserResponse`, `LoginRequest` (placeholder)

## Examples
Register request:
```
{
  "email": "user@example.com",
  "username": "johndoe",
  "displayName": "John Doe",
  "password": "P@ssw0rd!",
  "confirmPassword": "P@ssw0rd!"
}
```

Register response:
```
{
  "success": true,
  "data": {
    "userId": "...",
    "email": "user@example.com",
    "username": "johndoe",
    "displayName": "John Doe",
    "isEmailVerified": false,
    "createdAt": "2025-01-01T00:00:00Z"
  },
  "message": "Registration successful. Please check your email to verify your account.",
  "timestamp": "..."
}
```
