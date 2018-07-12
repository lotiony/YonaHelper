using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Yona
{
    public class YonaHelper
    {
        #region public properties

        public string Owner { get; set; } = "";
        public string Project { get; set; } = "";
        public string YonaToken { get; set; } = "";

        #endregion public properties

        #region API Hosts

        public string YONA_HOST { get; set; } = "http://yona.smartomr.com:9000";
        public string NEW_ISSUE_HOST { get { return $"{YONA_HOST}/-_-api/v1/owners/{Owner}/projects/{Project}/issues"; } }
        public string NEW_POST_HOST { get { return $"{YONA_HOST}/-_-api/v1/owners/{Owner}/projects/{Project}/posts"; } }
        public string FILE_UPLOAD_HOST { get { return $"{YONA_HOST}/files"; } }

        #endregion API Hosts

        #region local variables

        private bool IsEnableApi
        { get { return (Owner != "" && Project != "" && YonaToken != ""); } }
        private static string Boundary = "";
        private static string CRLF = "\r\n";
        private static Stream DataStream = new MemoryStream();
        private static byte[] formData;

        #endregion local variables

        /// <summary>
        /// ..ctor
        /// </summary>
        public YonaHelper(string owner, string project, string token)
        {
            Owner = owner;
            Project = project;
            YonaToken = token;
        }

        public void SetHost(string host)
        {
            if (host.Last() == '/') host = host.Substring(0, host.Length - 1);
            this.YONA_HOST = host;
        }

        #region File Upload

        public Attachment FileUpload(string fileName)
        {
            if (File.Exists(fileName) && IsEnableApi)
            {
                Boundary = $"------WebKitBoundary{DateTime.Now.Ticks.ToString("x")}------";

                try
                {
                    DataStream = new MemoryStream();

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(FILE_UPLOAD_HOST);
                    request.Method = "POST";
                    request.KeepAlive = true;
                    request.ContentType = "multipart/form-data; boundary=" + Boundary;
                    request.Headers.Add("Yona-Token", YonaToken);
                    buildFileParam("filePath", fileName); // 파일 [0]
                    buildByteParam(); // Byte Array 생성

                    Stream stream = request.GetRequestStream();
                    stream.Write(formData, 0, formData.Length); // request 전송
                    stream.Close();
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string text = reader.ReadToEnd();
                    stream.Close();
                    response.Close();
                    reader.Close();

                    if (text.Length > 0)
                    {
                        Attachment attachment;
                        attachment = JsonConverter.DeserializeJsonUsingJavaScript<Attachment>(text);
                        attachment.createDate = DateTime.Now;

                        return attachment;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }

            return null;
        }

        private void buildParam(String name, String value)
        {
            string paramName1 = name;
            string paramValue1 = value;
            string res = Boundary + CRLF + "Content-Disposition: form-data; name=\"" + paramName1 + "\"" + CRLF;
            res += "Content-Type: text/plain; charset=UTF-8" + CRLF + CRLF;
            res += paramValue1 + CRLF + CRLF + Boundary + CRLF;
            DataStream.Write(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetByteCount(res));
        }

        private void buildFileParam(String fileParamName, String filePathName)
        {
            FileStream fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read);
            string contentType = GetContentType(filePathName);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, fileData.Length);
            fs.Close();
            string postData = Boundary + CRLF + "Content-Disposition: form-data; name=\"" + fileParamName + "\"; filename=\"";
            postData += Path.GetFileName(filePathName) + "\"" + CRLF + "Content-Type: " + contentType + CRLF + CRLF;
            DataStream.Write(Encoding.UTF8.GetBytes(postData), 0, Encoding.UTF8.GetByteCount(postData));
            DataStream.Write(fileData, 0, fileData.Length);
            DataStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
        }

        private void buildByteParam()
        {
            string footer = "--" + Boundary;
            DataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
            DataStream.Position = 0;
            formData = new byte[DataStream.Length];
            DataStream.Read(formData, 0, formData.Length); DataStream.Close();
            DataStream.Close();
        }

        private string GetContentType(string fileName)
        {
            string rtn = "";

            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".jpg": rtn = "image/jpeg"; break;
                case ".jpeg": rtn = "image/jpeg"; break;
                case ".png": rtn = "image/png"; break;
                case ".gif": rtn = "image/gif"; break;
                case ".tif": rtn = "image/tiff"; break;
                case ".tiff": rtn = "image/tiff"; break;
                case ".zip": rtn = "application/zip"; break;
                case ".pdf": rtn = "application/pdf"; break;
                default: rtn = "application/octet-stream"; break;
            }

            return rtn;
        }

        #endregion File Upload

        #region New Issue

        /// <summary>
        /// Make Issue JSON data for API
        /// </summary>
        public string MakeIssueData(YonaAuthor author, string title, string body, List<string> attachList = null)
        {
            if (title == "") { return "FAILED : title is empty."; }
            if (body == "") { return "FAILED : body is empty."; }

            string rtnJson = "";

            YonaIssue issue = new YonaIssue()
            {
                title = title,
                body = body,
                author = author,
                state = "OPEN",
                type = "ISSUE_POST",
                sendNotification = "true",
                createdAt = $"{DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss", new CultureInfo("en-US"))} +0900",
                updatedAt = $"{DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss", new CultureInfo("en-US"))} +0900",
            };

            if (attachList != null)
            {
                List<long> list = new List<long>();
                List<Attachment> attList = new List<Attachment>();

                attachList.ForEach(x =>
                {
                    Attachment attachment = FileUpload(x);
                    if (attachment != null)
                    {
                        list.Add(attachment.id);
                        attList.Add(attachment);
                    }
                });

                if (list.Count > 0)
                {
                    issue.temporaryUploadFiles = list;      /// temporaryUploadFiles 에 [ 275, 276 ... ] 형태로 붙이면 된다.
                    issue.body += "\r\n\r\n* 첨부파일\r\n";
                    attList.ForEach(x => issue.body += $"  * {(x.mimeType.Contains("image") ? "!" : "")}[{x.name}](/files/{x.id})\r\n");    /// 이미지타입의 첨부파일은 맨앞에 ! 를 붙여서 미리보기 모드로 본문에 첨부한다.
                }
            }

            List<YonaIssue> issues = new List<YonaIssue>();
            issues.Add(issue);

            YonaIssueData data = new YonaIssueData(issues);
            rtnJson = JsonConverter.SerializeJsonUsingJavaScipt(data);

            return rtnJson;
        }

        public string NewIssue(string issueData = "")
        {
            string rtn = "";

            try
            {
                if (issueData == "") throw new Exception("IssueData is null.");

                WebClient client = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Yona-Token", YonaToken);
                client.Encoding = UTF8Encoding.UTF8;

                var response = client.UploadString(new Uri(NEW_ISSUE_HOST), issueData);
                //[{\"status\":201,\"location\":\"/WordTEST/WordTEST_Service_Web/post/4\"}]
                if (response.Length > 0)
                {
                    response = response.Substring(1, response.Length - 2);      // 앞뒤에 [ ] 를 없애줘야 Dictionary로 Convert 된다.
                    Dictionary<string, object> res = JsonConverter.DeserializeJsonUsingJavaScript<Dictionary<string, object>>(response);

                    if (res["status"].ToString() == "201")
                    {
                        rtn = "SUCCESS";
                    }
                    else
                    {
                        throw new Exception(res["status"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                rtn = $"FAILED : {ex.Message}";
            }

            return rtn;
        }

        #endregion New Issue

        #region New Post

        /// <summary>
        /// Make Post JSON data for API
        /// </summary>
        public string MakePostData(YonaAuthor author, string title, string body, List<string> attachList = null)
        {
            if (title == "") { return "FAILED : title is empty."; }
            if (body == "") { return "FAILED : body is empty."; }

            string rtnJson = "";

            YonaPost Post = new YonaPost()
            {
                title = title,
                body = body,
                author = author,
                sendNotification = "true",
                createdAt = $"{DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss", new CultureInfo("en-US"))} +0900",
                updatedAt = $"{DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss", new CultureInfo("en-US"))} +0900",
            };

            if (attachList != null)
            {
                List<long> list = new List<long>();
                List<Attachment> attList = new List<Attachment>();

                attachList.ForEach(x =>
                {
                    Attachment attachment = FileUpload(x);
                    if (attachment != null)
                    {
                        list.Add(attachment.id);
                        attList.Add(attachment);
                    }
                });

                if (list.Count > 0)
                {
                    Post.temporaryUploadFiles = list;      /// temporaryUploadFiles 에 [ 275, 276 ... ] 형태로 붙이면 된다.
                    Post.body += "\r\n\r\n* 첨부파일\r\n";
                    attList.ForEach(x => Post.body += $"  * {(x.mimeType.Contains("image") ? "!" : "")}[{x.name}](/files/{x.id})\r\n");    /// 이미지타입의 첨부파일은 맨앞에 ! 를 붙여서 미리보기 모드로 본문에 첨부한다.
                }
            }

            List<YonaPost> Posts = new List<YonaPost>();
            Posts.Add(Post);

            YonaPostData data = new YonaPostData(Posts);
            rtnJson = JsonConverter.SerializeJsonUsingJavaScipt(data);

            return rtnJson;
        }

        public string NewPost(string PostData = "")
        {
            string rtn = "";

            try
            {
                if (PostData == "") throw new Exception("PostData is null.");

                WebClient client = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Accept", "application/json");
                client.Headers.Add("Yona-Token", YonaToken);
                client.Encoding = UTF8Encoding.UTF8;

                var response = client.UploadString(new Uri(NEW_POST_HOST), PostData);
                //[{\"status\":201,\"location\":\"/WordTEST/WordTEST_Service_Web/post/4\"}]
                if (response.Length > 0)
                {
                    response = response.Substring(1, response.Length - 2);      // 앞뒤에 [ ] 를 없애줘야 Dictionary로 Convert 된다.
                    Dictionary<string, object> res = JsonConverter.DeserializeJsonUsingJavaScript<Dictionary<string, object>>(response);

                    if (res["status"].ToString() == "201")
                    {
                        rtn = "SUCCESS";
                    }
                    else
                    {
                        throw new Exception(res["status"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                rtn = $"FAILED : {ex.Message}";
            }

            return rtn;
        }

        #endregion New Post
    }

    public class Attachment
    {
        public long id { get; set; }
        public string name { get; set; }
        public string hash { get; set; }
        public string containerType { get; set; }
        public string mimeType { get; set; }
        public long size { get; set; }
        public string containerId { get; set; }
        public DateTime createDate { get; set; }
        public string ownerLoginId { get; set; }

        // 리소스 타입 참고 (containerType)
        // https://github.com/yona-projects/yona/blob/master/app/models/enumeration/ResourceType.java
    }

    public class YonaPostData
    {
        public List<YonaPost> posts { get; set; }

        public YonaPostData(List<YonaPost> posts)
        {
            this.posts = posts;
        }
    }

    public class YonaIssueData
    {
        public List<YonaIssue> issues { get; set; }

        public YonaIssueData(List<YonaIssue> issues)
        {
            this.issues = issues;
        }
    }

    public class YonaPost
    {
        public string title { get; set; }
        public string body { get; set; }
        public YonaAuthor author { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string sendNotification { get; set; }
        public List<long> temporaryUploadFiles { get; set; }
        public List<Attachment> attachmentList { get; set; }
    }

    public class YonaIssue
    {
        public string title { get; set; }
        public string body { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public YonaAuthor author { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string sendNotification { get; set; }
        public List<long> temporaryUploadFiles { get; set; }
        public List<Attachment> attachmentList { get; set; }
    }

    public class YonaAuthor
    {
        public string loginId { get; set; }
        public string name { get; set; }
        public string email { get; set; }

        public YonaAuthor(string loginId, string name, string email)
        {
            this.loginId = loginId;
            this.name = name;
            this.email = email;
        }
    }
}