using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeServerSide.Services
{
    public class NetworkService
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 1000000;
        private const int PORT = 27001;
        private static readonly Byte[] buffer = new Byte[BUFFER_SIZE];

        public static bool IsFirst { get; private set; } = false;
        public static char[,] Points { get; private set; } = new char[3, 3]
        {
            { '1','2','3'},
            { '4','5','6'},
            { '7','8','9'}
        };

        public static void Start()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine();
            CloseAllSockets();
        }

        private static void CloseAllSockets()
        {
            foreach (Socket s in clientSockets)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Close();
            }
            serverSocket.Close();
        }

        private static void SetupServer()
        {
            Console.Write("Setting up server . . .");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(2);
            serverSocket.BeginAccept(AcceptCallBack, null);

        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            Socket socket = null;
            try
            {
                socket = serverSocket.EndAccept(ar);
            }
            catch (Exception)
            {
                return;
            }
            clientSockets.Add(socket);

            string t;
            if (!IsFirst)
            {
                IsFirst = true;
                t = "X";
            }
            else
            {
                IsFirst = false;
                t = "O";
            }
            byte[] data = Encoding.ASCII.GetBytes(t);
            socket.Send(data);

            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, socket);
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket current = (Socket)ar.AsyncState;
            int received;
            try
            {
                received = current.EndReceive(ar);
            }
            catch (Exception)
            {
                Console.WriteLine("Client forcefully disconnected!");
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            try
            {
                var no = text[0];
                var symbol = text[1];
                var number = Convert.ToInt32(no) - 49;
                if (number >= 0 && number <= 2)
                    Points[0, number] = symbol;
                else if (number >= 3 && number <= 5)
                    Points[1, number - 3] = symbol;
                else if (number >= 6 && number <= 8)
                    Points[2, number - 6] = symbol;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Console.Write($"{Points[i, k]}");
                    if (k != 2)
                    {
                        Console.Write("|");
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }

            if (text != string.Empty)
            {
                var mydata = ConvertToString(Points);
                byte[] data = Encoding.ASCII.GetBytes(mydata);
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var item in clientSockets)
                {
                    item.Send(data);
                    Console.WriteLine($"Data was sent to {item.RemoteEndPoint}");
                }
                Console.ResetColor();
            }
            else if (text == "exit")
            {
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientSockets.Remove(current);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{ current.RemoteEndPoint} disconnected");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Text is invalid request");
                byte[] data = Encoding.ASCII.GetBytes("Invalid Request");
                current.Send(data);
                Console.WriteLine("Warning Sent");
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, current);
        }

        private static string ConvertToString(char[,] points)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                for (int k = 0; k < points.Length; k++)
                {
                    sb.Append(points[i, k]);
                    sb.Append('\t');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
