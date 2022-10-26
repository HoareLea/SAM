using System.Net.NetworkInformation;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Ping(string hostNameOrAddress = "https://www.google.com/", int timeout = 1500)
        {
            Ping ping = null;

            bool result = false;
            try
            {
                ping = new Ping();
                PingReply pingReply = ping.Send(hostNameOrAddress);
                result = pingReply.Status == IPStatus.Success;
            }
            catch
            {

            }
            finally
            {
                if(ping != null)
                {
                    ping.Dispose();
                }
            }

            return result;
        }
    }
}