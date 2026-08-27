using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.Utils.Results;

namespace UzonMail.CorePlugin.Database.Validators
{
    public class SendingGroupValidator : AbstractValidator<SendingGroup>
    {
        public SendingGroupValidator() { }

        /// <summary>
        /// 手动验证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ValidationResult Validate(ValidationContext<SendingGroup> context)
        {
            var result = new ValidationResult();
            var vdResult = ValidateCore(context.InstanceToValidate);
            if (vdResult)
                return result;

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
                if (!globalVdResult)
                    return globalVdResult;
            }
            else
            {
                // 有 excel 数据的情况
                ExcelDataInfo excelDataInfo = new(sendingGroup.Data);
                // 合并界面所选联系人后，仍需保留 Excel 行级完整性校验。
                if (sendingGroup.Recipients.Count > 0)
                {
                    int dataCount = excelDataInfo.RecipientEmails.Count;
                    sendingGroup.Recipients.ForEach(x =>
                        excelDataInfo.RecipientEmails.Add(x.Email)
                    );
                    int allCount = excelDataInfo.RecipientEmails.Count;
                    if (dataCount < allCount)
                    {
                        // 验证通用数据
                        var globalVdResult = ValidateGlobalData(sendingGroup);
                        if (!globalVdResult)
                            return globalVdResult;
                    }
                }

                // 验证其它数据
                if (
                    sendingGroup.SenderAccounts.Count == 0
                    && excelDataInfo.SenderAccountStatus != ExcelDataStatus.All
                    && sendingGroup.SenderAccountGroups?.Count == 0
                )
                {
                    return new ErrorResult<bool>("缺失发件账号，请在数据中指定发件账号或选择发件账号");
                }
                if (excelDataInfo.RecipientValidationStatus != ExcelDataStatus.All)
                {
                    var missingRecipientContactRows = GetMissingRecipientContactRowNumbers(
                        sendingGroup.Data
                    );
                    var rowNumbers = string.Join("、", missingRecipientContactRows);
                    return new ErrorResult<bool>($"Excel 数据第 {rowNumbers} 行缺少 recipientEmail");
                }
                if (
                    !ExistGlobalBody(sendingGroup)
                    && excelDataInfo.BodyStatus != ExcelDataStatus.All
                )
                {
                    return new ErrorResult<bool>("缺失邮件正文，请在数据中指定邮件正文 或 选择模板 或 填写正文");
                }
            }

            return new SuccessResult<bool>(true);
        }

        /// <summary>
        /// 返回缺少收件人邮箱的 Excel 数据行号。
        /// </summary>
        private static List<int> GetMissingRecipientContactRowNumbers(JArray excelData)
        {
            return excelData
                .Select((row, index) => new { Row = row, Number = index + 1 })
                .Where(x =>
                    x.Row is not JObject excelRow
                    || string.IsNullOrWhiteSpace(excelRow.GetValue("recipientEmail")?.ToString())
                )
                .Select(x => x.Number)
                .ToList();
        }

        private Result<bool> ValidateGlobalData(SendingGroup sendingGroup)
        {
            if (!ExistGlobalBody(sendingGroup))
            {
                return new ErrorResult<bool>("缺失邮件正文，请选择模板 或 填写正文");
            }

            if (
                sendingGroup.SenderAccounts.Count == 0
                && (
                    sendingGroup.SenderAccountGroups == null
                    || sendingGroup.SenderAccountGroups.Count == 0
                )
            )
            {
                return new ErrorResult<bool>("请选择发件人");
            }

            if (
                sendingGroup.Recipients.Count == 0
                && (
                    sendingGroup.RecipientContactGroups == null
                    || sendingGroup.RecipientContactGroups.Count == 0
                )
            )
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
