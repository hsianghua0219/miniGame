using System.Collections;
using UnityEngine;

public class UserPlayer : MonoBehaviour
{
    public int UserId;
    public bool IsDash, updata;
    public bool IsDead => HP <= 0;

    public int HP = 100;
    Quaternion targetQuaternion_;
    Vector3 movePosition_;

    public Vector3 nowV3, point;
    
    public GameObject Blood, canvas, Weapon, ScoreBox;
    public int Score;

    public float Speed=20;
    float hpAdd;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Speed > 0) Move(Speed);

        hpAdd += Time.deltaTime;
        HP = Mathf.Clamp(HP, 0, 100);
        if (HP < 100 && hpAdd > 0.2)
        {
            HP++;
            hpAdd = 0;
        }

        if (IsDead)
        {
            for (int i = Score; i >= 0; i--)
            {
                GameObject Clone = Object.Instantiate(ScoreBox) as GameObject;
                Clone.transform.Translate(transform.position);
            }
            GameEngine.Instance.Send(Message.ActionDamge, new ActionDamageMessage { UserId = UserId, Damage = 1 });
            Destroy(gameObject);
        }
        Blood.GetComponent<RectTransform>().sizeDelta = new Vector2(HP, 1f);
        //canvas.transform.LookAt(Camera.main.transform);
        canvas.transform.rotation = Camera.main.transform.rotation;
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

    void Move(float speed)
    {
        if (updata) {
            transform.position = nowV3;
            updata = false;
        }
        else if (Mathf.Abs(Vector3.Distance(point, transform.position)) >= 1.3f)
        {
            CharacterController controller = GetComponent<CharacterController>();
            Vector3 v = Vector3.ClampMagnitude(point - nowV3, speed * Time.deltaTime);
            controller.Move(v);
        }
    }
}
