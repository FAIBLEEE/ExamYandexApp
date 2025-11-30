using Microsoft.AspNetCore.Http;

namespace ExamYandexApp.Services
{
    public interface IObjectStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        Task<bool> DeleteFileAsync(string fileName);
        string GetFileUrl(string fileName);
    }
}