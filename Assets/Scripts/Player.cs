using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    private int gameState = 0;

    private Vector3 point;
    private float time, cdtime;
    Animator animator;

    public int HP = 100, Score;
    public GameObject Blood, canvas;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Animator>();
        SetGameState(IDLE);
    }

    // Update is called once per frame
    void Update()
    {
        Blood.GetComponent<RectTransform>().sizeDelta = new Vector2(HP, 1f);
        canvas.transform.LookAt(Camera.main.transform);
        cdtime += Time.deltaTime;
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
                if (!hit.collider.CompareTag("map") && !hit.collider.CompareTag("Player"))
                {
                    point = hit.point;
                    point.y = 0f;
                    SetGameState(RUN);
                    time = Time.realtimeSinceStartup;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && cdtime>2)
        {
            animator.Play("Attack");
            cdtime = 0;
        }
        if (HP < 0) SceneManager.LoadScene(0);
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
