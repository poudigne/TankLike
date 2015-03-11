using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace Network.PSoft.Logging
{
  public static class LoggingService
  {
    public static void Debug(string message, string className, string functionCall)
    {
      var writer = new StreamWriter("C:\\Users\\Pierre-Luc\\Documents\\GitHub\\TankLike\\Assets\\scripts\\libs\\log.txt", true);
      string line = string.Format("[{0}],  {2}, Call: {1}.{3}()", DateTime.Now, className, message, functionCall);
      writer.Write("{0}{1}", Environment.NewLine, line);
      writer.Close();
    } 

  }
}
