# Omega.FleetManagement

Guia de deploy Azure: `DEPLOYMENT_AZURE.md`.

## Configuração segura

- Não versionar credenciais de banco, chave JWT ou secrets de storage.
- Para desenvolvimento local, configurar `ConnectionStrings__DefaultConnection` e `Jwt__Key` via variáveis de ambiente ou `dotnet user-secrets`.
- Para produção, usar apenas App Settings/Secrets do provedor de hospedagem.

## Banco atual

- A solução está configurada para `SQL Server / Azure SQL Database`.
- Para aplicar migrations localmente, execute `dotnet tool restore` e depois `dotnet ef database update --project Omega.FleetManagement.Infrastructure --startup-project Omega.FleetManagement.API`.
