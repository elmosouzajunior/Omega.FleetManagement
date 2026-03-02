namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IStorageService
    {
        // Retorna a URL ou caminho final do arquivo salvo
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
    }
}
