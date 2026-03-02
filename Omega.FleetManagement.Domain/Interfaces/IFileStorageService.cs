namespace Omega.FleetManagement.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string subFolder);
        Task DeleteFileAsync(string filePath);
    }
}
