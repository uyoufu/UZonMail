﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Database;
using Server.Database.Models;
using Server.Http.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Http.Modules.SendEmail
{
    public class SendTask
    {
        public static SendTask Instance { get; private set; }

        // 新建发送任务
        public static bool CreateSendTask(string userId, string subject, JArray receivers, JArray data, string templateId, LiteDBManager liteDb, out string message)
        {
            // 判断原来任务的状态
            if (Instance != null && Instance.SendStatus >= SendStatus.Init)
            {
                message = "任务正在进行中";
                return false;
            }

            SendTask temp = new SendTask(userId, subject, receivers, data, templateId, liteDb);
            // 判断是否符合数据要求
            if (!temp.Validate(out string validateMsg))
            {
                message = validateMsg;
                return false;
            }

            Instance = temp;
            message = "success";
            return true;
        }

        public static bool CreateSendTask(string userId, LiteDBManager liteDb, out string message)
        {
            // 判断原来任务的状态
            if (Instance != null && Instance.SendStatus >= SendStatus.Init)
            {
                message = "任务正在进行中";
                return false;
            }

            SendTask temp = new SendTask(userId, liteDb);

            Instance = temp;
            message = "success";
            return true;
        }

        private string _userId;
        private string _subject;
        private JArray _receivers;
        private JArray _data;
        private Template _template;
        private LiteDBManager _liteDb;
        private List<SendItem> _sendItems;
        private List<ReceiveBox> _receiveBoxes;

        public SendStatus SendStatus { get; private set; }

        // 从发件信息中获取基本信息
        private SendTask(string userId, string subject, JArray receivers, JArray data, string templateId, LiteDBManager liteDb)
        {
            _userId = userId;
            _subject = subject;
            _receivers = receivers;
            _data = data;
            _liteDb = liteDb;

            // 获取模板
            _template = _liteDb.SingleOrDefault<Template>(t => t._id == templateId);

            // 生成发件模板，用异步，否则会阻塞该线程
            new Task(() =>
            {
                // 生成每一项
                _sendItems = new List<SendItem>();
                _receiveBoxes = new List<ReceiveBox>();
                // 获取当前收件人或组下的所有人
                foreach (JToken jt in _receivers)
                {
                    // 判断 type
                    string type = jt.Value<string>("type");
                    string id = jt.Value<string>("_id");
                    if (type == "group")
                    {
                        // 找到group下所有的用户
                        List<ReceiveBox> boxes = liteDb.Fetch<ReceiveBox>(r => r.groupId == id);

                        // 如果没有，才添加
                        foreach (ReceiveBox box in boxes)
                        {
                            if (_receiveBoxes.Find(item => item._id == box._id) == null) _receiveBoxes.Add(box);
                        }
                    }
                    else
                    {
                        // 选择了单个用户
                        var box = liteDb.SingleOrDefault<ReceiveBox>(r => r._id == id);
                        if (box != null && _receiveBoxes.Find(item => item._id == box._id) == null) _receiveBoxes.Add(box);
                    }
                }

                // 开始添加                
                foreach (var re in _receiveBoxes)
                {
                    // 判断有没有数据
                    var itemData = _data.FirstOrDefault(jt => jt.Value<string>("userName") == re.userName);
                    if (itemData == null) continue;

                    var item = new SendItem()
                    {
                        receiverName = re.userName,
                        receiverEmail = re.email,
                    };

                    // 获取数据
                    List<string> keys = (itemData as JObject).Properties().ToList().ConvertAll(p => p.Name);
                    string sendHtml = _template.html;
                    // 判断是否有自定义内容，然后判断是否有自定义模板
                    if (keys.Contains("body"))
                    {
                        // 获取 body 值
                        string body = itemData.Value<string>("body");
                        if (!string.IsNullOrEmpty(body))
                        {
                            sendHtml = body;
                        }
                    }
                    else if (keys.Contains("template"))
                    {
                        string customTemplateName = itemData.Value<string>("template");
                        if (!string.IsNullOrEmpty(customTemplateName))
                        {
                            // 获取新模板，如果失败，则跳过，不发送
                            var customTemplate = _liteDb.SingleOrDefault<Template>(t => t.name == customTemplateName);
                            if (customTemplate != null)
                            {
                                sendHtml = customTemplate.html;
                            }
                        }
                    }

                    // 替换模板内数据
                    string subjectTemp = _subject;
                    foreach (string key in keys)
                    {
                        var regex = new Regex("{{\\s*" + key + "\\s*}}");
                        sendHtml = regex.Replace(sendHtml, itemData[key].Value<string>());

                        // 同时替换主题数据
                        subjectTemp = regex.Replace(subjectTemp, itemData[key].Value<string>());
                    }

                    item.html = sendHtml;
                    item.subject = subjectTemp;

                    // 添加到保存的集合中
                    _sendItems.Add(item);
                };

                // 添加序号
                for (int i = 0; i < _sendItems.Count; i++)
                {
                    _sendItems[i].index = i;
                    _sendItems[i].total = _sendItems.Count;
                }
            }).Start();
        }

        private SendTask(string userId, LiteDBManager liteDb)
        {
            _userId = userId;
            _liteDb = liteDb;
        }
        private bool Validate(out string message)
        {
            message = "success";
            return true;
        }

        private int _index;

        // 获取预览内容
        public SendItem GetPreviewHtml(string directive)
        {
            if (_sendItems.Count < 1) return null;

            switch (directive)
            {
                case "first":
                    _index = 0;
                    return _sendItems.FirstOrDefault();
                case "next":
                    _index++;
                    return _sendItems[CycleInt(_index, _sendItems.Count)];
                case "previous":
                    _index--;
                    return _sendItems[CycleInt(_index, _sendItems.Count)];
                default:
                    // 通过名字来搜索
                    return null;
            }
        }

        private int CycleInt(int index, int total)
        {
            int result = index % total;
            if (result >= 0) return result;
            return total + result;
        }

        private SendingInfo _sendingInfo;
        public SendingInfo SendingInfo
        {
            get
            {
                if (_sendingInfo == null) _sendingInfo = new SendingInfo();
                return _sendingInfo;
            }
            set
            {
                // 判断是否已经完成
                if (value.index == value.total)
                {
                    this.SendStatus = SendStatus.SendFinish;
                    // 对于已经完成的，要更新数据的状态
                    var history = _liteDb.SingleById<HistoryGroup>(_currentHistoryGroupId);
                    if (history != null)
                    {
                        // 更新状态
                        history.sendStatus = SendStatus.SendFinish;
                        _liteDb.Update(history);

                        SendStatus = SendStatus.SendFinish;
                    }
                }

                _sendingInfo = value;
            }
        }

        private CancellationToken _cancleToken;
        private string _currentHistoryGroupId;
        /// <summary>
        /// 利用预览产生的数据进行发送
        /// </summary>
        /// <returns></returns>
        public string StartSending()
        {
            // 添加正在发送的标记
            this.SendStatus = SendStatus.Sending;

            // 获取发件箱
            var senders = _liteDb.Database.GetCollection<SendBox>().FindAll().ToList();

            // 添加历史
            HistoryGroup historyGroup = new HistoryGroup()
            {
                userId = _userId,
                createDate = DateTime.Now,
                subject = _subject,
                data = JsonConvert.SerializeObject(_data),
                receiverIds = _receiveBoxes.ConvertAll(rec => rec._id),
                templateId = _template._id,
                templateName = _template.name,
                senderIds = senders.ConvertAll(s => s._id),
                sendStatus = SendStatus.Sending,
            };
            _liteDb.Database.GetCollection<HistoryGroup>().Insert(historyGroup);
            _currentHistoryGroupId = historyGroup._id;

            // 将所有的待发信息添加到数据库，然后读取出来批量发送
            _sendItems.ForEach(item => item.historyId = historyGroup._id);
            _liteDb.Database.GetCollection<SendItem>().InsertBulk(_sendItems);

            SendItems(_sendItems, historyGroup._id);

            return historyGroup._id;
        }

        /// <summary>
        /// 重新发送
        /// </summary>
        /// <param name="sendItemIds">传入需要重新发送的id</param>
        /// <returns></returns>
        public bool Resend(string historyId, List<string> sendItemIds)
        {
            // 判断是否结束
            if (SendStatus != SendStatus.SendFinish) return false;

            // 修改状态
            SendStatus = SendStatus.Resending;
            _currentHistoryGroupId = historyId;
            // 更改数据库中的状态
            var history = _liteDb.SingleById<HistoryGroup>(historyId);
            if (history == null) return false;
            history.sendStatus = SendStatus.Resending;
            _liteDb.Update(history);

            // 找到待发送的项
            var sendItems = _liteDb.Fetch<SendItem>(item => sendItemIds.Contains(item._id));

            // 判断重发数
            if (sendItemIds.Count < 1)
            {
                history.sendStatus = SendStatus.SendFinish;
                _liteDb.Update(history);

                // 获取重发完成的信息
                var sendingInfo = new SendingInfo()
                {
                    historyId = historyId,
                    index = 1,
                    total = 1,
                };
                SendingInfo = sendingInfo;

                return false;
            }

            // 开始发件
            SendItems(sendItems, historyId);

            return true;
        }

        private void SendItems(List<SendItem> sendItemList, string historyId)
        {
            var senders = _liteDb.Database.GetCollection<SendBox>().FindAll().ToList();

            // 添加到栈中
            Stack<SendItem> sendItems = new Stack<SendItem>();
            sendItemList.ForEach(item => sendItems.Push(item));

            // 初始化进度
            var sendingInfo0 = new SendingInfo()
            {
                historyId = historyId,
                index = 0,
                total = sendItemList.Count,
            };
            SendingInfo = sendingInfo0;

            _cancleToken = new CancellationToken();
            int sentIndex = 0;
            // 开始发送邮件，采用异步进行发送
            // 一个发件箱对应一个异步
            foreach (SendBox sb in senders)
            {
                Task.Run(() =>
                {
                    Setting setting = _liteDb.SingleOrDefault<Setting>(s => s.userId == _userId);
                    while (sendItems.Count > 0)
                    {
                        // 开始并行发送
                        //确定smtp服务器地址 实例化一个Smtp客户端
                        SmtpClient smtpclient = new SmtpClient();
                        smtpclient.Host = sb.smtp;
                        //smtpClient.Port = "";//qq邮箱可以不用端口

                        //确定发件地址与收件地址
                        MailAddress sendAddress = new MailAddress(sb.email);

                        // 获取发件箱
                        var sendItem = sendItems.Pop();
                        MailAddress receiveAddress = new MailAddress(sendItem.receiverEmail);

                        //构造一个Email的Message对象 内容信息
                        MailMessage mailMessage = new MailMessage(sendAddress, receiveAddress);
                        mailMessage.Subject = sendItem.subject;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.Body = sendItem.html;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.IsBodyHtml = true;

                        //邮件发送方式  通过网络发送到smtp服务器
                        smtpclient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        //如果服务器支持安全连接，则将安全连接设为true
                        smtpclient.EnableSsl = true;
                        try
                        {
                            //是否使用默认凭据，若为false，则使用自定义的证书，就是下面的networkCredential实例对象
                            smtpclient.UseDefaultCredentials = false;

                            //指定邮箱账号和密码,需要注意的是，这个密码是你在QQ邮箱设置里开启服务的时候给你的那个授权码
                            NetworkCredential networkCredential = new NetworkCredential(sb.email, sb.password);
                            smtpclient.Credentials = networkCredential;

                            //发送邮件
                            smtpclient.Send(mailMessage);
                            // 发送成功后，更新数据，更新到数据库
                            sendItem.senderEmail = sb.email;
                            sendItem.senderName = sb.userName;
                            sendItem.isSent = true;
                            sendItem.sendMessage = "邮件送达";
                            sendItem.sendDate = DateTime.Now;
                            _liteDb.Upsert(sendItem);

                            // 更新到当前进度中
                            var sendingInfo = new SendingInfo()
                            {
                                historyId = historyId,
                                index = ++sentIndex,
                                total = sendItemList.Count,
                                receiverEmail = sendItem.receiverEmail,
                                receiverName = sendItem.receiverName,
                                SenderEmail = sendItem.senderEmail,
                                SenderName = sendItem.senderName,
                            };
                            SendingInfo = sendingInfo;
                        }
                        catch (Exception ex)
                        {
                            // 超过最大尝试次数就退出
                            if (sendItem.tryCount > 5)
                            {
                                // 此时也要更新进度
                                var sendingInfo = new SendingInfo()
                                {
                                    historyId = historyId,
                                    index = ++sentIndex,
                                    total = sendItemList.Count,
                                    receiverEmail = sendItem.receiverEmail,
                                    receiverName = sendItem.receiverName,
                                    SenderEmail = sendItem.senderEmail,
                                    SenderName = sendItem.senderName,
                                };
                                SendingInfo = sendingInfo;
                            }
                            else if (setting.isAutoResend) // 重新发送时，才重新推入栈中
                            {
                                // 重新推入栈中
                                sendItems.Push(sendItem);
                            }

                            // 更新状态                      
                            sendItem.tryCount++;
                            sendItem.isSent = false;
                            sendItem.sendMessage = ex.InnerException.Message;
                            _liteDb.Upsert(sendItem);
                        }
                        finally
                        {
                            // 每次发送完成，要等待一会儿再发送
                            double sleep = new Random().NextDouble() * 3 + 2;
                            if (setting != null)
                            {
                                sleep = setting.sendInterval_min + new Random().NextDouble() * (setting.sendInterval_max - setting.sendInterval_min);
                            }

                            Thread.Sleep((int)(sleep * 1000));
                        }
                    }
                }, _cancleToken);
            }
        }
    }
}
