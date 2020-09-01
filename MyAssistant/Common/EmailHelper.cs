using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyAssistant.Common
{
    public class EmailHelper
    {
        private string accout; //邮箱账户
        private string pass;//邮箱密码
        private string popServer; //pop服务地址
        private int popPort; //pop服务端口号（110）
        private bool isUseSSL;
        private string ServerDataDB;

        public EmailHelper(string _accout, string _pass, string _popServer, int _popPort, bool _isUseSSL, string _ServerDataDB)
        {
            this.accout = _accout;
            this.pass = _pass;
            this.popServer = _popServer;
            this.popPort = _popPort;
            this.isUseSSL = _isUseSSL;
            this.ServerDataDB = _ServerDataDB;
        }

        #region 验证邮箱是否登录成功
        public bool ValidateAccount(ref string error)
        {
            Pop3Client client = new Pop3Client();
            try
            {
                client.Connect(popServer, popPort, isUseSSL);
                client.Authenticate(accout, pass);
            }
            catch (InvalidLoginException ex)
            {
                error = "邮箱登录失败！";
                Console.WriteLine("0.1邮箱登录失败");
                return false;
            }
            catch (InvalidUseException ex)
            {
                error = "邮箱登录失败！";
                Console.WriteLine("0.2邮箱登录失败");
                return false;
            }
            catch (PopServerNotFoundException ex)
            {
                error = "服务器没有找到！";
                Console.WriteLine("0.3服务器没有找到");
                return false;
            }
            catch (PopServerException ex)
            {
                error = "请在邮箱开通POP3/SMTP！";
                Console.WriteLine("0.4请在邮箱开通POP3/SMTP！");
                return false;
            }
            catch (Exception ex)
            {
                error = "连接出现异常";
                Console.WriteLine("0.5连接出现异常");
                return false;
            }
            finally
            {
                client.Disconnect();
            }
            return true;
        }
        #endregion

        #region
        /// <summary>
        /// 获取邮件数量
        /// </summary>
        /// <returns></returns>
        public int GetEmailCount()
        {
            int messageCount = 0;
            using (Pop3Client client = new Pop3Client())
            {
                if (client.Connected)
                {
                    client.Disconnect();
                }
                client.Connect(popServer, popPort, isUseSSL);
                client.Authenticate(accout, pass, AuthenticationMethod.UsernameAndPassword);
                messageCount = client.GetMessageCount();
            }

            return messageCount;
        }
        #endregion

        #region 下载邮件附件
        /// <summary>
        /// 下载邮件附件
        /// </summary>
        /// <param name="path">下载路径</param>
        /// <param name="messageId">邮件编号</param>
        public bool DownAttachmentsById(string path, int messageId)
        {
            using (Pop3Client client = new Pop3Client())
            {
                try
                {
                    if (client.Connected)
                    {
                        client.Disconnect();
                    }
                    client.Connect(popServer, popPort, isUseSSL);
                    client.Authenticate(accout, pass);
                    Message message = client.GetMessage(messageId);
                    string senders = message.Headers.From.DisplayName;
                    string from = message.Headers.From.Address;
                    string subject = message.Headers.Subject;
                    DateTime Datesent = message.Headers.DateSent;


                    List<MessagePart> messageParts = message.FindAllAttachments();

                    if (messageParts.Count == 0) return false;

                    foreach (var item in messageParts)
                    {
                        if (item.IsAttachment)
                        {
                            if (item.FileName.Contains(".zip"))
                            {
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                var filePath = System.IO.Path.Combine(path, item.FileName);

                                File.WriteAllBytes(filePath, item.Body);

                                //解压到同名目录下

                                UnPackage.Unzip(filePath, path);

                                break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取附件出错：" + ex.Message);
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
