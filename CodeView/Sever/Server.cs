﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ThreadTCPServer
{
    class Server
    {
        const int SERVERPORT = 9000;
        const int BUFSIZE = 512;

        static List<Socket> socketList = new List<Socket>();

        // 클라이언트와 데이터 통신
        static void ProcessClient(Socket sock, IPEndPoint ip)
        {
            int retval;
            Socket client_sock = sock;
            IPEndPoint clientaddr = ip;
            byte[] buf = new byte[BUFSIZE];

            while (true)
            {
                try
                {
                    // 데이터 받기
                    retval = client_sock.Receive(buf, BUFSIZE, SocketFlags.None);
                    if (retval == 0) break;

                    // 받은 데이터 출력
                    Console.WriteLine("[TCP/{0}:{1}] {2}", clientaddr.Address, clientaddr.Port, Encoding.Default.GetString(buf, 0, retval));

                    // 데이터 보내기
                    foreach(var clientSocket in socketList)
                    {
                        clientSocket.Send(buf, retval, SocketFlags.None);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            client_sock.Close();
            Console.WriteLine("[TCP 서버] 클라이언트 종료: IP 주소={0}, 포트 번호={1}", clientaddr.Address, clientaddr.Port);
        }
        static void Main(string[] args)
        {
            Socket listen_sock = null;

            try
            {
                // 소켓 생성
                listen_sock = new Socket(AddressFamily.InterNetwork,
                 SocketType.Stream, ProtocolType.Tcp);

                // Bind()
                listen_sock.Bind(new IPEndPoint(IPAddress.Any, SERVERPORT));

                // Listen()
                listen_sock.Listen(Int32.MaxValue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 데이터 통신에 사용할 변수
            Socket client_sock = null;
            IPEndPoint clientaddr = null;

            while (true)
            {
                try
                {
                    // accept()
                    client_sock = listen_sock.Accept();
                    socketList.Add(client_sock);

                    // 접속한 클라이언트 정보 출력
                    clientaddr = (IPEndPoint)client_sock.RemoteEndPoint;
                    Console.WriteLine("\n[TCP 서버] 클라이언트 접속: IP 주소={0}, 포트번호 ={1}", clientaddr.Address, clientaddr.Port);

                    // 스레드 생성
                    Thread t = new Thread(() => ProcessClient(client_sock, clientaddr));
                    t.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            listen_sock.Close();
        }
    }
}