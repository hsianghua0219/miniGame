using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Niwatori : MonoBehaviour
{
    public float RotationSpeed = 100.0f;
    public float WalkSpeed = 1f;
    public float DashSpeed = 3f;

    public Animator Anima;

    public int UserId;
    public int Hp = 0;
    public int Power;
    public bool IsDash;

    public GameObject Bullet;

    Quaternion targetQuaternion_;
    Vector3 movePosition_;

    /// <summary>
    /// 死亡したか？
    /// </summary>
    public bool IsDead => Hp <= 0;

    void Awake()
    {
        //Bullet.SetActive(false);
    }

    void OnEnable()
    {
        //Anima.SetBool("Walk", true);
        //Anima.SetBool("Run", false);
    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        // 向き変更
        if (transform.rotation.y != targetQuaternion_.y)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion_, RotationSpeed * Time.deltaTime);
        }

        var distance = Vector3.Distance(transform.position, movePosition_);
        if (distance > 0.1f)
        {
            // 1.5f以上の距離があるとダッシュに変化する
            //Anima.SetBool("Walk", !IsDash);
           // Anima.SetBool("Run", IsDash);
            // 移動処理
            var speed = IsDash ? DashSpeed : WalkSpeed;
            transform.position = Vector3.MoveTowards(transform.position, movePosition_, speed * Time.deltaTime);
        }
        else
        {
            //Anima.SetBool("Walk", false);
            //Anima.SetBool("Run", false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Bullet"))
        {
            GameEngine.Instance.Send(Message.ActionDamge, new ActionDamageMessage { UserId = UserId, Damage = 1 });
            Destroy(collision.gameObject);
        }
    }

    /// <summary>
    /// 行く先の座標を設定する
    /// </summary>
    /// <param name="movePosition"></param>
    public void SetMovePosition(Vector3 movePosition, bool isDash)
    {
        movePosition_ = movePosition;
        IsDash = isDash;
    }

    /// <summary>
    /// 向きの設定
    /// </summary>
    /// <param name="movePosition"></param>
    public void SetQuaternion(float angle)
    {
        targetQuaternion_ = Quaternion.Euler(0, angle, 0);
    }

    /// <summary>
    /// 弾を撃つ
    /// </summary>
    public void Shot()
    {
        var bullet = Instantiate(Bullet);
        bullet.SetActive(true);
        bullet.transform.position = transform.position + transform.TransformDirection(Vector3.forward) + new Vector3(0, 0.25f, 0);
        bullet.transform.rotation = transform.rotation;
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="damage"></param>
    public void Damage(int damage)
    {
        Hp -= damage;
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public IEnumerator Dead()
    {
        while (transform.eulerAngles.z < 80)
        {
            transform.Rotate(Vector3.forward*1.5f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
