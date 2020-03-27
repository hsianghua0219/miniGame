using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public GameObject Player, Blood;
    private Vector3 v3s, v3e;
    private float v3m, time = 10;
    private bool lookplayer = false;
    private bool stop = false;
    public int HP = 100,Id;
    public float X, Z;

    int frameCount_;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        time += Time.deltaTime;
        if (time > 10)
        {
            lookplayer = false;
            time = 0;
        }
        v3s = transform.position;
        v3e = new Vector3(X, 2f, Z);
        v3m = Vector3.Distance(v3s, v3e);

        if (Player == null) stop = false;

        if (stop) return;

        if (lookplayer)
        {
            transform.position = Vector3.Lerp(transform.position, Player.transform.position, (Time.deltaTime * 1.5f));
            transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
            transform.LookAt(new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z));
            GameEngine.Instance.Send(Message.ZombieLockPlayer, new UpdateZombieMessage { LockPlayer = Player });
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, v3e, (Time.deltaTime * 2f) / v3m);
            transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
            transform.LookAt(new Vector3(v3e.x, transform.position.y, v3e.z));
            GameEngine.Instance.Send(Message.ZombieLockPlayer, new UpdateZombieMessage { LockPlayer = null });
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) {
            Player = other.gameObject;
            lookplayer = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) stop = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<UserPlayer>().HP -= 1;
            GameObject Clone = Object.Instantiate(Blood) as GameObject;
            Clone.transform.Translate(Player.transform.position);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) stop = false;
    }
}
