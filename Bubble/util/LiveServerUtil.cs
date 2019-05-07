using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bubble
{
    public static class LiveServerUtil
    {
        private static Dictionary<String, Object> liveServerManager = new Dictionary<String, Object>();

        public static void addServer(String serverName, Object serverClass)
        {
            for (int i = liveServerManager.Count - 1; i >= 0; i--)
            {
                var item = liveServerManager.ElementAt(i);
                if (item.Key.Equals(serverName))
                {
                    liveServerManager.Remove(item.Key);
                }
            }

            liveServerManager.Add(serverName, serverClass);
        }
        public static void deleteServer(String serverName)
        {
            foreach (var item in liveServerManager)
            {
                if (item.Key.Equals(serverName))
                {
                    LiveServerImp server = (LiveServerImp)item.Value;
                    server.stop();
                    liveServerManager.Remove(item.Key);
                }
            }
        }
        public static void runServer(String serverName)
        {
            foreach (var item in liveServerManager)
            {
                if(item.Key.Equals(serverName))
                { 
                    LiveServerImp server = (LiveServerImp)item.Value;
                    server.run();
                }
            }
        }
        public static void stopServer(String serverName)
        {
            foreach (var item in liveServerManager)
            {
                if (item.Key.Equals(serverName))
                {
                    LiveServerImp server = (LiveServerImp)item.Value;
                    server.stop();
                }
            }
        }
        public static void runAllServer()
        {
            foreach (var item in liveServerManager)
            {
                LiveServerImp server = (LiveServerImp)item.Value;
                server.run();
            }
        }
        public static void stopAllServer()
        {
            foreach (var item in liveServerManager)
            {
                LiveServerImp server = (LiveServerImp)item.Value;
                server.stop();
            }
        }


        /// <summary>
        /// read buffer of selected count byte
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public static void readByte(this NetworkStream stream, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new ArgumentException();
            int read = 0;
            while (read < count)
            {
                var available = stream.Read(buffer, offset, count - read);
                if (available == 0)
                {
                    throw new ObjectDisposedException(null);
                }
                //                if (available != count)
                //                {
                //                    throw new NotSupportedException();
                //                }
                read += available;
                offset += available;

            }

        }
    }
}
