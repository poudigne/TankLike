using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Network.PSoft.Logging;

namespace Network.PSoft.Network
{
  public class RemotingService
  {
    private String gameName { get; set; }
    private static RemotingService instance;

    public RemotingService()
    {
    }
    public RemotingService(string name)
    {
      gameName = name;
    }

    public static RemotingService GetInstance()
    {
      if (instance == null)
        instance = new RemotingService(string.Empty);
      return instance;
    }

    public string GetGameName()
    {
      return gameName;
    }

    /*public string SendRequestPost(string link, string parameters)
    {
      var data = Encoding.ASCII.GetBytes(parameters);
      var request = WebRequest.Create(link);
      request.Credentials = CredentialCache.DefaultCredentials;
      request.Method = RequestMethod.POST.ToString();
      request.ContentType = "application/x-www-form-urlencoded";
      request.ContentLength = data.Length;
      
      using (var stream = request.GetRequestStream())
      {
        stream.Write(data, 0, data.Length);
      }

      LoggingService.Debug(String.Format("Registering user sent, waiting for answer from API."), typeof(RemotingService).Name, "SendRequestPost");
      var response = (HttpWebResponse)request.GetResponse();

      var dataStream = response.GetResponseStream();

      if (dataStream != null)
      {
        var reader = new StreamReader(dataStream);
        var responseFromServer = reader.ReadToEnd();
      
        reader.Close();
        dataStream.Close();
        response.Close();

        return responseFromServer;
      }
      return string.Empty;
    }*/

    public string HttpPostRequest(string url, Dictionary<string, string> postParameters)
    {
      LoggingService.Debug(String.Format("Post URL : {0}", url), typeof(RemotingService).Name, "HttpPostRequest");
      var postData = "";

      foreach (string key in postParameters.Keys)
      {
        postData += HttpUtility.UrlEncode(key) + "="
              + HttpUtility.UrlEncode(postParameters[key]) + "&";
      }

      LoggingService.Debug(String.Format("Post Data : {0}", postData), typeof(RemotingService).Name, "HttpPostRequest");

      var myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
      myHttpWebRequest.Method = "POST";

      var data = Encoding.ASCII.GetBytes(postData);

      myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
      myHttpWebRequest.ContentLength = data.Length;

      var requestStream = myHttpWebRequest.GetRequestStream();
      requestStream.Write(data, 0, data.Length);
      requestStream.Close();

      var myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

      var responseStream = myHttpWebResponse.GetResponseStream();

      var myStreamReader = new StreamReader(responseStream, Encoding.Default);

      var pageContent = myStreamReader.ReadToEnd();
      LoggingService.Debug(String.Format("Response received : {0}", pageContent), typeof(RemotingService).Name, "HttpPostRequest");
      myStreamReader.Close();
      responseStream.Close();

      myHttpWebResponse.Close();

      return pageContent;
    }

  }

  public enum RequestMethod
  {
    POST,
    GET
  }

  public enum RequestAction
  {
    GetHighscore,
    Login,
    CreateUser
  }
}
