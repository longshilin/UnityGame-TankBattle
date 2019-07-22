using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class FinalChatClient : MonoBehaviour
{
    Socket socket; // 定义套接字

    public InputField inputAddress; // 输入的IP+Port
    public InputField inputMessage; // 输入的消息
    public Text result;

    private string recvStr = "";
    private List<Socket> checkRead = new List<Socket>(); //checkRead列表

    // 点击连接按钮
    public void Connection()
    {
        // socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // connnect
        socket.Connect(inputAddress.text, 14444);
    }

    // 点击发送按钮
    public void Send()
    {
        string sendStr = inputMessage.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            Debug.Log("Socket Send success" + count);
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Connect fail" + e.ToString());
        }
    }

    private void Awake()
    {
        inputAddress.text = "127.0.0.1";
    }

    private void Update()
    {
        if (socket == null)
        {
            return;
        }

        checkRead.Clear();
        checkRead.Add(socket);
        Socket.Select(checkRead, null, null, 0);

        foreach (Socket s in checkRead)
        {
            byte[] readBuff = new byte[1024];
            int count = s.Receive(readBuff);
            string str = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            recvStr = str + "\n" + recvStr;
            result.text = recvStr;
        }
    }
}