using Microsoft.AspNetCore.Http;

namespace RecipesApi.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder = "recipes");
    void DeleteFile(string? relativeUrl);
}
