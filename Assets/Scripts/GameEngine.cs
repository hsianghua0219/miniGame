using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public Camera TrackingCamera;
    public PlayerController Player;

    public GameObject UserPlayer;

    public static GameEngine Instance { get; private set; }

    WsClient client_;
    List<UserPlayer> playerList_ = new List<UserPlayer>();
    List<UserData> users_ = new List<UserData>();
    int frameCount_;

    public bool GameStarted { get; private set; } = false;

    Stack<Message> messages_ = new Stack<Message>();

    void Awake()
    {
        UserPlayer.SetActive(false);
        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        client_ = new WsClient("ws://localhost:3000");
        client_.OnMessage = onMessage;
    }

    void OnApplicationQuit()
    {
        client_.Dispose();
    }

    void Update()
    {
        if (messages_.Count > 0)
        {
            var msg = messages_.Pop();
            if (GameStarted)
            {
                updateMessageForGameStarted(msg);
            }
            else
            {
                updateMessage(msg);
            }
        }

        if (GameStarted)
        {
            updateServerUser();
        }
    }

    /// <summary>
    /// ゲーム開始後のMessage処理
    /// </summary>
    /// <param name="msg"></param>
    void updateMessageForGameStarted(Message msg)
    {
        switch (msg.Type)
        {
            case Message.ActionShot: // 弾を撃つ
                {
                    var data = JsonUtility.FromJson<ActionShotMessage>(msg.Data);
                    var player = FindUserPlayer(data.UserId);
                    //player.Shot();
                }
                break;
            case Message.ActionDamge: // ダメージを受ける
                {
                    var data = JsonUtility.FromJson<ActionDamageMessage>(msg.Data);
                    var player = FindUserPlayer(data.UserId);
                    player.Damage(data.Damage);

                    // HPが0以下になったら、死ぬ
                    if (player.IsDead)
                    {
                        playerList_.Remove(player);

                        // プレイヤーが死んだときの処理
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

                        // プレイヤーのニワトリを破棄
                        StartCoroutine(player.Dead());
                    }
                }
                break;
            case Message.UpdateUser: // 座標を更新する
                {
                    var data = JsonUtility.FromJson<UpdateUserMessage>(msg.Data);
                    var player = FindUserPlayer(data.User.Id);
                    if (player != null)
                    {
                        player.SetMovePosition(new Vector3(data.User.X, 0, data.User.Y), data.User.IsDash);
                        player.SetQuaternion(data.User.Angle);
                    }
                    else
                    {
                        users_.Add(data.User);
                        playerList_.Add(createUserPlayer(data.User));
                    }
                }
                break;
            case Message.ExitUser: // 退室したユーザの処理
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

    /// <summary>
    /// ゲーム開始前のMessage処理
    /// </summary>
    /// <param name="msg"></param>
    void updateMessage(Message msg)
    {
        switch (msg.Type)
        {
            case Message.Join: // 接続成功
                client_.SendMessage(Message.GameStart, "Player");
                break;
            case Message.GameStart: // ゲーム開始
                {
                    var data = JsonUtility.FromJson<GameStartMessage>(msg.Data);
                    users_ = data.Users;
                    foreach (var user in users_)
                    {
                        var obj = createUserPlayer(user);
                        playerList_.Add(obj);

                        // プレイヤーの操作するオブジェクトの初期化
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

    /// <summary>
    /// WebSocketからの処理を受け取る
    /// </summary>
    /// <param name="msg"></param>
    void onMessage(Message msg)
    {
        messages_.Push(msg);
    }

    /// <summary>
    /// サーバーのキャラクター情報を更新する
    /// </summary>
    void updateServerUser()
    {
        // 3フレームごとに座標を同期
        if (frameCount_ % 3 == 0)
        {
            var msg = new UpdateUserMessage();
            var c = FindUser(Player.UserPlayer.UserId);
            c.X = Player.UserPlayer.transform.position.x;
            c.Y = Player.UserPlayer.transform.position.z;
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

    UserPlayer createUserPlayer(UserData u)
    {
        var obj = Instantiate(UserPlayer);
        var pos = new Vector3(u.X, 0, u.Y);
        obj.transform.position = pos;
        obj.SetActive(true);
        var player = obj.GetComponent<UserPlayer>();
        player.UserId = u.Id;
        player.HP = u.HP;
        player.Power = u.Power;
        player.SetMovePosition(pos, u.IsDash);
        player.SetQuaternion(u.Angle);
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

/// <summary>
/// キャラクター情報
/// </summary>
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
    public float Y;
    public bool IsDash;
}