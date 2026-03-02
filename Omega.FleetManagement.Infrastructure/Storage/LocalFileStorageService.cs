using Microsoft.Extensions.Configuration;
using Omega.FleetManagement.Domain.Interfaces;

namespace Omega.FleetManagement.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            // Pega o caminho base do appsettings.json (ex: "C:\\Storage" ou "/var/www/storage")
            _basePath = configuration["StorageConfig:BasePath"] ?? "uploads";
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string subFolder)
        {
            // Criamos o caminho: uploads/empresa_id/2026/02
            var relativePath = Path.Combine(subFolder, DateTime.Now.ToString("yyyy/MM"));
            var fullPath = Path.Combine(_basePath, relativePath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            // Geramos um nome único para evitar sobrescrever arquivos
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(fullPath, uniqueFileName);

            using (var targetStream = File.Create(filePath))
            {
                await fileStream.CopyToAsync(targetStream);
            }

            // Retornamos o caminho relativo para salvar no banco de dados
            return Path.Combine(relativePath, uniqueFileName);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_basePath, filePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }
    }
}
