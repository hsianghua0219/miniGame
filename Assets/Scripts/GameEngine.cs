﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public Camera TrackingCamera;
    public PlayerController Player;

    public GameObject UserPlayer, ZombieObj;

    public static GameEngine Instance { get; private set; }

    WsClient client_;
    List<UserPlayer> playerList_ = new List<UserPlayer>();
    List<UserData> users_ = new List<UserData>();
    int frameCount_;

    List<Zombie> zombieList_ = new List<Zombie>();
    List<ZombieData> zombie_ = new List<ZombieData>();

    public bool GameStarted { get; private set; } = false;

    Stack<Message> messages_ = new Stack<Message>();

    void Awake()
    {
        UserPlayer.SetActive(false);
        //ZombieObj.SetActive(false);
        Instance = this;
    }

    void Start() { Init(); }

    public void Init() { client_ = new WsClient("ws://localhost:3000") { OnMessage = OnMessage }; }

    void OnApplicationQuit() { client_.Dispose(); }

    void Update()
    {
        if (messages_.Count > 0)
        {
            var msg = messages_.Pop();
            if (GameStarted) UpdateMessageForGameStarted(msg);
            else UpdateMessage(msg);
        }
        if (GameStarted) UpdateServerUser();
    }

    void UpdateMessageForGameStarted(Message msg)
    {
        switch (msg.Type)
        {
            case Message.Zombie:
                {
                    var data = JsonUtility.FromJson<ZombieMessage>(msg.Data);
                    var player = FindUserPlayer(data.Zombie.Id);
                    if (player != null)
                    {
                        CharacterController controller = player.GetComponent<CharacterController>();
                        Vector3 v = Vector3.ClampMagnitude(new Vector3(data.Zombie.X, 0f, data.Zombie.Z) - player.transform.position, 1f);
                        controller.transform.rotation = Quaternion.Euler(0, data.Zombie.Angle, 0);
                        controller.Move(v);
                        player.HP = data.Zombie.HP;
                    }
                    else
                    {
                        zombie_.Add(data.Zombie);
                        zombieList_.Add(CreateZombie(data.Zombie));
                    }
                }
                break;
            case Message.ActionShot: // 弾を撃つ
                {
                    var data = JsonUtility.FromJson<ActionShotMessage>(msg.Data);
                    var player = FindUserPlayer(data.UserId);
                    //player.Shot();
                }
                break;
            case Message.ActionDamge: 
                {
                    var data = JsonUtility.FromJson<ActionDamageMessage>(msg.Data);
                    var player = FindUserPlayer(data.UserId);
                    player.Damage(data.Damage);

                    if (player.IsDead)
                    {
                        playerList_.Remove(player);

                        if (player.UserId == Player.UserPlayer.UserId)
                        {
                            client_.Dispose();
                            GameStarted = false;
                            TrackingCamera.transform.SetParent(null, false);
                            foreach (var n in playerList_)
                            {
                                Destroy(n.gameObject);
                            }
                            playerList_.Clear();
                        }

                        StartCoroutine(player.Dead());
                    }
                }
                break;
            case Message.UpdateUser:
                {
                    var data = JsonUtility.FromJson<UpdateUserMessage>(msg.Data);
                    var player = FindUserPlayer(data.User.Id);
                    if (player != null)
                    {
                        CharacterController controller = player.GetComponent<CharacterController>();
                        Vector3 v = Vector3.ClampMagnitude(new Vector3(data.User.X, 0f, data.User.Z) - player.transform.position, 1f);
                        controller.transform.rotation = Quaternion.Euler(0, data.User.Angle, 0);
                        controller.Move(v);
                        player.HP = data.User.HP;
                    }
                    else
                    {
                        users_.Add(data.User);
                        playerList_.Add(CreateUserPlayer(data.User));
                    }
                }
                break;
            case Message.ExitUser:
                {
                    var data = JsonUtility.FromJson<ExitUserMessage>(msg.Data);
                    var user = users_.First(u => u.WsName == data.WsName);
                    var player = playerList_.First(v => v.UserId == user.Id);
                    playerList_.Remove(player);
                    Destroy(player.gameObject);
                }
                break;
        }
    }

    void UpdateMessage(Message msg)
    {
        switch (msg.Type)
        {
            case Message.Join:
                client_.SendMessage(Message.GameStart, "Player");
                break;
            case Message.GameStart:
                {
                    var data = JsonUtility.FromJson<GameStartMessage>(msg.Data);
                    users_ = data.Users;
                    foreach (var user in users_)
                    {
                        var obj = CreateUserPlayer(user);
                        playerList_.Add(obj);
                        if (user.Id == data.Player.Id)
                        {
                            TrackingCamera.gameObject.GetComponent<PlayerCamera>().Player = obj.transform;
                            Player.Init(obj);
                        }
                    }
                    GameStarted = true;
                }
                break;
        }
    }

    void OnMessage(Message msg)
    {
        messages_.Push(msg);
    }

    void UpdateServerUser()
    {
        if (frameCount_ % 3 == 0)
        {
            var msg = new UpdateUserMessage();
            var c = FindUser(Player.UserPlayer.UserId);
            c.X = Player.UserPlayer.transform.position.x;
            c.Z = Player.UserPlayer.transform.position.z;
            c.Angle = Player.UserPlayer.transform.eulerAngles.y;
            c.HP = Player.UserPlayer.HP;
            c.Power = Player.UserPlayer.Power;
            c.IsDash = Player.UserPlayer.IsDash;
            msg.User = c;
            Send(Message.UpdateUser, msg);
        }
        frameCount_++;
    }

    public void Send(string type, object data)
    {
        client_.SendMessage(type, data);
    }

    public UserData FindUser(int id)
    {
        return users_.First(u => u.Id == id);
    }

    public UserPlayer FindUserPlayer(int id)
    {
        return playerList_.FirstOrDefault(v => v.UserId == id);
    }

    UserPlayer CreateUserPlayer(UserData u)
    {
        var obj = Instantiate(UserPlayer);
        var pos = new Vector3(u.X, 1.5f, u.Z);
        obj.transform.position = pos;
        obj.SetActive(true);
        var player = obj.GetComponent<UserPlayer>();
        player.UserId = u.Id;
        player.HP = u.HP;
        player.Power = u.Power;
        //player.SetMovePosition(pos, u.IsDash);
        //player.SetQuaternion(u.Angle);
        return player;
    }

    Zombie CreateZombie(ZombieData z)
    {
        var obj = Instantiate(UserPlayer);
        var pos = new Vector3(z.X, 1.5f, z.Z);
        obj.transform.position = pos;
        obj.SetActive(true);
        var player = obj.GetComponent<Zombie>();
        player.Id = z.Id;
        player.HP = z.HP;
        return player;
    }
}

public partial struct Message
{
    public const string GameStart = "gameStart";
    public const string ExitUser = "exitUser";
    public const string Join = "join";
    public const string UpdateUser = "updateUser";
    public const string ActionShot = "actionShot";
    public const string ActionDamge = "actionDamage";
    public const string Zombie = "zombie";
}

[Serializable]
class GameStartMessage
{
    public List<UserData> Users;
    public UserData Player;
}

[Serializable]
class ExitUserMessage
{
    public string WsName;
}

[Serializable]
struct UpdateUserMessage
{
    public UserData User;
}

[Serializable]
public struct ActionShotMessage
{
    public int UserId;
}

[Serializable]
public struct ActionDamageMessage
{
    public int UserId;
    public int Damage;
}

[Serializable]
public struct UserData
{
    public int Id;
    public string WsName;
    public string Name;
    public int HP;
    public int Power;
    public float Angle;
    public float X;
    public float Z;
    public bool IsDash;
}

[Serializable]
struct ZombieMessage
{
    public ZombieData Zombie;
}

[Serializable]
public struct ZombieData
{
    public int Id;
    public int HP;
    public float Angle;
    public float X;
    public float Z;
}