using GymManagement.BLL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.BLL.Services.Attachment
{
    public interface IAttachmentService
    {
        Task<Result<string>> UploadAsync(Stream fileStream, string fileName, string folderName, CancellationToken ct = default);
        Result Delete(string fileName, string folderName);
        Result<(Stream stream, string contentType)> GetFile(string fileName, string folderName);
    }
}
