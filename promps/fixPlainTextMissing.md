# 修复发件后检测 MPART_ALT_DIFF 异常问题

## 现象描述

当发送邮件后，被接收方提示 MPART_ALT_DIFF 和 HTML_IMAGE_ONLY_28 异常。

发件的内容为：

``` html
<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <meta name="x-apple-disable-message-reformatting">
  <meta http-equiv="X-UA-Compatible" content="IE=edge">
  <title>达人商务合作</title>
</head>

<body style="margin:0; padding:0; background-color:#f5f6f8; font-family:Arial, 'Microsoft YaHei', 'PingFang SC', sans-serif; color:#333333;">

  <table role="presentation"
         width="100%"
         border="0"
         cellspacing="0"
         cellpadding="0"
         style="width:100%; border-collapse:collapse; background-color:#f5f6f8;">
    <tr>
      <td align="center" style="padding:30px 12px;">

        <table role="presentation"
               width="600"
               border="0"
               cellspacing="0"
               cellpadding="0"
               style="width:100%; max-width:600px; border-collapse:collapse; background-color:#ffffff; border:1px solid #e5e7eb;">

          <tr>
            <td align="center"
                style="padding:26px 30px; border-bottom:1px solid #eeeeee; font-size:24px; line-height:34px; font-weight:bold; color:#222222;">
              达人商务合作
            </td>
          </tr>

          <tr>
            <td style="padding:30px; font-size:16px; line-height:28px; color:#333333;">

              <p style="margin:0 0 22px 0;">
                致：{{email}}，您好！很抱歉打扰你了。
              </p>

              <p style="margin:0 0 22px 0;">
                因本公司社群发展业务需要，现招各平台达人兼职发贴，合作带货！
              </p>

              <p style="margin:0 0 22px 0;">
                各类目多种类爆款商品无条件提供样品包邮到你手，达人无需退回。
              </p>

              <p style="margin:0;">
                请添加下方客服微信，填表统计名额后统一安排。
              </p>

            </td>
          </tr>

          <!-- 底部自适应图片 -->
          <tr>
            <td align="center"
                style="padding:0; margin:0; font-size:0; line-height:0;">
              <img
                src="https://youjian-1258728248.cos.ap-guangzhou.myqcloud.com/ga.jpg"
                width="600"
                alt=""
                border="0"
                style="display:block; width:100%; max-width:600px; height:auto; margin:0; padding:0; border:0; outline:none; text-decoration:none;"
              >
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>

</body>
</html>
```

## 修复需求

1. 修复上述 MPART_ALT_DIFF 异常问题

## 重构需求

1. D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Services\SendCore\WaitList\UsableTemplateList.cs 中，分析模板缓存逻辑，是否存在可以优化的地方，模板使用完成后，有没有及时释放，模板是否可以增量缓存

## 相关文件

1. D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Services\SendCore\ResponsibilityChains\LocalEmailSendingHandler.cs