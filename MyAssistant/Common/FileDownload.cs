using Acme.Common;
using MyAssistant.Common;
using ServiceStack;
using ServiceStack.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Acme.ServiceInterface.Download
{
    public static class FileDownload
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static object Download(string file, string folder)
        {
            var response = new ReportTable
            {
                ErrCode = 0,
                ErrMsg = "ok"
            };
            try
            {
                var appSettings = new AppSettings();
                var contentRootPath = appSettings.Get("HostingEnvironment.ContentRootPath", HostContext.AppHost.MapProjectPath("~/"));
                var folderPath = HostContext.AppHost.MapProjectPath($"{ Path.Combine(contentRootPath, folder)}");

                var filePath = Path.Combine(folderPath, file);

                if (!System.IO.File.Exists(filePath))
                {
                    response.ErrCode = -1;
                    response.ErrMsg = $"文件[{filePath}]不存在";

                    return response;
                }
                var reportBytes = System.IO.File.ReadAllBytes(filePath);

                var extension = Path.GetExtension(filePath);
                var mimeType = MimeMapping.GetMimeType(extension.ToLower());
                var result = new HttpResult(reportBytes, mimeType);

                string fileName = HttpUtility.UrlEncode(Path.GetFileName(filePath));

                if (file.StartsWith("pc"))
                {
                    if (file.Contains("_"))
                    {
                        var segs = file.Split("_");
                        fileName = segs[1] + extension;
                    }
                }

                result.Headers.Add("Content-Disposition", $"inline;filename={fileName};");
                result.StatusCode = HttpStatusCode.OK;

                return result;
            }
            catch (Exception ex)
            {
                response.ErrCode = -1;
                response.ErrMsg = ex.Message;

                return response;
            }
        }
    }
}
