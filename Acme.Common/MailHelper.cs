using System;
using System.Linq;
using System.Net.Mail;

namespace Acme.Common
{
    /// <summary>
    /// 发送邮件类
    /// https://www.cnblogs.com/love201314/p/9182622.html
    /// </summary>
    public class SmtpEmailHelper
    {
        #region Properties

        /// <summary>
        /// 发送邮箱全地址
        /// </summary>
        public string SmtpUserName { get; set; }

        /// <summary>
        /// 发送邮箱显示名
        /// </summary>
        public string SmtpDisplayName { get; set; }

        /// <summary>
        /// 发送邮箱密码
        /// </summary>
        public string SmtpPassword { get; set; }

        /// <summary>
        /// 邮箱服务器地址
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// 邮箱服务器端口
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// 是否ssl加密
        /// </summary>
        public bool SmtpEnableSsl { get; set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public SmtpEmailHelper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName">发送邮箱全地址</param>
        /// <param name="passWord">发送邮箱密码</param>
        /// <param name="displayName">发送邮箱显示名</param>
        /// <param name="server">邮箱服务器地址</param>
        /// <param name="port">邮箱服务器端口</param>
        /// <param name="enableSsl">是否ssl加密</param>
        public SmtpEmailHelper(string userName, string passWord, string displayName, string server, int port, bool enableSsl = true) : this()
        {
            SmtpUserName = userName;
            SmtpPassword = passWord;
            SmtpDisplayName = displayName;
            SmtpServer = server;
            SmtpPort = port;
            SmtpEnableSsl = enableSsl;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">内容消息正文</param>
        /// <param name="mailTo">收件人</param>
        /// <returns></returns>
        public bool Send(string subject, string body, string mailTo)
        {
            var arrMailTo = new string[] { mailTo };
            string[] attachments = null;
            const MailPriority priority = MailPriority.Normal;
            return Send(subject, body, arrMailTo, null, attachments, null, System.Text.Encoding.UTF8, priority, true);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">内容消息正文</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        public bool Send(string subject, string body, string mailTo, string[] attachments)
        {
            var arrMailTo = new string[] { mailTo };
            var priority = MailPriority.Normal;
            return Send(subject, body, arrMailTo, null, attachments, null, System.Text.Encoding.UTF8, priority, true);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">内容消息正文</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="mailCC">抄送人</param>
        /// <param name="attachments">附件</param>
        /// <returns></returns>
        public bool Send(string subject, string body, string mailTo, string[] mailCC, string[] attachments)
        {
            var arrMailTo = new string[] { mailTo };
            var priority = MailPriority.Normal;
            return Send(subject, body, arrMailTo, mailCC, attachments, null, System.Text.Encoding.UTF8, priority, true);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">内容消息正文</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="attachments">附件</param>
        /// <param name="priority">邮件优先级</param>
        /// <returns></returns>
        public bool Send(string subject, string body, string[] mailTo, string[] attachments, MailPriority priority = MailPriority.Normal)
        {
            return Send(subject, body, mailTo, null, attachments, null, System.Text.Encoding.UTF8, priority, true);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">内容消息正文</param>
        /// <param name="mailTo">收件人</param>
        /// <param name="mailCC">抄送人</param>
        /// <param name="attachments">附件</param>
        /// <param name="bccs">密件抄送地址</param>
        /// <param name="bodyEncoding">编码</param>
        /// <param name="priority">邮件优先级</param>
        /// <param name="IsBodyHtml">是否是HTML邮件</param>
        /// <returns></returns>
        public bool Send(string subject, string body, string[] mailTo, string[] mailCC, string[] attachments, string[] bccs, System.Text.Encoding bodyEncoding, MailPriority priority = MailPriority.Normal, bool IsBodyHtml = true)
        {
            //创建Email实体
            var mailMessage = new MailMessage
            {
                From = new MailAddress(SmtpUserName, SmtpDisplayName, bodyEncoding),
                Subject = subject,
                SubjectEncoding = bodyEncoding,//邮件标题编码
                Body = body,
                BodyEncoding = bodyEncoding,//邮件内容编码
                IsBodyHtml = true,//是否是HTML邮件
                Priority = priority//邮件优先级
            };

            //插入附件
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(attachment) && System.IO.File.Exists(attachment))
                    {
                        //建立邮件附件类的一个对象，语法格式为System.Net.Mail.Attachment(文件名，文件格式)  
                        var attFile = new Attachment(attachment, System.Net.Mime.MediaTypeNames.Application.Octet)
                        {
                            Name = System.IO.Path.GetFileName(attachment),
                            NameEncoding = bodyEncoding
                        };
                        // MIME协议下的一个对象，用以设置附件的创建时间，修改时间以及读取时间
                        var disposition = attFile.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(attachment);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(attachment);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(attachment);
                        mailMessage.Attachments.Add(attFile);
                    }
                }
            }

            //插入收件人地址，抄送地址和密件抄送地址
            if (null != mailTo)
            {
                foreach (var to in mailTo.Where(m => !string.IsNullOrWhiteSpace(m)))
                {
                    mailMessage.To.Add(new MailAddress(to));
                }
            }
            if (null != mailCC)
            {
                foreach (var cc in mailCC.Where(m => !string.IsNullOrWhiteSpace(m)))
                {
                    mailMessage.CC.Add(new MailAddress(cc));
                }
            }
            if (null != bccs)
            {
                foreach (var bcc in bccs.Where(m => !string.IsNullOrWhiteSpace(m)))
                {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }
            }

            //创建Smtp客户端
            var client = new SmtpClient
            {
                Credentials = new System.Net.NetworkCredential(SmtpUserName, SmtpPassword),
                //上述写你的邮箱和密码
                Port = SmtpPort, //使用的端口
                Host = SmtpServer,
                EnableSsl = SmtpEnableSsl //经过ssl加密.
            };

            object userState = mailMessage;
            try
            {
                //发送邮件
                client.Send(mailMessage);
                //client.SendAsync(mailMessage);

                mailMessage.Dispose();
                client.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                mailMessage.Dispose();
                client.Dispose();
                return false;
            }
        }
    }
}
