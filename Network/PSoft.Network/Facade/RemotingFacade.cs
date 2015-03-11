using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Network.PSoft.BusinessObject;
using Network.PSoft.BusinessObject.Remoting;
using Network.PSoft.Logging;

namespace Network.PSoft.Network.Facade
{
  public class RemotingFacade
  {

    private static RemotingFacade instance;

    private const String GET_LINK = "http://api.francoischolette.com/api/1.0/{0}?{1}";
    private const String POST_LINK_BYGAMES = "http://api.francoischolette.com/api/1.0/{0}/post/{1}";
    public const String POST_LINK = "http://api.francoischolette.com/api/1.0/{0}";


    public RemotingFacade()
    {
    }

    public static RemotingFacade GetInstance()
    {
      return instance ?? (instance = new RemotingFacade());
    }

    public string SendRegisterAccountRequest(IURLParameters registerInformations)
    {
      string link = BuildURL(registerInformations, RequestAction.CreateUser);
      LoggingService.Debug(String.Format("Preparing request to send. URL : {0}", link), typeof(RemotingFacade).Name, "SendRegisterAccountRequest");
      return RemotingService.GetInstance().HttpPostRequest(link, registerInformations.GetParametersDict());
    }

    private string BuildURL(IURLParameters registerInformations, RequestAction action)
    {
      switch (action)
      {
        case RequestAction.GetHighscore:
          break;
        case RequestAction.Login:
          break;
        case RequestAction.CreateUser:
          return string.Format(POST_LINK, GetLinkActionString(action));
        default:
          return string.Empty;
      }
      return string.Empty;
    }

    private string GetLinkActionString(RequestAction action)
    {
      switch (action)
      {
        case RequestAction.GetHighscore:
          break;
        case RequestAction.Login:
          break;
        case RequestAction.CreateUser:
          return "users";
        default:
          return string.Empty;
      }
      return string.Empty;
    }
  }
}
