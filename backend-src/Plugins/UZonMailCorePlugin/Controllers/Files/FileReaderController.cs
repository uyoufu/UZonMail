using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Files;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Files;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Core.Controllers.Files
{
    public class FileReaderController(SqlContext db, FileStoreService fileStoreService) : ControllerBaseV1
    {
        /// <summary>
        /// 获取文件对象的下载 Id
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<long>> GetObjectAsync(long fileUsageId)
        {
            FileUsage? fileUsage = await db.FileUsages.FirstOrDefaultAsync(x => x.Id == fileUsageId);

            // 判断文件是否存在
            if (fileUsage == null) return 0L.ToFailResponse("文件不存在");

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
            var fileReader = await db.FileReaders.Where(x => x.Id == fileReaderId)
                .Include(x => x.FileObject)
                .ThenInclude(x => x.FileBucket)
                .FirstOrDefaultAsync();

            if (fileReader == null) return NotFound();

            // 判断是否过期
            if (fileReader.ExpireDate < DateTime.Now)
            {
                db.FileReaders.Remove(fileReader);
                await db.SaveChangesAsync();

                return NotFound();
            }

            // 获取文件对象
            string fullPath = fileStoreService.GetFileFullPath(fileReader.FileObject);
            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var result = new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = fileReader.FileName
            };
            return result;
        }
    }
}
