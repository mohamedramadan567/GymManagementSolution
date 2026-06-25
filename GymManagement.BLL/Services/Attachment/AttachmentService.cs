using GymManagement.BLL.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Attachment
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ILogger<IAttachmentService> _logger;
        private readonly IWebHostEnvironment _env;

        private readonly long _maxFileSize = 1024 * 1024;
        private readonly string[] allowedExtensions = {".jpg", ".jpeg", ".png"};
        public AttachmentService(ILogger<IAttachmentService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this._env = env;
        }

        public Result Delete(string fileName, string folderName)
        {
            //1 Build the file path from the folder name and the stored file name.
            var fullPath = Path.Combine(_env.ContentRootPath, folderName, fileName);

            try
            {
                //2 If the file exists, remove it — otherwise do nothing and report failure.
                if (!File.Exists(fullPath))
                {
                    return Result.NotFound("Attachment Not Found");
                }

                File.Delete(fullPath);
                return Result.OK();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed To Delete Attachment {fileName}");
                return Result.Fail($"Failed To Delete Attachment {fileName}");
            }
        }

        public Result<(Stream stream, string contentType)> GetFile(string fileName, string folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folderName))
                return Result<(Stream, string)>.Fail("File name or folder name is invalid.");

            var fullPath = Path.Combine(_env.ContentRootPath, folderName, fileName);
            if (!File.Exists(fullPath))
                return Result<(Stream, string)>.Fail("File not found.");

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var extension = Path.GetExtension(fullPath).ToLower();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream" // Binary Data
            };

            return Result<(Stream, string)>.OK((stream, contentType));
        }
        public async Task<Result<string>> UploadAsync(Stream fileStream, string fileName, string folderName, CancellationToken ct = default)
        {
            if (fileStream == null || !fileStream.CanRead) return Result<string>.NotFound("file stream not found or can't read");
            if (fileStream.Length == 0) return Result<string>.Fail("cannot upload photo with length 0");

            //1 Check the extension — only .jpg .jpeg .png allowed.
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            {
                _logger.LogError($"File Rejected: {extension} Not Allowed");
                return Result<string>.Fail($"File Rejected: {extension} Not Allowed");
            }

            //2 Check the size — reject anything over 5 MB.
            if(fileStream.Length > _maxFileSize)
            {
                _logger.LogError($"File Rejected: File Too Large {fileStream.Length} Bytes");
                return Result<string>.Fail("Size must be less than 5 MB");
            }

            //3 Locate the folder & create it if missing.
            var uploadsFolder = Path.Combine(_env.ContentRootPath, folderName);
            Directory.CreateDirectory(uploadsFolder);

            //4 Make the name unique using a GUID.
            var storedFileName = $"{Guid.NewGuid()}{fileName}";

            //5 Build the full file path.
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            try
            {
                //6 Open a file stream (an unmanaged resource).
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                //7 Copy the file into that stream.
                await fileStream.CopyToAsync(fs, ct);
                //8 Return the file name to store in the database.
                return Result<string>.OK(storedFileName);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to Upload File {fileName}");
                return Result<string>.Fail("Failed to upload");
            }

        }
    }
}
