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

            CommunicationManager cm = CommunicationManager.instance;
            bool ret = cm.Init();

            while(true)
            {
                string s = System.Console.ReadLine();
                UIDisplayer.instance.HandleInput(s);
            }

        }
    }

    class CommunicationManager
    {
        private CommunicationManager() { }
        public static readonly CommunicationManager instance = new CommunicationManager();

        SocketClient _socketClient;

        public bool Init()
        {

            _socketClient = new SocketClient();
            bool ret = _socketClient.ConnectServer();
            return true;
        }

        public bool SendMessage(int type, string str )
        {
            Message msg = new Message();
            msg.Type = 0;
            msg.Content = str;


            byte[] bMsg;
            int len = msg.serializeToBytes(out bMsg);
            byte[] bLen = BitConverter.GetBytes(len);
            byte[] bSend = new byte[bMsg.Length + 4];
            Array.Copy(bLen, 0, bSend, 0, 4);
            Array.Copy(bMsg, 0, bSend, 4, len);
            _socketClient.Send(bSend);

            return true;
        }


    }

    class SocketClient
    {
        public SocketClient(){}

        private Socket _socket;

        public bool ConnectServer()
        {
            IPAddress ip = IPAddress.Parse(csharp_client.Config.server);
            int port = csharp_client.Config.port;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(new IPEndPoint(ip, port));
                //Console.WriteLine("connected");
                return true;
            }
            catch
            {
                Console.WriteLine("connect fail");
                return false;
            }

        }


        public void Send(byte[] data)
        {
            _socket.Send(data);
        }

    }

    class UIDisplayer
    {
        private UIDisplayer() { }
        public static readonly UIDisplayer instance = new UIDisplayer();




        public void HandleInput(string str)
        {
             CommunicationManager.instance.SendMessage(0, str);
        }




    }
}
