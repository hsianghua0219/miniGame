using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLight : MonoBehaviour
{
    float time;
    int mod;

    // Start is called before the first frame update
    void Start()
    {
        time = Random.Range(0, 2f);
        mod = Random.Range(0, 3);
    }

    // Update is called once per frame
    void Update()
    {

        time += Time.deltaTime;
        if (time > 1f)
        {
            switch (mod)
            {
                case 0:
                    gameObject.GetComponent<Light>().intensity = Random.Range(2.5f, 3f);
                    time = 0f;
                    break;
                case 1:
                    gameObject.GetComponent<Light>().intensity = Random.Range(0, 1f);
                    time = 0.8f;
                    break;
                case 2:
                    if (time < 1.1f) gameObject.GetComponent<Light>().intensity = Random.Range(0, 3f);
                    break;
                case 3:
                    gameObject.GetComponent<Light>().intensity = 0;;
                    break;
            }
        }
    }
}
