using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject Player;
    public float distance = 20.0f, height = 15.0f;
    int frameCount_;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (!Player) return;
        transform.position = Player.transform.position;
        transform.position -= Vector3.forward * distance;
        transform.position = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        if (frameCount_ % 3 == 0) transform.LookAt(Player.transform);
        frameCount_++;
    }

}