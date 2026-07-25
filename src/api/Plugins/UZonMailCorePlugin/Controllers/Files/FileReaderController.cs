using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Files;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Files
{
    public class FileReaderController(
        SqlContext db,
        FileStoreService fileStoreService,
        TokenService tokenService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// 获取文件对象的下载 Id
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<long>> GetObjectAsync(long fileUsageId)
        {
            var userId = tokenService.GetUserSqlId();
            FileUsage? fileUsage = await db.FileUsages.FirstOrDefaultAsync(x =>
                x.Id == fileUsageId && x.OwnerUserId == userId
            );

            // 判断文件是否存在
            if (fileUsage == null)
                return 0L.ToFailResponse("文件不存在");

            // 生成临时读取链接
            FileReader fileReader = new(fileUsage);
            await db.FileReaders.AddAsync(fileReader);
            await db.SaveChangesAsync();

            return fileReader.Id.ToSuccessResponse();
        }

        /// <summary>
        /// 不需要授权即可访问
        /// 获取文件流
        /// </summary>
        /// <param name="fileReaderId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{fileReaderId:long}/stream")]
        public async Task<IActionResult> GetFileStream(long fileReaderId)
        {
            // 获取文件流
            var fileReader = await db
                .FileReaders.Where(x => x.Id == fileReaderId)
                .Include(x => x.FileObject)
                .ThenInclude(x => x.FileBucket)
                .FirstOrDefaultAsync();

            if (fileReader == null)
                return NotFound();

            // 判断是否过期
            if (fileReader.ExpireDate < DateTime.UtcNow)
            {
                fileReader.IsDeleted = true;
                await db.SaveChangesAsync();

                return NotFound();
            }

            // 获取文件对象
            string fullPath = fileStoreService.GetFileFullPath(fileReader.FileObject);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var result = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = fileReader.FileName
            };
            return result;
        }
    }
}
