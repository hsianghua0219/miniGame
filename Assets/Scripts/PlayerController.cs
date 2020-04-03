using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    private int gameState = 0;

    public Vector3 point;
    private float time, cdtime, at;
    public GameObject ScoreUI, Restart, miniMap;

    public UserPlayer UserPlayer { get; private set; }
    
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
        UserPlayer.Speed = 0;
        at += Time.deltaTime;

        if (time > 3 && UserPlayer != null) {
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

                if (Input.GetMouseButtonDown(1))
                {
                    UserPlayer.transform.Rotate(new Vector3(0f, 180f, 0f));
                    at = 0;
                }
                else if( at > 1 ) UserPlayer.transform.LookAt(new Vector3(point.x, UserPlayer.transform.position.y, point.z));
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


        //float t = Time.time;
        //Quaternion from_qua = Quaternion.Euler(0f, 0f, 0f);
        //Quaternion to_qua = Quaternion.Euler(0f, 180f, 0f);
        //if (Input.GetMouseButton(1)) transform.rotation = Quaternion.Slerp(from_qua, to_qua, t);
        //else transform.rotation = Quaternion.Slerp(to_qua, from_qua, t);
        
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
