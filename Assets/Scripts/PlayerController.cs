using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float ShotCoolTime = 5;
    float lastShotTime_;

    /// <summary>
    /// 移動可能な制限距離
    /// </summary>
    public float MoveDistance = 19;

    public UserPlayer UserPlayer { get; private set; }

    public void Init(UserPlayer player)
    {
        UserPlayer = player;
    }

    void Update()
    {
        if (UserPlayer == null)
        {
            return;
        }

        var angle = 0f;
        if (Input.GetKey(KeyCode.D))
        {
            angle = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            angle = -1;
        }
        if (angle != 0)
        {
            UserPlayer.SetQuaternion(UserPlayer.transform.eulerAngles.y + angle);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            GameEngine.Instance.Send(Message.ActionDamge, new ActionDamageMessage { UserId = UserPlayer.UserId, Damage = 1 });
        }
        if (Input.GetKeyDown(KeyCode.Space) && lastShotTime_ + ShotCoolTime < Time.time)
        {
            lastShotTime_ = Time.time;
            GameEngine.Instance.Send(Message.ActionShot, new ActionShotMessage { UserId = UserPlayer.UserId });
        }

        var move = UserPlayer.transform.TransformDirection(Vector3.forward);
        if (Vector3.Distance(Vector3.zero, UserPlayer.transform.position + move) < MoveDistance)
        {
            UserPlayer.SetMovePosition(UserPlayer.transform.position + move, Input.GetKey(KeyCode.W));
        }
    }
}
