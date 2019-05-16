using Bubble.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bubble
{
    class Twitch:LiveServerImp
    {
        TcpClient dmClient = null;
        NetworkStream netStream = null;
        String roomId;
        String token;
        private object connectresult;
        Regex privmsgRegex;
        public delegate void MessageReceiveHandler(string roomId, string token);
        public event MessageReceiveHandler MessageReceived;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomId">room name</param>
        /// <param name="token">get from "http://www.twitchapps.com/tmi/" </param>
        public Twitch(String roomId,String token)
        {
            this.roomId = roomId;
            this.token = token;
            privmsgRegex = new Regex($@":(?<nickname>[^!@:#\s]+)!(?<realname>[^!@:#\s]+)@(?<host>[^!@:#\s]+) PRIVMSG #{roomId} :(?<message>.+)", RegexOptions.Compiled);
        }

        public async void run()
        {
            getViewrCount();
            bool result = await connect();

            Thread getViewrThread = new Thread(loopToGetViewrCount);
            getViewrThread.Start();


        }
        /// <summary>
        /// connect to the server of twitch
        /// </summary>
        /// <returns>if success return true, lese return false</returns>
        private async Task<bool> connect()
        {
            //the ip address of server
            string host = "irc.chat.twitch.tv";
            //the port of server
            int serverPort = 6667;
            dmClient = new TcpClient();

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString()+" start to connect to server");
            await dmClient.ConnectAsync(host, serverPort);

            netStream = dmClient.GetStream();

            var reader = new StreamReader(netStream);
            var writer = TextWriter.Synchronized(new StreamWriter(netStream) { AutoFlush = true, NewLine = "\r\n" });

            Object obj = null;
            FormUtil.formManager.TryGetValue("mainForm",out obj);
            MainForm mainForm = (MainForm)obj;

            var listeningTask = Task.Run(async () =>
            {
                while (dmClient.Connected)
                {
                    var receivedMessage = await reader.ReadLineAsync();
                    Console.WriteLine("> " + receivedMessage);

                    if (receivedMessage.StartsWith("PING"))
                    {
                        Console.WriteLine("PONG");
                        await writer.WriteLineAsync(receivedMessage.Replace("PING", "PONG"));
                    }

                    var privmsgMatch = privmsgRegex.Match(receivedMessage);
                    if (privmsgMatch.Success)
                    {
                        mainForm.dm_invoke(EnumCommentType.MSG, privmsgMatch.Groups["nickname"].Value, privmsgMatch.Groups["message"].Value);
                        MessageReceived?.Invoke(privmsgMatch.Groups["nickname"].Value, privmsgMatch.Groups["message"].Value);
                    }
                }
            });

            Console.WriteLine("PASS");
            await writer.WriteLineAsync($"PASS {token}");
            Console.WriteLine($"NICK {roomId}");
            await writer.WriteLineAsync($"NICK {roomId}");
            Console.WriteLine($"USER {roomId} 0 * :{roomId}");
            await writer.WriteLineAsync($"USER {roomId} 0 * :{roomId}");
            Console.WriteLine($"JOIN #{roomId}");
            await writer.WriteLineAsync($"JOIN #{roomId}");
            Console.WriteLine("entry in room");


            return false;
        }

        public void stop()
        {
            dmClient.Close();
        }

        private void loopToGetViewrCount()
        {
            Object obj = null;

            FormUtil.formManager.TryGetValue("mainForm", out obj);
            MainForm mainForm = (MainForm)obj;

            while (true)
            {

                int viewerCount = getViewrCount();
                mainForm.dm_invoke(EnumCommentType.HEART,"",viewerCount.ToString());
                Thread.Sleep(10000);
            }
        }

        private int getViewrCount()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/streams/"+roomId);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Headers.Add("Client-ID", "5kw217mse7lux1wklk19hf7qr0yn6e");

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();



            TwitchInfo info = JsonConvert.DeserializeObject<TwitchInfo>(retString.Trim());


            return info.stream.viewers;
        }

    }

}
