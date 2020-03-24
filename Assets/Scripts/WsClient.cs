using System;
using UnityEngine;
using WebSocketSharp;

public partial struct Message
{
    public string Type;
    public string Data;
}

public class WsClient
{
    WebSocket ws_;
    public Action<Message> OnMessage;

    public WsClient(string url)
    {
        ws_ = new WebSocket(url);
        ws_.OnMessage += onMessage;
        ws_.Connect();
    }

    void onMessage(object sender, MessageEventArgs e)
    {
        var msg = JsonUtility.FromJson<Message>(e.Data);
        //Debug.Log("Type: " + msg.Type);
        OnMessage(msg);
    }

    public void SendMessage(string type, object data)
    {
        ws_.Send(JsonUtility.ToJson(new Message
        {
            Type = type,
            Data = JsonUtility.ToJson(data)
        }));
    }

    public void SendMessage(string type, string data)
    {
        ws_.Send(JsonUtility.ToJson(new Message
        {
            Type = type,
            Data = data
        }));
    }

    public void Dispose()
    {
        ws_.Close();
    }
}
