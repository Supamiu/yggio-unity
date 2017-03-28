using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace YggioUnity
{
    public class GameClient
    {
        private double version = 1.0;
        public Queue<Action> updateQueue = new Queue<Action>();
        public BinaryReader NetworkIn;
        public BinaryWriter NetworkOut;
        private NetworkStream networkStream;
        private MemoryStream memoryStream;
        private Thread networkThread;

        public static GameClient Singleton { get; private set; }

        public TcpClient TcpNetworking { get; private set; }

        public UUID UdpKey { get; private set; }

        private string host { get; set; }

        public int TcpPort { get; set; }

        public int UdpPort { get; set; }

        public bool isRunning { get; set; }

        public double Version
        {
            get { return version; }
            set { version = value; }
        }

        public GameClient(string host)
        {
            if (Singleton != null)
            {
                Debug.LogError("There was already an instance of ogclient-framework::GameClient created, canceling operating.");
            }
            else
            {
                Singleton = this;
                this.host = host;
                TcpPort = -1;
                UdpPort = -1;
            }
        }

        public void Init()
        {
            if (TcpPort == -1)
            {
                Debug.LogError("The TCP port was not set, the client must establish a TCP Connection");
                Application.Quit();
            }
            else
            {
                if (UdpPort == -1)
                    Debug.LogWarning("The UDP port was not set, UDP networking will be disabled.");
                TcpNetworking = new TcpClient();
                TcpNetworking.Connect(host, TcpPort);
                NetworkOut = new BinaryWriter(TcpNetworking.GetStream());
                networkStream = new NetworkStream(TcpNetworking.Client);
                NetworkIn = new BinaryReader(networkStream);
                networkThread = new Thread(NetworkLoop);
                networkThread.Start();
            }
        }

        public void Prepare(Action task)
        {
            updateQueue.Enqueue(task);
        }

        public void Update()
        {
            if (updateQueue.Count <= 0)
                return;
            updateQueue.Dequeue()();
        }

        private void NetworkLoop()
        {
            isRunning = true;
            while (isRunning && TcpNetworking.Connected)
            {
                var num = 0;
                if (TcpNetworking.Client.Available < 4) continue;
                for (var index = 0; index < 4; ++index)
                    num = (num << 8) + networkStream.ReadByte();
                Debug.Log("Packet length: " + num);
                if (TcpNetworking.Client.Available < num)
                {
                    Debug.Log("Not enough incoming data, (" + TcpNetworking.Client.Available + ": received) expected: " + num);
                }
                else
                {
                    var index = ByteBuffer.ReadInt();
                    Debug.Log("Incoming packet: " + index);
                    if (index != 0)
                    {
                        try
                        {
                            Debug.Log("Part A[" + index + "]");
                            var packet = PacketManager.Packets[index];
                            Debug.Log("Part B[" + index + "]");
                            Debug.Log("Part C[" + index + "]");
                            if (packet != null)
                            {
                                Debug.Log(packet.GetType() + " Wtf");
                                packet.Decode();
                            }
                            else
                                Debug.LogWarning("A packet with the id of (" + index + ") was requested, but is not handled.");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                    else
                    {
                        UdpKey = ByteBuffer.ReadUUID();
                        Debug.Log("Client UUID(128bits): " + UdpKey.MostSigBits + "||" + UdpKey.LeastSigBits);
                    }
                }
            }
        }

        public void OnApplicationQuit()
        {
            TcpNetworking.Close();
            NetworkIn.Close();
            NetworkOut.Close();
            isRunning = false;
        }
    }
}
