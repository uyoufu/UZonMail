using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Files.DTOs;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Files
{
    /// <summary>
    /// 用户文件上传、读取和逻辑文件管理接口。
    /// </summary>
    public sealed class FileController(
        FileStoreService fileStoreService,
        FileUsageService fileUsageService,
        TokenService tokenService,
        DebugConfig debugConfig
    ) : ControllerBaseV1
    {
        [HttpGet("file-id")]
        public async Task<ResponseResult<long>> GetFileId(
            string sha256,
            string fileName,
            CancellationToken cancellationToken
        )
        {
            var fileObject = await fileStoreService.GetExistFileObjectAsync(
                sha256,
                cancellationToken
            );
            if (fileObject is null)
                return (-1L).ToSuccessResponse();
            var usage = await fileStoreService.GetOrCreateFileUsageAsync(
                tokenService.GetUserSqlId(),
                fileName,
                sha256,
                cancellationToken
            );
            return usage.Id.ToSuccessResponse();
        }

        [HttpPost("upload-file-object")]
        public async Task<ResponseResult<FileUploadResult>> UploadFileObject(
            ObjectFileUploaderBody fileParams,
            CancellationToken cancellationToken
        )
        {
            if (debugConfig.IsDemo)
                return new FileUploadResult(0, 0, false).ToFailResponse("演示环境不支持上传文件");
            fileParams.File ??= Request.Form.Files.FirstOrDefault();
            var result = await fileStoreService.UploadFileObjectAsync(
                tokenService.GetUserSqlId(),
                fileParams,
                cancellationToken
            );
            return result.ToSuccessResponse();
        }

        [AllowAnonymous]
        [HttpGet("public-file-stream/{fileUsageId:long}")]
        public async Task<IActionResult> GetPublicFileStream(
            long fileUsageId,
            CancellationToken cancellationToken
        ) =>
            CreateFileStreamResult(
                await fileStoreService.GetPublicFileFullPathAsync(fileUsageId, cancellationToken)
            );

        [HttpGet("file-stream/{fileUsageId:long}")]
        public async Task<IActionResult> GetFileStream(
            long fileUsageId,
            CancellationToken cancellationToken
        ) =>
            CreateFileStreamResult(
                await fileStoreService.GetOwnedFileFullPathAsync(
                    fileUsageId,
                    tokenService.GetUserSqlId(),
                    cancellationToken
                )
            );

        [HttpPost("upload-static-file")]
        public async Task<ResponseResult<string>> UploadToStaticFile(
            StaticFileUploaderBody fileParams,
            CancellationToken cancellationToken
        )
        {
            var userId = tokenService.GetUserSqlId();
            var safeFileName = Path.GetFileName(fileParams.File.FileName);
            var (fullPath, relativePath) = fileStoreService.GenerateStaticFilePath(
                userId.ToString(),
                fileParams.SubPath,
                safeFileName
            );
            await using var stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous
            );
            await fileParams.File.CopyToAsync(stream, cancellationToken);
            return relativePath.ToSuccessResponse();
        }

        [HttpGet("file-usages/filtered-count")]
        public async Task<ResponseResult<int>> GetFileUsagesCount(
            long? categoryId,
            string? filter,
            CancellationToken cancellationToken
        ) =>
            (
                await fileUsageService.GetCountAsync(
                    tokenService.GetUserSqlId(),
                    categoryId,
                    filter,
                    cancellationToken
                )
            ).ToSuccessResponse();

        [HttpPost("file-usages/filtered-data")]
        public async Task<ResponseResult<List<FileUsageListItem>>> GetFileUsagesData(
            long? categoryId,
            string? filter,
            [FromBody] Pagination pagination,
            CancellationToken cancellationToken
        ) =>
            (
                await fileUsageService.GetDataAsync(
                    tokenService.GetUserSqlId(),
                    categoryId,
                    filter,
                    pagination,
                    cancellationToken
                )
            ).ToSuccessResponse();

        [HttpDelete("file-usages/{fileUsageId:long}")]
        public async Task<ResponseResult<FileUsageDeletionResult>> DeleteFileUsage(
            long fileUsageId,
            CancellationToken cancellationToken
        ) =>
            (
                await fileUsageService.DeleteAsync(
                    tokenService.GetUserSqlId(),
                    [fileUsageId],
                    cancellationToken
                )
            ).ToSuccessResponse();

        [HttpDelete("file-usages/ids/many")]
        public async Task<ResponseResult<FileUsageDeletionResult>> DeleteFileUsages(
            [FromBody] DeleteFileUsagesDto request,
            CancellationToken cancellationToken
        ) =>
            (
                await fileUsageService.DeleteAsync(
                    tokenService.GetUserSqlId(),
                    request.FileUsageIds,
                    cancellationToken
                )
            ).ToSuccessResponse();

        [HttpPut("file-usages/category")]
        public async Task<ResponseResult<bool>> MoveFileUsages(
            [FromBody] MoveFileUsagesDto request,
            CancellationToken cancellationToken
        )
        {
            await fileUsageService.MoveToCategoryAsync(
                tokenService.GetUserSqlId(),
                request.FileUsageIds,
                request.CategoryId,
                cancellationToken
            );
            return true.ToSuccessResponse();
        }

        [HttpPut("file-usages/{fileUsageId:long}/display-name")]
        public async Task<ResponseResult<bool>> UpdateDisplayName(
            long fileUsageId,
            [FromQuery] string displayName,
            CancellationToken cancellationToken
        )
        {
            await fileUsageService.RenameAsync(
                tokenService.GetUserSqlId(),
                fileUsageId,
                displayName,
                cancellationToken
            );
            return true.ToSuccessResponse();
        }

        private static FileStreamResult CreateFileStreamResult(string fullPath)
        {
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(fullPath),
            };
        }
    }
}
