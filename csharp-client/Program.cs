using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace csharp_client
{
    class Program
    {
        static void Main(string[] args)
        {

            bool connected = CommunicationManager.instance.Init();
            if (!connected)
            {
                Console.WriteLine("Cannot connect to server...");
                Console.ReadKey();
                return;
            }
            UIDisplayer.instance.Init();

            while(true)
            {
                ConsoleKeyInfo input = System.Console.ReadKey(true);
                UIDisplayer.instance.HandleInput((int)(input.KeyChar));
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
            return ret;
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


        public void Receive(byte[] data, int bytes)
        {
            int len = 0;
            len = BitConverter.ToInt32(data, 0);
	        Message msg = new Message();
            byte[] bMsg = new byte[len];
            Array.Copy(data,4,bMsg,0,len);
            msg.ParseFromBytes(bMsg, len);
	        HandleMessage(msg );
        }

        private void HandleMessage(Message msg)
        {
            if(msg.Type == 0)
	        {
		        UIDisplayer.instance.AppendMessage(msg.Content);
	        }

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


                Thread tRecv = new Thread(new ThreadStart(ReceiveMsg));
                tRecv.IsBackground = true;
                tRecv.Start();


                return true;
            }
            catch
            {
                Console.WriteLine("connect fail");
                return false;
            }

        }

        void ReceiveMsg()
        {
            byte[] recvBytes = new byte[csharp_client.Config.buffer_max_length];
            int bytes;
            while (true)
            {
                bytes = _socket.Receive(recvBytes, recvBytes.Length, 0);
                if(bytes > 0)
                {
                    CommunicationManager.instance.Receive(recvBytes, recvBytes.Length);
                }
                if (bytes < 0)
                {
                    break;
                }
            }

            _socket.Close();

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
        readonly object msgQueueLock = new object();

        private bool _needRefresh = true;
        Queue<string> _messagesWithFormat = new Queue<string>();
        List<int> _input = new List<int>();


        public bool Init()
        {
            Console.CursorVisible = false;
            DisplayInput();
	        Thread tDisplay = new Thread(new ThreadStart(Display));
            tDisplay.IsBackground = true;
            tDisplay.Start();
            
            return true;
        }

        public void HandleInput(int input)
        {
            lock (msgQueueLock)
            {
                if (input == 8 && _input.Count != 0)
                {
                    _input.RemoveAt(_input.Count - 1);
                }
                else if (input == 13 && _input.Count != 0)
                {
                    int count = _input.Count;
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        sb.Append((char)_input[i]);
                    }
                    string str = sb.ToString();
                    CommunicationManager.instance.SendMessage(0, str);
                    _input.Clear();
                }
                else if (input != 8)
                {
                    _input.Add(input);
                }
                _needRefresh = true;
            }
        }


        private void Display()
        {

            while (true)
            {
                
                if (_needRefresh)
                {
                    _needRefresh = false;
                    ClearScreen();
                    DisplayMessage();
                    Seperate();
                    DisplayInput();
                }
                
            }

        }

        void ClearScreen()
        {
            System.Console.Clear();

        }

        void DisplayMessage()
        {
            lock (msgQueueLock)
            {
                foreach (var msgStr in _messagesWithFormat)
                {
                    Console.WriteLine(msgStr);
                }
            }

        }

        void Seperate()
        {
            int width = 30;
            while (width > 0)
            {
                System.Console.Write("-");
                width--;
            }
            System.Console.WriteLine("");

        }

        void DisplayInput()
        {
            int count = _input.Count;
            for (int i = 0; i < count; i++)
            {
                Console.Write((char)_input[i]);
            }
            Console.WriteLine("");
        }


        public void AppendMessage(string str)
        {
            lock (msgQueueLock)
            {
                if (_messagesWithFormat.Count > csharp_client.Config.max_message_amount_display)
                {
                    _messagesWithFormat.Dequeue();
                }
                _messagesWithFormat.Enqueue(str);

                _needRefresh = true;
            }
        }
    }
}
