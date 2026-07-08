using Domain.Dtos;
using Domain.Dtos.Utilities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Utilities
{
    public interface IFileManager
    {
        BaseResponse<UploadFileResponseDto> UploadFile(IFormFile file, string folderPath);
        string GetFileHostedUrl(string filePath);
    }
}
