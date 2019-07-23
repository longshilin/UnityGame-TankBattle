using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    public delegate void MsgListener(string str); // 消息体类型对应的委托方法

    private static Socket socket;                                                                     //定义套接字
    private static byte[] readBuff = new byte[1024];                                                  //接受缓冲区
    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>(); //待处理的消息列表
    private static List<string> msgList = new List<string>();                                         //接受服务器的消息体列表

    /// <summary>
    /// 添加监听
    /// </summary>
    /// <param name="msgName">消息体类型</param>
    /// <param name="listener">委托方法名</param>
    public static void AddListene(string msgName, MsgListener listener)
    {
        listeners[msgName] = listener;
    }

    /// <summary>
    /// 客户端连接服务器
    /// </summary>
    /// <param name="ip">服务器ip地址</param>
    /// <param name="port">服务器端口号</param>
    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip, port);
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

    /// <summary>
    /// 获取自己客户端的描述信息
    /// </summary>
    /// <returns>返回带有终端标记的描述信息，socket为null或断开连接时返回""</returns>
    public static string GetDesc()
    {
        if (socket == null) return "";
        if (!socket.Connected) return "";
        return socket.LocalEndPoint.ToString();
    }

    /// <summary>
    /// Receive的回调方法，用于异步处理
    /// </summary>
    /// <param name="ar">IAsyncResult</param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);

            msgList.Add(recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Receive fail" + e.ToString());
        }
    }

    /// <summary>
    /// 服务器向客户端发送消息
    /// </summary>
    /// <param name="sendStr">需要发送的消息体</param>
    public static void Send(string sendStr)
    {
        if (socket == null) return;
        if (!socket.Connected) return;

        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        // socket.Send(sendBytes);
        // 这里修改为异步发送命令，不然会造成客户端阻塞，两次send都完成之后一起接受socket的返回信息，导致帧命令信息未分开。
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        // Debug.Log("Send: " + sendStr);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            //int count = socket.EndSend(ar);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket Send fail" + ex.ToString());
        }
    }


    /// <summary>
    /// 轮询监听并处理，每执行一次处理一个消息体
    /// </summary>
    public static void Update()
    {
        if (msgList.Count <= 0)
        {
            return;
        }

        string msgStr = msgList[0];
        msgList.RemoveAt(0);
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];

        // 监听回调
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }
    }
}