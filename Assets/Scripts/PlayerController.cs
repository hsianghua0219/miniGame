using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    private int gameState = 0;

    private Vector3 point;
    private float time, cdtime;
    public GameObject ScoreUI, Restart, miniMap;

    public UserPlayer UserPlayer { get; private set; }

    public float Vx, Vz;

    public void Init(UserPlayer player)
    {
        UserPlayer = player;
    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        SetGameState(IDLE);
    }

    void Update()
    {
        if(time > 3 && UserPlayer != null) {
            Vector3 player = UserPlayer.transform.position;
            player.y = 800f;
            miniMap.transform.position = player;
        }

        if (UserPlayer == null) return;

        if (UserPlayer.HP <= 0)
        {
            Restart.SetActive(true);
            return;
        }

        ScoreUI.GetComponent<Text>().text = "" + UserPlayer.Score;
        cdtime += Time.deltaTime;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("map"))
            {
                point = hit.point;
                UserPlayer.transform.LookAt(new Vector3(point.x, UserPlayer.transform.position.y, point.z));
                if (Time.realtimeSinceStartup - time <= 0.2f) SetGameState(RUN);
                else SetGameState(WALK);
                time = Time.realtimeSinceStartup;
            }
            if (hit.collider.CompareTag("Zombie")&&hit.collider.CompareTag("Player"))
            {
                point = hit.point;
                point.y = 0f;
                SetGameState(RUN);
                time = Time.realtimeSinceStartup;
            }
        }
        if (Input.GetMouseButtonDown(0) && cdtime > 0.45)
        {
            UserPlayer.Anima.Play("Standby");
        }
        if (Input.GetMouseButtonUp(0) && cdtime > 0.15)
        {
            UserPlayer.Weapon.AddComponent<Rigidbody>();
            Rigidbody rigidbody = UserPlayer.Weapon.gameObject.GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            UserPlayer.Anima.Play("Attack");
            cdtime = 0;
        }
        if (cdtime > 0.45)
        {
            Rigidbody rigidbody = UserPlayer.Weapon.gameObject.GetComponent<Rigidbody>();
            Destroy(rigidbody);
        }

        switch (gameState)
        {
            case IDLE: break;
            case WALK: Move(10f); break;
            case RUN: Move(20f); break;
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

    void Move(float speed)
    {
        if (Mathf.Abs(Vector3.Distance(point, UserPlayer.transform.position)) >= 1.3f)
        {
            CharacterController controller = UserPlayer.GetComponent<CharacterController>();
            Vector3 v = Vector3.ClampMagnitude(point - UserPlayer.transform.position, speed*Time.deltaTime);
            controller.Move(v);
        }
        else SetGameState(IDLE);
    }
}
