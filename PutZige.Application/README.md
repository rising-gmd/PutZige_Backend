# PutZige.Application

Application layer orchestrating use cases.

## Contains
- `DTOs/` – data transfer objects for input and output
- `Services/` – application services and use case handlers
- `Interfaces/` – contracts used by the application layer
- `Validators/` – request and DTO validation logic

## Guidelines
- Depends on `Domain`, but not on `Infrastructure`
- Contains application-specific business rules
- Keep code testable and framework-agnostic
