namespace SSToDo.Utilities
{
    public interface IFileService
    {
        Task<string> AddImageAsync(IFormFile file, string folderName);
    }

    public class FileService : IFileService
    {
        private static readonly HashSet<string> _permittedExtensionsForImage = new(){
        ".jpg", ".jpeg", ".png" , "tiff" , "webp" , "heic" , "ico"
        };

        public async Task<string> AddImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return "No file sent.";

            if (folderName == null)
                return "Please enter folder name.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !_permittedExtensionsForImage.Contains(extension))
                return "Unsupported file type.";

            var uploadFolder = Path.Combine($"F:\\FTProot\\Afshin\\{folderName}");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);


            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileurl = $"{folderName}/{fileName}";

            return fileurl;
        }
    }
}
