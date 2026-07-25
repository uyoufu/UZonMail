using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Controllers.Files.DTOs;
using UzonMail.CorePlugin.Services.Files;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.CorePlugin.Controllers.Files
{
    /// <summary>
    /// 用户文件分类树管理接口。
    /// </summary>
    public sealed class FileCategoryController(
        FileCategoryService categoryService,
        TokenService tokenService
    ) : ControllerBaseV1
    {
        [HttpGet("all")]
        public async Task<ResponseResult<List<FileCategoryDto>>> GetAll(
            CancellationToken cancellationToken
        )
        {
            var categories = await categoryService.GetAllAsync(
                tokenService.GetUserSqlId(),
                cancellationToken
            );
            return categories.ConvertAll(FileCategoryDto.FromEntity).ToSuccessResponse();
        }

        [HttpPost]
        public async Task<ResponseResult<FileCategoryDto>> Create(
            [FromBody] CreateFileCategoryDto request,
            CancellationToken cancellationToken
        )
        {
            var category = await categoryService.CreateAsync(
                tokenService.GetUserSqlId(),
                request.Name,
                request.ParentId,
                cancellationToken
            );
            return FileCategoryDto.FromEntity(category).ToSuccessResponse();
        }

        [HttpPut("{categoryId:long}")]
        public async Task<ResponseResult<FileCategoryDto>> Rename(
            long categoryId,
            [FromBody] RenameFileCategoryDto request,
            CancellationToken cancellationToken
        )
        {
            var category = await categoryService.RenameAsync(
                tokenService.GetUserSqlId(),
                categoryId,
                request.Name,
                cancellationToken
            );
            return FileCategoryDto.FromEntity(category).ToSuccessResponse();
        }

        [HttpPut("{categoryId:long}/position")]
        public async Task<ResponseResult<bool>> Move(
            long categoryId,
            [FromBody] MoveFileCategoryDto request,
            CancellationToken cancellationToken
        )
        {
            await categoryService.MoveAsync(
                tokenService.GetUserSqlId(),
                categoryId,
                request.TargetCategoryId,
                request.Placement,
                cancellationToken
            );
            return true.ToSuccessResponse();
        }

        [HttpDelete("{categoryId:long}")]
        public async Task<ResponseResult<bool>> Delete(
            long categoryId,
            CancellationToken cancellationToken
        )
        {
            await categoryService.DeleteAsync(
                tokenService.GetUserSqlId(),
                categoryId,
                cancellationToken
            );
            return true.ToSuccessResponse();
        }
    }
}
