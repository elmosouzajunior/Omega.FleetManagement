# Omega.FleetManagement

Guia de deploy Azure: `DEPLOYMENT_AZURE.md`.

## Configuração segura

- Não versionar credenciais de banco, chave JWT ou secrets de storage.
- Para desenvolvimento local, configurar `ConnectionStrings__DefaultConnection` e `Jwt__Key` via variáveis de ambiente ou `dotnet user-secrets`.
- Para produção, usar apenas App Settings/Secrets do provedor de hospedagem.
