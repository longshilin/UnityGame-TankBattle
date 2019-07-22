using System;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ChatClient : MonoBehaviour
{
    Socket socket; // 定义套接字

    public InputField inputField;
    public Text result;

    // 接收缓冲区
    byte[] readBuff = new byte[1024];
    string recvStr = "";

    // 点击连接按钮
    public void Connection()
    {
        // socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // connnect
        socket.BeginConnect("127.0.0.1", 14444, ConnnectCallback, socket);
    }

    // Connect回调
    private void ConnnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connet Success");
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Connect fail" + e.ToString());
        }
    }

    // Receive回调
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket) ar.AsyncState;
            int count = socket.EndReceive(ar);
            string str = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            recvStr = str + "\n" + recvStr;
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Connect fail" + e.ToString());
        }
    }

    // 点击发送按钮
    public void Send()
    {
        // Send
        string sendStr = inputField.text;
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

    private void Update()
    {
        result.text = recvStr;
    }
}