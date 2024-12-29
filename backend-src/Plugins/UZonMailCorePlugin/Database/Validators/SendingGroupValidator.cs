using FluentValidation;
using FluentValidation.Results;
using Org.BouncyCastle.Utilities;
using UZonMail.Utils.Results;
using UZonMail.DB.SQL.Emails;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.DB.SQL.Permission;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UZonMail.Core.Database.Validators
{
    public class SendingGroupValidator : AbstractValidator<SendingGroup>
    {
        public SendingGroupValidator()
        {
        }

        /// <summary>
        /// 手动验证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValidationResult Validate(ValidationContext<SendingGroup> context)
        {
            var result = new ValidationResult();
            var vdResult = ValidateCore(context.InstanceToValidate);
            if (vdResult) return result;

            result.Errors.Add(new ValidationFailure("", vdResult.Message));
            return result;
        }

        #region 数据验证
        /// <summary>
        /// 验证数据
        /// </summary>
        /// <param name="validateOption"></param>
        /// <returns></returns>
        private Result<bool> ValidateCore(SendingGroup sendingGroup)
        {
            // 1. 主题必须
            if (string.IsNullOrEmpty(sendingGroup.Subjects))
            {
                return new ErrorResult<bool>("主题不能为空");
            }

            // 没有 excel 数据的情况
            if (sendingGroup.Data == null || sendingGroup.Data.Count == 0)
            {
                // 2. 没有数据
                var globalVdResult = ValidateGlobalData(sendingGroup);
                if (!globalVdResult) return globalVdResult;
            }
            else
            {
                // 有 excel 数据的情况
                ExcelDataInfo excelDataInfo = new(sendingGroup.Data);
                // 若用户选择了收件箱，要判断发件箱是否在数据表格中出现
                if (sendingGroup.Inboxes.Count > 0)
                {
                    int dataCount = excelDataInfo.InboxSet.Count;
                    sendingGroup.Inboxes.ForEach(x => excelDataInfo.InboxSet.Add(x.Email));
                    int allCount = excelDataInfo.InboxSet.Count;
                    if (dataCount < allCount)
                    {
                        // 验证通用数据
                        var globalVdResult = ValidateGlobalData(sendingGroup);
                        if (!globalVdResult) return globalVdResult;
                    }
                }

                // 验证其它数据
                if (sendingGroup.Outboxes.Count == 0 && excelDataInfo.OutboxStatus != ExcelDataStatus.All)
                {
                    return new ErrorResult<bool>("缺失发件箱，请在数据中指定发件箱或选择发件箱");
                }
                if (excelDataInfo.InboxStatus != ExcelDataStatus.All)
                {
                    return new ErrorResult<bool>("请保证每条数据都有 inbox (收件人邮箱)");
                }
                if (!ExistGlobalBody(sendingGroup) && excelDataInfo.BodyStatus != ExcelDataStatus.All)
                {
                    return new ErrorResult<bool>("缺失邮件正文，请在数据中指定邮件正文 或 选择模板 或 填写正文");
                }
            }

            return new SuccessResult<bool>(true);
        }

        private Result<bool> ValidateGlobalData(SendingGroup sendingGroup)
        {
            if (!ExistGlobalBody(sendingGroup))
            {
                return new ErrorResult<bool>("缺失邮件正文，请选择模板 或 填写正文");
            }

            if (sendingGroup.Outboxes.Count == 0 && (sendingGroup.OutboxGroups == null || sendingGroup.OutboxGroups.Count == 0))
            {
                return new ErrorResult<bool>("请选择发件人");
            }

            if (sendingGroup.Inboxes.Count == 0 && (sendingGroup.InboxGroups == null || sendingGroup.InboxGroups.Count == 0))
            {
                return new ErrorResult<bool>("请选择收件人");
            }

            return new SuccessResult<bool>(true);
        }

        // 是否存在全局正文
        private bool ExistGlobalBody(SendingGroup sendingGroup)
        {
            return sendingGroup.Templates?.Count > 0 || !string.IsNullOrEmpty(sendingGroup.Body);
        }
        #endregion
    }
}
