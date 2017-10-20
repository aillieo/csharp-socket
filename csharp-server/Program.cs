using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace csharp_server
{
    class Program
    {
        static void Main(string[] args)
        {
			SocketServer ss = new SocketServer ();
			ss.StartServer ();
		}
    }

	class SocketServer
	{
		public SocketServer(){}

		private Socket _socket;

		public void StartServer()
		{
			IPAddress ip = IPAddress.Parse(csharp_server.Config.server);
			int port = csharp_server.Config.port;
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				_socket.Bind(new IPEndPoint(ip, port));
				_socket.Listen(10);
				Console.WriteLine("Server started {0}:{1} ...",ip.ToString(),port.ToString());  


				Thread thread = new Thread(StartAcceptClients);  
				thread.Start();
			}
			catch
			{
				Console.WriteLine("Start failed");
				Console.ReadLine ();
			}

		}

		void StartAcceptClients()
		{
			while (true)  
			{  
				Socket client = _socket.Accept();  
				Thread receiveThread = new Thread(HandleClientConnected);  
				receiveThread.Start(client);
			}  
		}

		void HandleClientConnected(Object client)
		{
			Socket s = (Socket)client;
			byte[] lenBytes = new byte[4];
			byte[] recvBytes = new byte[csharp_server.Config.buffer_max_length];
			int bytes;
			while (true)
			{
				int len = 0;
				bytes = s.Receive(lenBytes, 4, 0);
				if(bytes > 0)
				{
					len = BitConverter.ToInt32(lenBytes, 0);
				}
				if (bytes < 0)
				{
					break;
				}


				if (len > 0)
				{
					bytes = s.Receive(recvBytes, len , 0);
					Message msg = new Message();
					msg.ParseFromBytes(recvBytes, len);
					Message msg_resp = MessageHandler.instance.HandleMessage (msg);

					byte[] bMsg;
					int len_resp = msg_resp.serializeToBytes(out bMsg);
					byte[] bLen = BitConverter.GetBytes(len_resp);
					byte[] bSend = new byte[bMsg.Length + 4];
					Array.Copy(bLen, 0, bSend, 0, 4);
					Array.Copy(bMsg, 0, bSend, 4, len_resp);
					s.Send(bSend);
				}
				if (bytes < 0)
				{
					break;
				}



			}

			s.Close();

		}
			
	}


	class MessageHandler
	{
		public static readonly MessageHandler instance = new MessageHandler();

		public Message HandleMessage(Message msg)
		{
			Message msg_resp = new Message ();

			if(msg.Type == 0)
			{
				
				msg_resp.Type = 0;
				msg_resp.Content = "Server get your message : " + msg.Content ;
			}

			return msg_resp;
		}



		
	}
}
