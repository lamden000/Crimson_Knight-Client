using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    public static class Session
    {
        public static string serverIp = "127.0.0.1";
        public static int serverPort = 7679;

        private static TcpClient tcpClient;
        private static NetworkStream stream;
        private static BinaryReader reader;
        private static BinaryWriter writer;

        private static Thread receiveThread;
        private static Thread sendThread;
        private static volatile bool isRunning;

        private static readonly ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();
        private static readonly ConcurrentQueue<Message> sendQueue = new ConcurrentQueue<Message>();

        public static void Connect()
        {
            if (isRunning)
            {
                Debug.LogWarning("Đã kết nối.");
                return;
            }

            Debug.Log("Bắt đầu kết nối...");
            isRunning = true;
            //khong chan main thread
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    tcpClient = new TcpClient();
                    Debug.Log($"Đang kết nối đến {serverIp}:{serverPort}...");
                    tcpClient.Connect(serverIp, serverPort);

                    stream = tcpClient.GetStream();
                    reader = new BinaryReader(stream, Encoding.UTF8, true);
                    writer = new BinaryWriter(stream, Encoding.UTF8, true);

                    Debug.Log("Kết nối thành công.");

                    receiveThread = new Thread(ReceiveLoop);
                    receiveThread.IsBackground = true;
                    receiveThread.Start();

                    sendThread = new Thread(SendLoop);
                    sendThread.IsBackground = true;
                    sendThread.Start();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Lỗi kết nối: {ex.Message}");
                    receiveQueue.Enqueue(null);
                }
            });
            connectThread.IsBackground = true;
            connectThread.Start();
        }

        public static void AddMessage(Message msg)
        {
            if (!isRunning)
            {
                Debug.LogWarning("Chưa kết nối. Không thể gửi tin nhắn.");
                msg.Close();
                return;
            }
            sendQueue.Enqueue(msg);
        }


        public static void CheckReceiveMessage()
        {
            while (isRunning && receiveQueue.TryDequeue(out Message msg))
            {
                if (msg == null) // Tín hiệu ngắt kết nối
                {
                    Close();
                    return;
                }

                try
                {
                    MessageHandler.HandleMessage(msg);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Lỗi xử lý tin nhắn: {ex.Message}");
                }
                finally
                {
                    msg.Close();
                }
            }
        }


        private static object lockClose = new object();
        public static void Close()
        {
            if (!isRunning) return;
            lock (lockClose)
            {
                isRunning = false;
                try { reader?.Close(); } catch { }
                try { writer?.Close(); } catch { }
                try { stream?.Close(); } catch { }
                try { tcpClient?.Close(); } catch { }

                receiveQueue.Enqueue(null);

                sendQueue.Clear();
                receiveQueue.Clear();
            }
        }

        private static void ReceiveLoop()
        {
            try
            {
                while (isRunning)
                {
                    int length = reader.ReadInt32();
                    if (length <= 0) continue;

                    byte[] data = reader.ReadBytes(length);
                    Message msg = new Message(data);
                    receiveQueue.Enqueue(msg);
                }
            }
            catch (Exception) { }
            finally
            {
                if (isRunning)
                    receiveQueue.Enqueue(null);
            }
        }


        private static void SendLoop()
        {
            try
            {
                while (isRunning)
                {
                    if (sendQueue.TryDequeue(out Message msg))
                    {
                        SendMessage(msg);
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                if (isRunning)
                    receiveQueue.Enqueue(null);
            }
        }

        private static void SendMessage(Message msg)
        {
            byte[] data = msg.GetData();
            writer.Write(data.Length);
            writer.Write(data);
            writer.Flush();
            msg.Close();
        }


    }
}
