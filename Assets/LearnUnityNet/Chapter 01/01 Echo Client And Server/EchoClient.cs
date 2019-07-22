using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class EchoClient : MonoBehaviour
{
    Socket socket; // 定义套接字

    public InputField inputField;
    public Text text;

    // 点击连接按钮
    //点击发送按钮
    public void Connection()
    {
        // socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // connnect
        socket.Connect("127.0.0.1", 13333);
    }

    public void Send()
    {
        // send
        string sendStr = inputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);

        // recv
        byte[] readBuff = new byte[1024];
        int count = socket.Receive(readBuff);
        string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        text.text = recvStr;

        //close
        socket.Close();
    }
}