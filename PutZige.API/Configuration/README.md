# Configuration

This document explains environment-specific settings and best practices for configuration.

Environments:
- Development: Use for local developer work. Enable detailed logs and sensitive data logging. Use User Secrets for connection strings.
- Test: Use for CI and automated tests. Keep logging limited and use a test database.
- QA: Use for quality assurance testing before staging.
- Staging: Use for pre-production validation. Mirror production settings where possible.
- Production: Use for live traffic. Do not enable sensitive data logging.
- Release: Use for release builds and automated deployments.

Configuration loading:
- `appsettings.json` is loaded first
- `appsettings.{Environment}.json` is overlaid next
- Environment variables override file-based settings
- Secrets from User Secrets or Azure Key Vault should be configured for sensitive values

Secrets best practices:
- Never commit production connection strings or secrets to source control
- Use User Secrets during local development: `dotnet user-secrets` or IDE secrets storage
- Use managed secret stores (Azure Key Vault, AWS Secrets Manager) in pipelines and production

When to use which environment:
- Local dev: `Development`
- CI: `Test`
- Manual QA testing: `QA`
- Pre-release: `Staging`
- Live: `Production`
