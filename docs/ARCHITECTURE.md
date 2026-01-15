# Architecture

This solution follows a clean architecture style with clear separation of concerns.

## Layers
- `Domain` – enterprise business rules, entities, enums, and core interfaces
- `Application` – use cases, DTOs, validators, and orchestration logic
- `Infrastructure` – data access, external services, and technical details
- `API` – HTTP endpoints, middleware, and presentation

## Principles
- Dependencies point inward toward the domain
- Outer layers depend on inner layers, never the reverse
- Domain remains free of infrastructure and framework concerns
- Each project exposes clear, stable contracts
