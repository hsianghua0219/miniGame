using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEngine : MonoBehaviour
{
    public Camera TrackingCamera;
    public PlayerController Player;

    public GameObject UserPlayer, ZombieObj;

    public static GameEngine Instance { get; private set; }

    WsClient client_;
    List<UserPlayer> playerList_ = new List<UserPlayer>();
    List<UserData> users_ = new List<UserData>();
    float frameCount_;

    public List<Zombie> zombieList_ = new List<Zombie>();
    List<ZombieData> zombie_ = new List<ZombieData>();

    public bool GameStarted { get; private set; } = false;

    Stack<Message> messages_ = new Stack<Message>();

    float AngleSave;
    public float AngleUpdate = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    void Start() { Init(); }

    public void Init()
    {
        string hostname = Dns.GetHostName();
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);
        client_ = new WsClient("ws://192.168.8.54:3000") { OnMessage = OnMessage };
    }

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
            case Message.ZombieJoin:
                {
                    var data = JsonUtility.FromJson<ZombieMessage>(msg.Data);
                    zombie_ = data.zombie;
                    var obj = CreateZombie(zombie_[zombie_.Count - 1]);
                    zombieList_.Add(obj);
                }
                break;
            case Message.ZombieMove:
                {
                    var data = JsonUtility.FromJson<ZombieMessage>(msg.Data);
                    zombie_ = data.zombie;
                    for (int i = 0; i < zombie_.Count; i++)
                    {
                        var zombieID = FindZombie(zombie_[i].Id);
                        if (zombieID != null)
                        {
                            zombieID.moveX = zombie_[i].moveX;
                            zombieID.moveZ = zombie_[i].moveZ;
                        }
                    }
                }
                break;
            case Message.ZombieLockPlayer:
                {
                    var data = JsonUtility.FromJson<UpdateZombieMessage>(msg.Data);
                    var zombieID = FindZombie(data.zombie.Id);
                    if (zombieID != null && zombieID.Player == null && zombieID.HP > 0) zombieID.transform.position = new Vector3(data.zombie.X, 2f, data.zombie.Z);
                    if (zombieID != null && zombieID.HP > data.zombie.HP) zombieID.HP = data.zombie.HP;
                }
                break;
            case Message.ActionDamge:
                {
                    var data = JsonUtility.FromJson<ActionDamageMessage>(msg.Data);
                    var player = FindUserPlayer(data.UserId);
                    if (player != null) player.Damage(data.Damage);

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
                    bool exists = ((IList)playerList_).Contains(data.User);
                    if (player != null && player != Player.UserPlayer)
                    {
                        player.point = data.User.point;
                        player.nowV3 = new Vector3(data.User.X, player.transform.position.y, data.User.Z);
                        player.transform.rotation = Quaternion.Euler(0, data.User.Angle, 0);
                        player.HP = data.User.HP;
                        player.updata = true;
                        switch (data.User.Speed)
                        {
                            case 0: player.Speed = 0; break;
                            case 1: player.Speed = 10; break;
                            case 2: player.Speed = 20; break;
                        }
                    } else if (!exists && player == null)
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
                            TrackingCamera.gameObject.GetComponent<PlayerCamera>().Player = obj.gameObject;
                            Player.Init(obj);
                        }
                    }
                    zombie_ = data.zombie;
                    foreach (var zombie in zombie_)
                    {
                        var obj = CreateZombie(zombie);
                        zombieList_.Add(obj);
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
        var c = FindUser(Player.UserPlayer.UserId);
        bool v = frameCount_ % 30 == 0 || (Player.UserPlayer.transform.eulerAngles.y - AngleSave) > AngleUpdate && frameCount_ % 3 == 0 || (Player.UserPlayer.transform.eulerAngles.y - AngleSave) < (AngleUpdate * -1) && frameCount_ % 3 == 0;
        if (v)
        {
            var msg = new UpdateUserMessage();
            c.X = Player.UserPlayer.transform.position.x;
            c.Z = Player.UserPlayer.transform.position.z;
            c.Angle = Player.UserPlayer.transform.eulerAngles.y;
            c.point = Player.point;
            c.HP = Player.UserPlayer.HP;
            c.IsDash = Player.UserPlayer.IsDash;
            c.Speed = Player.gameState;
            msg.User = c;
            Send(Message.UpdateUser, msg);
            AngleSave = Player.UserPlayer.transform.eulerAngles.y;
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

    public ZombieData FindZombieData(int id)
    {
        return zombie_.First(u => u.Id == id);
    }

    public UserPlayer FindUserPlayer(int id)
    {
        return playerList_.FirstOrDefault(v => v.UserId == id);
    }

    public Zombie FindZombie(int id)
    {
        return zombieList_.FirstOrDefault(v => v.Id == id);
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
        return player;
    }

    Zombie CreateZombie(ZombieData z)
    {
        var obj = Instantiate(ZombieObj);
        var pos = new Vector3(z.X, 1.5f, z.Z);
        obj.transform.position = pos;
        obj.SetActive(true);
        var zombie = obj.GetComponent<Zombie>();
        zombie.Id = z.Id;
        zombie.HP = z.HP;
        return zombie;
    }

    public void RestartGAME() => SceneManager.LoadScene(0);
}

public partial struct Message
{
    public const string GameStart = "gameStart";
    public const string ExitUser = "exitUser";
    public const string Join = "join";
    public const string UpdateUser = "updateUser";
    public const string ActionShot = "actionShot";
    public const string ActionDamge = "actionDamage";
    public const string ZombieJoin = "createZombie";
    public const string ZombieMove = "ZombieMove";
    public const string ZombieLockPlayer = "ZombieLockPlayer";
}

[Serializable]
class GameStartMessage
{
    public List<UserData> Users;
    public List<ZombieData> zombie;
    public UserData Player;
}

[Serializable]
class ZombieMessage
{
    public List<ZombieData> zombie;
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
struct UpdateZombieMessage
{
    public ZombieData zombie;
}

[Serializable]
public struct ActionShotMessage
{
    public int UserId;
}

[Serializable]
public struct ActionDamageMessage
{
    public int UserId, Damage;
}

[Serializable]
public struct UserData
{
    public int Id, HP, Speed;
    public string WsName, Name;
    public float X, Z, Angle;
    public Vector3 point;
    public bool IsDash;
}

[Serializable]
public struct ZombieData
{
    public int Id, HP;
    public float X, Z, moveX, moveZ;
}