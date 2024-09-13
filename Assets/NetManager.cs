using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    
    static Socket socket;

    private static byte[] readBuff = new byte[1204];

    public delegate void MsgListener(string str);

    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();

    private static List<string> msgList = new List<string>();



    // 添加委托到字典
    public static void AddListener(string key, MsgListener listener)
    {
        if (listeners.ContainsKey(key))
        {
            // 如果字典已经包含此键，使用 += 将新的委托添加到已有的委托链
            listeners[key] += listener;
        }
        else
        {
            // 如果字典中没有此键，直接将委托添加
            listeners[key] = listener;
        }
    }

    public static void RemoveListener(string key, MsgListener listener)
    {
        if (listeners.ContainsKey(key))
        {
           
            listeners[key] -= listener;
        }
      
    }
    // 调用指定键的委托
    public static void InvokeListener(string key, string message)
    {
        if (listeners.ContainsKey(key) && listeners[key] != null)
        {
            // 调用委托
            listeners[key].Invoke(message);
        }
    }
    
    public static string GetDesc()
    {
        if (socket == null)
        {
            return "NullSocket";
        }

        if (!socket.Connected)
        {
            return "UnConnectedSocket";
        }

        return socket.LocalEndPoint.ToString();
    }
    
    
    //链接
    public static void Connect(string ip, int port)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ip,port);
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

    static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);

            string recvStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            msgList.Add(recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (Exception e)
        {
            Debug.LogError("Socket Receive fail"+e.ToString());
        }
    }


    public static void Send(string sendStr)
    {
        if(socket==null)
            return;
        if(!socket.Connected)
            return;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);
    }

    public static void Update()
    {
        if(msgList.Count<=0)
            return;

        string msgStr = msgList[0];
        msgList.RemoveAt(0);
        Debug.Log("受到来自服务端消息："+msgStr);

        string[] split = msgStr.Split("|");
        string msgName = split[0];
        string msgArgs = split[1];

        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName]?.Invoke(msgArgs);
        }
    }
}
