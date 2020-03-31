using System.Collections;
using UnityEngine;

public class UserPlayer : MonoBehaviour
{
    public int UserId;
    public bool IsDash;
    public bool IsDead => HP <= 0;

    public int HP = 100;
    Quaternion targetQuaternion_;
    Vector3 movePosition_;

    public Vector3 nowV3;

    public Animator Anima;

    public GameObject Blood, canvas, Weapon;
    public int Score;

    public float Vx, Vz;

    // Start is called before the first frame update
    void Start()
    {
        Anima = gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HP = Mathf.Clamp(HP, 0, 100);
        if (IsDead)
        {
            GameEngine.Instance.Send(Message.ActionDamge, new ActionDamageMessage { UserId = UserId, Damage = 1 });
        }
        Blood.GetComponent<RectTransform>().sizeDelta = new Vector2(HP, 1f);
        canvas.transform.LookAt(Camera.main.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScoreBox"))
        {
            Score += 1;
            Destroy(other.gameObject);
        }
    }

    public IEnumerator Dead()
    {
        while (transform.eulerAngles.z < 80)
        {
            transform.Rotate(Vector3.forward * 1.5f);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Damage(int damage)
    {
        HP -= damage;
    }
}
