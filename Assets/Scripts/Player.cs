using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    private int gameState = 0;

    private Vector3 point;
    private float time;
    
    // Start is called before the first frame update
    void Start()
    {
        SetGameState(IDLE);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("map"))
                {
                point = hit.point;
                transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
                if (Time.realtimeSinceStartup - time <= 0.2f) SetGameState(RUN);
                else SetGameState(WALK);
                time = Time.realtimeSinceStartup;
                }
            }
        }
    }

    void FixedUpdate()
    {
        switch (gameState)
        {
            case IDLE: break;
            case WALK: Move(0.1f); break;
            case RUN: Move(0.5f); break;
        }
    }

    void SetGameState(int state)
    {
        switch (state)
        {
            case IDLE: break;
            case WALK: break;
            case RUN: break;
        }
        gameState = state;
    }

    void Move(float speed) {
        if (Mathf.Abs(Vector3.Distance(point, transform.position)) >= 1.3f)
        {
            CharacterController controller = GetComponent<CharacterController>();
            Vector3 v = Vector3.ClampMagnitude(point - transform.position, speed);
            controller.Move(v);
        }
        else SetGameState(IDLE);
    }
}
