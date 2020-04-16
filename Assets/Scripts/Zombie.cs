using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public GameObject Player, Blood, BloodUI, canvas, ScoreBox, ZombieController;
    private Vector3 v3s, v3e;
    private float v3m, time = 10;
    private bool lookplayer = false;
    public int Id, HP = 100;
    public float moveX, moveZ;
    Animator Anima;
    readonly int frameCount_ = 0;

    // Start is called before the first frame update
    void Start()
    {
        ZombieController = GameObject.Find("GameEngine");
        Anima = gameObject.transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HP = Mathf.Clamp(HP, 0, 100);
        canvas.transform.rotation = Camera.main.transform.rotation;
        BloodUI.GetComponent<RectTransform>().sizeDelta = new Vector2(HP, 1f);
        if (HP <= 0)
        {
            GameObject Clone = Object.Instantiate(ScoreBox) as GameObject;
            Clone.transform.Translate(transform.position);
            UpdateZombie();
            GameEngine.Instance.zombieList_.Remove(gameObject.GetComponent<Zombie>());
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        ZombieController zc = ZombieController.gameObject.GetComponent<ZombieController>();
        if (Id < zc.ZombieUpdateV3.Count) {
            moveX = zc.ZombieUpdateV3[Id].x;
            moveZ = zc.ZombieUpdateV3[Id].z;
        } else if (Id >= zc.ZombieUpdateV3.Count) {
            moveX = zc.ZombieUpdateV3[Id % zc.ZombieMax].x;
            moveZ = zc.ZombieUpdateV3[Id % zc.ZombieMax].z;
        }

        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        time += Time.deltaTime;
        if (time > 10)
        {
            if(lookplayer) Anima.Play("ZombieIDE");
            lookplayer = false;
            time = 0;
        }
        v3s = transform.position;
        v3e += new Vector3(moveX, 0f, moveZ);
        v3m = Vector3.Distance(v3s, v3e);

        if (lookplayer && Player != null)
        {
            transform.position = Vector3.Lerp(transform.position, Player.transform.position, (Time.deltaTime * 2f));
            transform.LookAt(new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z));
            UpdateZombie();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, v3e, (Time.deltaTime * 4f) / v3m);
            transform.LookAt(new Vector3(v3e.x, transform.position.y, v3e.z));
        }
    }
    void UpdateZombie()
    {
        if (frameCount_ % 30 == 0 || HP <= 0)
        {
            var msg = new UpdateZombieMessage();
            var c = GameEngine.Instance.FindZombieData(Id);
            c.X = transform.position.x;
            c.Z = transform.position.z;
            c.HP = HP;
            msg.zombie = c;
            GameEngine.Instance.Send(Message.ZombieLockPlayer, msg);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) {
            Player = other.gameObject;
            lookplayer = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Anima.Play("ZombieAttack");

        if (other.CompareTag("MapBox")) gameObject.transform.parent = other.transform;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Border")) HP = 0;
        
        if (collision.gameObject.CompareTag("Player") && HP>0)
        {
            collision.gameObject.GetComponent<UserPlayer>().HP -= 1;
            if (collision.gameObject == Camera.main.GetComponent<PlayerCamera>().Player) Camera.main.GetComponent<Animator>().Play("CameraShake");
            if (collision.gameObject.GetComponent<UserPlayer>().HP > 0)
            {
                GameObject Clone = Object.Instantiate(Blood) as GameObject;
                Clone.transform.Translate(Player.transform.position);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Weapon"))
        {
            HP -= 35;
            Anima.Play("ZombieRepel");
        }
    }
}
