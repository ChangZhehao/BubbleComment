using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bubble
{
    class Twitch
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
            bool result = await connect();

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

            Console.WriteLine("start to connect to server");
            await dmClient.ConnectAsync(host, serverPort);

            netStream = dmClient.GetStream();

            var reader = new StreamReader(netStream);
            var writer = TextWriter.Synchronized(new StreamWriter(netStream) { AutoFlush = true, NewLine = "\r\n" });

            var listeningTask = Task.Run(async () =>
            {
                while (true)
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

            listeningTask.Wait();

            return false;
        }
    }
}
