using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public GameObject Player, lookplayerbox;
    public Vector3 move;
    private float time = 0;
    private bool lookplayer = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 10)
        {
            move = new Vector3(Random.Range(-100f, 100f), 2f, Random.Range(-100f, 100f));
            time = 0;
        }
        transform.position = Vector3.Lerp(transform.position, move, 0.2f * Time.deltaTime);
        transform.LookAt(new Vector3(move.x, transform.position.y, move.z));
    }


}
