using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bubble
{
    class BiliBili: LiveServerImp
    {
        TcpClient dmClient = null;
        NetworkStream netStream = null;
        String roomId;

        public BiliBili(String roomId)
        {
            this.roomId = roomId;

        }

        public async void run()
        {
            await connect();

        }
        public void stop()
        {
            dmClient.Close();
        }
        /// <summary>
        /// connect to the server of bilibili
        /// </summary>
        /// <returns>if success return true, lese return false</returns>
        private async Task<bool> connect()
        {
            //the ip address of server
            string serverIp = "120.92.112.150";
            //the port of server
            int serverPort = 2243;
            dmClient = new TcpClient();

            Console.WriteLine("start to connect to server");
            await dmClient.ConnectAsync(IPAddress.Parse(serverIp),serverPort);

            netStream = dmClient.GetStream();
            connectToChannel();
            Thread x = new Thread(recevieMsg);
            x.Start();
            bool result = await heartBeatLoop();

            return false;
        }

        private bool connectToChannel()
        {
            List<byte[]> dataStructureList = new List<byte[]>();
            // the head
            dataStructureList.Add(new byte[]{
                0x00,0x00,0x00,0x38,0x00,0x10,0x00,0x01,0x00,0x00,0x00,0x07,0x00,0x00,0x00,0x01,0x7b,0x22,0x72,0x6f,0x6f,0x6d,0x69,0x64,0x22,0x3a });
            // the roomId
            dataStructureList.Add(Encoding.ASCII.GetBytes(roomId));
            dataStructureList.Add(new byte[] { 0x2c, 0x22, 0x75, 0x69, 0x64, 0x22, 0x3a });
            // the random uid
            dataStructureList.Add(Encoding.ASCII.GetBytes(getRandomUid()));
            // the tail
            dataStructureList.Add(new byte[] { 0x7d });

            int totalLength = dataStructureList[0].Length +
                dataStructureList[1].Length +
                dataStructureList[2].Length +
                dataStructureList[3].Length +
                dataStructureList[4].Length;
            dataStructureList[0][3] = (byte)(totalLength);
            List<byte> data = new List<byte>(totalLength);
            data.AddRange(dataStructureList[0]);
            data.AddRange(dataStructureList[1]);
            data.AddRange(dataStructureList[2]);
            data.AddRange(dataStructureList[3]);
            data.AddRange(dataStructureList[4]);

            byte[] msg = data.ToArray();
            msg[3] = (byte)msg.Count();

            //wait for 1 second
            Thread.Sleep(1000);
            try
            {
                netStream.WriteAsync(msg,0,msg.Length);
                netStream.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to send get in room msg");
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        private void recevieMsg()
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("mainForm", out obj);
            MainForm mainForm = (MainForm)obj;

            byte[] stableBuffer = new byte[dmClient.ReceiveBufferSize];
            while (dmClient.Connected)
            {
                
                netStream.readByte(stableBuffer, 0, 4);
                var packetLength = BitConverter.ToInt32(stableBuffer, 0);
                packetLength = IPAddress.NetworkToHostOrder(packetLength);
                if (packetLength < 16)
                {
                    throw new NotSupportedException("protocol failed.: (L:" + packetLength + ")");
                }
                netStream.readByte(stableBuffer, 0, 2);
                netStream.readByte(stableBuffer, 0, 2); 

                netStream.readByte(stableBuffer, 0, 4);
                var typeId = BitConverter.ToInt32(stableBuffer, 0);
                typeId = IPAddress.NetworkToHostOrder(typeId);

                netStream.readByte(stableBuffer, 0, 4);
                var playloadlength = packetLength - 16;
                if (playloadlength == 0)
                {
                    continue;//no content
             
                }
                typeId = typeId - 1;//to fit the decompiled code.
                var buffer = new byte[playloadlength];
                netStream.readByte(buffer, 0, playloadlength);
                switch (typeId)
                {
                    case 0:
                    case 1:
                    case 2:
                        {
                            UInt32 audience = BitConverter.ToUInt32(buffer.Take(4).Reverse().ToArray(), 0);
                            //mainForm.dm_invoke(EnumCommentType.HEART, "", viewerCount.ToString());
                            mainForm.dm_invoke(EnumCommentType.HEART, "",audience.ToString());
                            Console.WriteLine("the current audience number is: "+audience);
                            break;
                        }
                    case 3:
                    case 4://audience Command, includes audience msg, gift msg and welcome msg.
                        {
                            var json = Encoding.UTF8.GetString(buffer, 0, playloadlength);
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + json);
                            mainForm.dm_invoke(EnumCommentType.MSG, "", json.ToString());
                            break;
                        }
                    case 5://newScrollMessage
                        {

                            break;
                        }
                    case 7:
                        {

                            break;
                        }
                    case 16:
                        {
                            break;
                        }
                    default:
                        {

                            break;
                        }
                        //                     
                }
            }
        }

        private Task<bool> heartBeatLoop()
        {
            var task = Task.Run(() =>
            {
                try
                {
                    while (dmClient.Connected)
                    {
                        Send_Heart();
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString() + " send heart");
                        Thread.Sleep(20000);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("head beat error.");
                    Console.WriteLine(e);

                }
                return true;
            }
            );
            return task;
        }

        private void Send_Heart()
        {
            try
            {
                byte[] data = { 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01 };
                 netStream.Write(data, 0, data.Length);
                 netStream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to heat ");
            }

        }

        /// <summary>
        /// random a audience uid
        /// </summary>
        /// <returns>uid</returns>
        private string getRandomUid()
        {
            double uid_guest;
            Random x = new Random();
            uid_guest = Math.Floor((100000000000000.0 + 200000000000000.0 * x.NextDouble()));
            string uid = uid_guest.ToString();
            return uid;
        }
    }
}
