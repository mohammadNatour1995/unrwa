using Application.Interfaces.Logging;
using Application.Interfaces.Utilities;
using Domain.Dtos;
using Domain.Dtos.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using static Domain.Enums.ApplicationEnum;

namespace Application.Services.Utilities
{
    public class FileManager : IFileManager
    {
        private readonly IWebHostEnvironment _Environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerManager<FileManager> _loggerManager;
        public FileManager(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILoggerManager<FileManager> loggerManager)
        {
            _loggerManager = loggerManager;
            _Environment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public BaseResponse<UploadFileResponseDto> UploadFile(IFormFile file, string folderPath)
        {
            var result = new BaseResponse<UploadFileResponseDto>();
            try
            {
                var tempFolderPath = folderPath;

                if (tempFolderPath.StartsWith("/"))
                    tempFolderPath = tempFolderPath.Remove(0, 1);

                string basePath = Path.Combine(_Environment.WebRootPath, tempFolderPath);

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                if (!GenerateFileName(Path.GetExtension(file.FileName), out string generatedFileName))
                {
                    result.Header = new BaseHeader
                    {
                        Status = ResponseStatus.Error,
                        Message = "An error occurred while generating file name."
                    };

                    return result;
                }

                using (FileStream stream = new FileStream(Path.Combine(basePath, generatedFileName), FileMode.Create))
                    file.CopyTo(stream);

                result.Data = new UploadFileResponseDto
                {
                    UploadedFilePath = $"{tempFolderPath}/{generatedFileName}"
                };

                result.Header = new BaseHeader
                {
                    Status = ResponseStatus.Success,
                    Message = "File uploaded successfully."
                };

            }
            catch (Exception ex)
            {
                result.Header = new BaseHeader
                {
                    Status = ResponseStatus.Error,
                    Message = "An error occurred while uploading file."
                };
                _loggerManager.Error(ex, new { file =file, folderPath =folderPath });
            }

            return result;
        }

        public string GetFileHostedUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;
            return $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/{filePath}";
        }

        #region Helper
        private bool GenerateFileName(string fileType, out string generatedFileName)
        {
            bool result;
            generatedFileName = string.Empty;

            try
            {
                generatedFileName = Guid.NewGuid().ToString("N") + fileType;
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
        #endregion
    }
}
