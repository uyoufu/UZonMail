using Newtonsoft.Json.Linq;
using Server.Database;
using Server.Database.Models;
using Server.Protocol;
using Server.Websocket.Temp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Http.Modules.SendEmail
{
    /// <summary>
    /// 发送线程
    /// 一个发件箱对应一个发件线程，在该线程中控制发件的总量
    /// 如果发件连续失败，退出该线程
    /// </summary>
    class SendThread
    {
        #region 设置
        // 失败多少次后退出
        // 默认 3 次
        public int ExitofFailureCount { get; set; } = 3;
        #endregion

        #region 私有属性
        private string _userId;
        private SendBox _sendBox;
        private LiteDBManager _liteDb;
        private Setting _setting;

        private CancellationTokenSource _cancellationTokenSource;

        // 失败的次数
        private int _failureCount;
        // 最大发件量

        #endregion

        #region 事件
        public event Action<SendResult> SendCompleted;
        #endregion
        public SendThread(string userId, SendBox sendBox, LiteDBManager liteDb)
        {
            _userId = userId;
            _sendBox = sendBox;
            _liteDb = liteDb;

            // 获取设置
            // 获取设置
            _setting = _liteDb.SingleOrDefault<Setting>(s => s.userId == userId);

            _cancellationTokenSource = new CancellationTokenSource();
        }


        /// <summary>
        /// 开始发件
        /// </summary>
        /// <param name="sendItems">不同进程共享的栈</param>
        /// <param name="sendItemsList">原始发件数据，只有成功后才移除，当数量为 0 时，所有的进程退出</param>
        /// <returns></returns>
        public Task Run(Stack<SendItem> sendItems, List<SendItem> sendItemsList)
        {
            // 创建线程
            Task task = Task.Run(async () =>
            {
                while (sendItemsList.Count > 0)
                {
                    // 开始并行发送
                    //确定smtp服务器地址 实例化一个Smtp客户端
                    SmtpClient smtpclient = new SmtpClient();
                    smtpclient.Host = _sendBox.smtp;
                    //smtpClient.Port = "";//qq邮箱可以不用端口
                    //邮件发送方式  通过网络发送到smtp服务器
                    smtpclient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //如果服务器支持安全连接，则将安全连接设为true
                    smtpclient.EnableSsl = true;

                    // 获取发件箱
                    var sendItem = sendItems.Pop();
                    var mailMessage = await GenerateMailMessage(sendItem);
                    try
                    {
                        //是否使用默认凭据，若为false，则使用自定义的证书，就是下面的networkCredential实例对象
                        smtpclient.UseDefaultCredentials = false;
                        //指定邮箱账号和密码,需要注意的是，这个密码是你在QQ邮箱设置里开启服务的时候给你的那个授权码
                        NetworkCredential networkCredential = new NetworkCredential(_sendBox.email, _sendBox.password);
                        smtpclient.Credentials = networkCredential;

                        //发送邮件
                        smtpclient.Send(mailMessage);

                        // 发送成功后，马上从原始数据中移除
                        var index = sendItemsList.FindIndex(item => item._id == sendItem._id);
                        if (index > -1) sendItemsList.RemoveAt(index);

                        // 发送成功后，更新数据，更新到数据库
                        sendItem.senderEmail = _sendBox.email;
                        sendItem.senderName = _sendBox.userName;
                        sendItem.isSent = true;
                        sendItem.sendMessage = "mail_sent_successfully";
                        sendItem.sendDate = DateTime.Now;
                        _liteDb.Upsert(sendItem);

                        // 更新到当前进度中
                        SendCompleted?.Invoke(new SendResult()
                        {
                            SendBox = _sendBox,
                            SendItem = sendItem,
                            IsSent = true,
                        });

                        // 保存成功的发件量
                        if (!_sendBox.IncreaseSentCount(_liteDb, _setting))
                        {
                            // 直接退出
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        // 超过最大尝试次数就退出
                        if (sendItem.tryCount > 5)
                        {
                            // 此时也要更新进度
                            SendCompleted?.Invoke(new SendResult()
                            {
                                SendBox = _sendBox,
                                SendItem = sendItem,
                                IsSent = false
                            });
                        }
                        else if (_setting.isAutoResend) // 重新发送时，才重新推入栈中
                        {
                            // 更新状态                      
                            sendItem.tryCount++;
                            sendItem.isSent = false;

                            // 重新推入栈中
                            sendItems.Push(sendItem);
                        }

                        // 添加失败原因
                        if (ex.InnerException == null) sendItem.sendMessage = ex.Message;
                        else sendItem.sendMessage = ex.InnerException.Message;
                        _liteDb.Upsert(sendItem);

                        // 标记当前邮件失败次数
                        // 要等数据处理保存后，再更新
                        IncreaseFailure();
                    }
                    finally
                    {
                        // 每次发送完成，要等待一会儿再发送
                        double sleep = new Random().NextDouble() * 3 + 2;
                        if (_setting != null)
                        {
                            sleep = _setting.sendInterval_min + new Random().NextDouble() * (_setting.sendInterval_max - _setting.sendInterval_min);
                        }

                        // 线程暂停
                        Thread.Sleep((int)(sleep * 1000));
                    }
                }
            }, _cancellationTokenSource.Token);

            return task;
        }

        /// <summary>
        /// 将发送内容转成图片
        /// </summary>
        /// <param name="sendItem"></param>
        private async Task<bool> ConvertToImage(Setting setting, SendItem sendItem)
        {
            if (sendItem.sendItemType == SendItemType.dataUrl && setting.sendWithImageAndHtml && string.IsNullOrEmpty(sendItem.dataUrl))
            {
                // 从前端转存图片
                ReceivedMessage message = await SendCallback.Insance.SendAsync(_userId, new Protocol.Response()
                {
                    eventName = "sendingInfo",
                    command = "html2image",
                    result = sendItem,
                });
                if (message == null) return false;

                // 将结果添加到sendItem中
                var dataUrl = message.JObject.SelectToken("result");
                if (dataUrl == null) return false;

                dataUrl = dataUrl.Value<string>();

                sendItem.dataUrl = $"<img src=\"{dataUrl}\">";

                // 更新保存的数据
                _liteDb.Update(sendItem);
            }

            return true;
        }

        private void AddCustomHeaders(MailMessage mail)
        {
            ///以下四个自定义Header是将邮件伪装成OutLook发送的。
            ///目的是为了防止一些网站的反垃圾邮件功能
            ///将本系统发送的邮件当做垃圾邮件过滤掉。

            mail.Headers.Add("X-Priority", "3");

            mail.Headers.Add("X-MSMail-Priority", "Normal");

            mail.Headers.Add("X-Mailer", "Microsoft Outlook Express 6.00.2900.2869");

            mail.Headers.Add("X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2900.2869");

        }

        // 添加附件
        private void AddAttachmenets(MailMessage mail, SendItem sendItem)
        {
            if (sendItem.attachments == null || sendItem.attachments.Count < 1) return;

            for (int i = 0; i < sendItem.attachments.Count; i++)
            {
                string pathFileName = sendItem.attachments[i].fullName.Replace('/', '\\');
                var fileInfo = new FileInfo(pathFileName);

                if (!fileInfo.Exists)
                {
                    sendItem.attachments[i].isSent = false;
                    sendItem.attachments[i].reason = "文件不存在";
                    continue;
                }

                var attachment = new Attachment(pathFileName);
                //设置附件的MIME信息
                ContentDisposition cd = attachment.ContentDisposition;
                cd.CreationDate = fileInfo.CreationTime;//设置附件的创建时间
                cd.ModificationDate = fileInfo.LastWriteTime;//设置附件的修改时间
                cd.ReadDate = fileInfo.LastAccessTime;//设置附件的访问时间
                mail.Attachments.Add(attachment);//将附件添加到mailmessage对象

                sendItem.attachments[i].isSent = true;
            }
        }

        // 添加抄送
        private void AddCopyToEmails(MailMessage mail, SendItem sendItem)
        {
            foreach (var email in sendItem.copyToEmails)
            {
                mail.CC.Add(email);
            }
        }

        // 增加失败次数
        private void IncreaseFailure()
        {
            _failureCount++;
            if (_failureCount > ExitofFailureCount)
            {
                // 取消 task
                _cancellationTokenSource.Cancel();
            }
        }

        // 生成发送消息的消息体
        private async Task<MailMessage> GenerateMailMessage(SendItem sendItem)
        {
            //确定发件地址与收件地址
            MailAddress sendAddress = new MailAddress(_sendBox.email);

            // 判断是否需要转成图片
            await ConvertToImage(_setting, sendItem);

            MailAddress receiveAddress = new MailAddress(sendItem.receiverEmail);

            //构造一个Email的Message对象 内容信息
            MailMessage mailMessage = new MailMessage(sendAddress, receiveAddress)
            {
                Subject = sendItem.subject,
                SubjectEncoding = Encoding.UTF8
            };

            // 设置发件主体                        
            if (sendItem.sendItemType == SendItemType.dataUrl && !string.IsNullOrEmpty(sendItem.dataUrl)) mailMessage.Body = sendItem.dataUrl;
            else mailMessage.Body = sendItem.html;

            // 发件编码
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;

            // 伪装成 outlook 发送
            AddCustomHeaders(mailMessage);

            // 添加附件
            AddAttachmenets(mailMessage, sendItem);

            // 添加抄送
            AddCopyToEmails(mailMessage, sendItem);


            return mailMessage;
        }
    }
}
