# PutZige.Infrastructure

Infrastructure layer for technical concerns.

## Contains
- `Data/` – data access configuration and context
- `Repositories/` – repository implementations
- `Services/` – infrastructure service implementations

## Guidelines
- Depends on `Domain` and `Application` contracts
- Keep external dependencies isolated here
- Avoid leaking infrastructure types into inner layers
