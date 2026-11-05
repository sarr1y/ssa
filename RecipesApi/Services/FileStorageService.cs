namespace RecipesApi.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder = "recipes")
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Файл не выбран или пуст.");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new ArgumentException("Размер файла превышает допустимый предел в 5 МБ.");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"Недопустимый формат файла '{ext}'. Разрешены только: {string.Join(", ", AllowedExtensions)}.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var targetDir = Path.Combine(webRoot, "uploads", subFolder);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // Имя файла на сервере — строго Guid
        var fileName = $"{Guid.NewGuid():D}{ext}";
        var filePath = Path.Combine(targetDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public void DeleteFile(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return;

        try
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var trimmed = relativeUrl.TrimStart('/', '\\');
            var fullPath = Path.Combine(webRoot, trimmed);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Игнорируем ошибки при удалении отсутствующего файла
        }
    }
}
