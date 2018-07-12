# Yona Helper
    - Yona Issue, Post API wrapper -
    외부 환경에서 Yona의 이슈, 게시판에 글을 작성할 수 있는 API Wrapper dll 프로젝트입니다.
    간단한 내용 또는 일정 포맷이 정해진 글에 첨부파일 등을 붙여서 등록 가능합니다.

## * SPEC
  * Language : C# (6.0, .net framework 4.6 이상)
  * IDE : VS2017 (VS2013 이상에서 실행 가능)
  * Runtime : .Net framework 4.0 이상
  * x86, x64 겸용

## * 제공 함수 및 속성
  * Properties
    * YONA_HOST : 요나 서비스의 메인 호스트 속성입니다.
    * Owner : 프로젝트 그룹 오너명
    * Project : 프로젝트 경로
    * YonaToken : 글 작성자 사용자 토큰값
  * Major Functions
    * SetHost(string host) : host값을 입력받아 YONA_HOST를 셋팅합니다.
    * FileUpload(string fileName) : 로컬의 파일을 요나 서버로 업로드하고 Attachment객체를 받습니다.
    * MakeIssueData : API를 통해 '이슈'로 등록할 데이터의 Json을 만듭니다. API Payload에 맞는 Json 반환됩니다.
    * NewIssue : 만들어진 Json을 Issue api를 이용해 등록합니다.
    * MakePostData : API를 통해 '게시판'에 등록할 데이터의 Json을 만듭니다. API Payload에 맞는 Json 반환됩니다.
    * NewPost : 만들어진 Json을 Post api를 이용해 등록합니다.

## * Usage
  1. YonaHelper 프로젝트를 빌드합니다.
  2. Test프로젝트에서 YonaHelper.dll 파일을 참조합니다.
  3. YonaHelper를 초기화 합니다. 이 때 Owner, ProjectName, UserToken(글작성자) 이 필요합니다.
  Yona 서비스의 도메인도 설정해 줍니다.
  ```cs
  YonaHelper yona = new YonaHelper("owner", "projectName", "userToken");
  yona.SetHost("https://repo.yona.io");
  ```
  
  4. 첨부할 파일이 있을 경우 List<string> 리스트를 만들고 파일 경로를 추가합니다.
  ```cs
  List<string> attachList = new List<string>();
  attachList.Add(@"D:\s1pro.jpg");
  attachList.Add(@"D:\wordpress-3.8-ko_KR.zip");
  ```
  
  5. 글작성자(YonaAuthor)객체를 만들어줍니다. userToken값을 가져온 사용자와 일치해야 합니다.
  ```cs
  // YonaAuthor author = new YonaAuthor("아이디", "이름", "이메일");
  YonaAuthor author = new YonaAuthor("admin", "관리자", "lotiony@gmail.com");
  ```
  
  6. 작성할 글의 제목과 내용을 준비합니다.
  ```cs
  string title = "테스트 이슈 등록합니다.";
  string body = "API테스트 이슈입니다. 파일 첨부도 테스트 합니다.";  
  ```
  
  7. 준비된 글작성자, 제목, 내용, 첨부파일 값으로 Json payload를 만들어줍니다. 
  첨부파일이 없으면 attachList 파라메터는 null로 보냅니다.
  해당 함수 안에서 첨부파일을 업로드하고 temporaryUploadFiles 에 File id를 붙이게 됩니다.
  첨부파일 업로드에 오류가 있으면 해당 파일을 pass하고 글만 올라가게 됩니다.
  ```cs
  string json = yona.MakeIssueData(author, title, body, attachList);
  // string json = yona.MakeIssueData(author, title, body);  // 첨부파일 없을 경우
  ```
  
  8. 만들어진 json data에 이상이 없으면 실제로 API를 호출해 글을 등록합니다.
  리턴값은 성공시 SUCCESS   실패시 FAILED : api에서 리턴한 메시지
  ```cs
  string result = yona.NewIssue(json);
  ```
  
  * 파일을 첨부하게 되면 작성내용(body) 하단에 '첨부파일' 목록을 추가로 덧붙입니다.
    * 이미지파일의 경우 본문에 미리보기 모드로
    * 이외의 파일이면 파일명(클릭시 다운로드)로
    
    
 ### 문의 : lotiony@gmail.com
