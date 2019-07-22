using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    static Socket socket; //定义套接字

    static byte[] readBuff = new byte[1024]; //接受缓冲区

    public delegate void MsgListener(string str); //委托类型

    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>(); //监听列表
    static List<string> msgList = new List<string>();                                                 //消息列表

    // 添加监听
    public static void AddListene(string msgName, MsgListener listener)
    {
        listeners[msgName] = listener;
    }


    // 返回描述
    public static string GetDesc()
    {
        if (socket == null)
        {
            return "";
        }

        if (!socket.Connected)
        {
            return "";
        }

        return socket.LocalEndPoint.ToString();
    }

    // 连接
    public static void Connect(string ip, int port)
    {
        // Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Connect
        socket.Connect(ip, port);

        // BeginReceive
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

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

    // 发送
    public static void Send(string sendStr)
    {
        if (socket == null)
        {
            return;
        }

        if (!socket.Connected)
        {
            return;
        }

        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
    }


    // Update
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