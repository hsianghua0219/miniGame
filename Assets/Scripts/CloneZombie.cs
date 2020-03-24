using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneZombie : MonoBehaviour
{
    private float time = 3;
    private int MaxZombie;
    public GameObject Zombie;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (MaxZombie < 10)
        {
            GameObject Clone = Object.Instantiate(Zombie) as GameObject;
            Clone.transform.Translate(transform.position.x + Random.Range(-50f,50f), 2f, transform.position.z + Random.Range(-50f, 50f));
            time = 0;
            MaxZombie++;
        }
    }
}
