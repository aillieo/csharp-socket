using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace csharp_client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("hello client");

            IPAddress ip = IPAddress.Parse(csharp_client.Config.server);
            int port = csharp_client.Config.port;
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, port));
                Console.WriteLine("connected");
            }
            catch
            {
                Console.WriteLine("connect fail");
                return;
            }





            Message msg = new Message();
            msg.Type = 0;
            msg.Content = "hello server";


            byte[] bMsg;
            int len = msg.serializeToBytes(out bMsg);
            byte[] bLen = BitConverter.GetBytes(len);
            byte[] bSend = new byte[bMsg.Length + 4];
            Array.Copy(bLen,0,bSend,0,4);
            Array.Copy(bMsg,0,bSend,4,len);
            clientSocket.Send(bSend);

            System.Console.ReadLine();

        }
    }

    class CommunicationManager
    {
        private CommunicationManager() { }
        public static readonly CommunicationManager instance = new CommunicationManager();


    }

    class SocketClient
    {
    }

    class UIDisplayer
    {
        private UIDisplayer() { }
        public static readonly UIDisplayer instance = new UIDisplayer();





    }
}
