# Deploy em Produção (Azure com baixo custo)

## 1) Recursos mínimos no Azure

- `Azure SQL Database` (`General Purpose - Serverless`)
- `Azure Blob Storage` (container `omega-fleet-files`)
- `Azure App Service` (Linux) para API `.NET`
- `Azure Static Web Apps` para frontend Angular

## 2) Configuração da API (App Service > Configuration)

Defina estas **Application settings**:

- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ConnectionStrings__DefaultConnection` = `Server=tcp:<sql-server>.database.windows.net,1433;Initial Catalog=<database-name>;Persist Security Info=False;User ID=<admin-user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
- `Jwt__Key` = `<jwt-key-forte>`
- `Jwt__Issuer` = `OmegaFleetAPI`
- `Jwt__Audience` = `OmegaFleetAngularApp`
- `Jwt__DurationInMinutes` = `480`
- `Cors__AllowedOrigins__0` = `https://<seu-frontend>.azurestaticapps.net`
- `StorageConfig__Provider` = `AzureBlob`
- `StorageConfig__AzureBlob__ConnectionString` = `<connection-string-blob>`
- `StorageConfig__AzureBlob__ContainerName` = `omega-fleet-files`

Importante:

- Não publique segredos em `appsettings.json`, `appsettings.Production.json` ou no frontend.
- Se alguma connection string, senha de banco ou chave JWT já foi commitada anteriormente, faça a rotação antes do deploy.
- Em desenvolvimento local, prefira `dotnet user-secrets` ou variáveis de ambiente para `ConnectionStrings__DefaultConnection` e `Jwt__Key`.
- Mantenha `BootstrapAdmin__Enabled=false` em produção. Se precisar criar um `Master`, faça isso de forma controlada e temporária, com `BootstrapAdmin__Email` e `BootstrapAdmin__Password` vindos de secret externo.
- Para o banco, prefira `Azure SQL Database - Serverless` com autopause habilitado para reduzir custo em ambientes de baixa utilização.

## 3) Secrets no GitHub Actions

### Workflow da API (`.github/workflows/api-deploy.yml`)

- `AZURE_WEBAPP_NAME_API`
- `AZURE_WEBAPP_PUBLISH_PROFILE_API`

### Workflow do Web (`.github/workflows/web-deploy.yml`)

- `AZURE_STATIC_WEB_APPS_API_TOKEN`
- `WEB_API_BASE_URL` (exemplo: `https://api.seudominio.com/api/v1`)

## 4) Banco de dados (migrações)

Antes do primeiro uso em produção, aplique as migrações no banco:

```bash
dotnet ef database update --project Omega.FleetManagement.Infrastructure --startup-project Omega.FleetManagement.API
```

Observação:

- A solução agora usa `SQL Server / Azure SQL` como provider EF Core.
- Se você estiver em uma máquina nova, execute antes:

```bash
dotnet tool restore
```

## 5) Observações

- `appsettings.Production.json` já foi criado com placeholders.
- `appsettings.json` também deve permanecer sem segredos reais versionados.
- O bootstrap do usuário `Master` agora depende de configuração explícita e não deve permanecer ativo após a criação inicial.
- As migrations antigas de PostgreSQL foram substituídas por uma migration inicial nova para SQL Server/Azure SQL.
- O upload de arquivos agora suporta provider configurável:
  - `Local` (desenvolvimento)
  - `AzureBlob` (produção)
- Frontend usa `environment.prod.ts` com `apiBaseUrl` injetado no pipeline via secret.
