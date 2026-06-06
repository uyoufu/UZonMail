using System;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace UZonMail.Utils.Web.Multipart
{
    /// <summary>
    /// Multipart 操作类
    /// 参考：https://www.cnblogs.com/liuxiaoji/p/10266609.html
    /// </summary>
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            //var boundary = Microsoft.Net.Http.Headers.HeaderUtilities.RemoveQuotes(contentType.Boundary);// .NET Core <2.0
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value; //.NET Core 2.0
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            //注意这里的boundary.Length指的是boundary=---------------------------99614912995中等号后面---------------------------99614912995字符串的长度，也就是section分隔符的长度，上面也说了这个长度一般不会超过70个字符是比较合理的
            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        /// <summary>
        /// 判断是否是 multipart 类型
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                    && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 如果 section 是表单键值对 section，那么本方法返回true
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="key";
            return contentDisposition != null
                    && contentDisposition.DispositionType.Equals("form-data")
                    && string.IsNullOrEmpty(contentDisposition.FileName.Value) // For .NET Core <2.0 remove ".Value"
                    && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value); // For .NET Core <2.0 remove ".Value"
        }

        /// <summary>
        /// 如果 section 是上传文件 section，那么本方法返回 true
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
            return contentDisposition != null
                    && contentDisposition.DispositionType.Equals("form-data")
                    && (!string.IsNullOrEmpty(contentDisposition.FileName.Value) // For .NET Core <2.0 remove ".Value"
                        || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value)); // For .NET Core <2.0 remove ".Value"
        }

        /// <summary>
        /// 如果一个section的Header是： Content-Disposition: form-data; name="files"; filename="Misc 002.jpg"
        /// 那么本方法返回： files
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static string GetFileContentInputName(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition.Name.Value;
        }

        /// <summary>
        /// 如果一个section的Header是： Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        /// 那么本方法返回： Misc 002.jpg
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static string GetFileName(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition.FileName.Value;
        }
    }
}
