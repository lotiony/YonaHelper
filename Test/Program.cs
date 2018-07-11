using System;
using System.Collections.Generic;

using Yona;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            YonaHelper yona = new YonaHelper("owner", "projectName", "userToken");
            yona.YONA_HOST = "https://repo.yona.io";

            List<string> attachList = new List<string>();
            attachList.Add(@"D:\s1pro.jpg");
            attachList.Add(@"D:\wordpress-3.8-ko_KR.zip");

            YonaAuthor author = new YonaAuthor("admin", "관리자", "lotiony@gmail.com");
            string title = "테스트 이슈 등록합니다.";
            string body = "API테스트 이슈입니다. 파일 첨부도 테스트 합니다.";

            string data = yona.MakeIssueData(author, title, body, attachList);
            string result = yona.NewIssue(data);

            Console.WriteLine(result);

            Console.ReadKey();
        }
    }
}
