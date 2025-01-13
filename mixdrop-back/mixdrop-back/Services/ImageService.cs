using mixdrop_back.Helper;

namespace mixdrop_back.Services;

public class ImageService
{
    private const string IMAGES_FOLDER = "avatar/";

    public async Task<string> InsertAsync(IFormFile file)
    {
        string relativePath = $"{IMAGES_FOLDER}{file.FileName}";

        await StoreImageAsync(relativePath, file);

        return relativePath;
    }

    private async Task StoreImageAsync(string relativePath, IFormFile file)
    {
        using Stream stream = file.OpenReadStream();

        await FileHelper.SaveAsync(stream, relativePath);
    }
}
