using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    public int gameState = 0;

    public Vector3 point;
    private float time, cdtime, at;
    public GameObject ScoreUI, Restart, miniMap, rufu;

    public UserPlayer UserPlayer { get; private set; }
    
    public void Init(UserPlayer player)
    {
        UserPlayer = player;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        SetGameState(IDLE);
    }

    void Update()
    {
        if (UserPlayer.HP <= 0　|| UserPlayer == null)
        {
            Restart.SetActive(true);
            return;
        }

        if (at <= 10)
        {
            rufu.GetComponent<RectTransform>().sizeDelta = new Vector2(at, 15f);
            rufu.GetComponent<Image>().color = new Color32(255, 183, 2, 255);
        }
        else rufu.GetComponent<Image>().color = new Color32(255, 0, 0, 255);

        UserPlayer.Speed = 0;
        at += Time.deltaTime;

        if (time > 3) {
            Vector3 player = UserPlayer.transform.position;
            player.y = 10f;
            miniMap.transform.position = player;
        }

        ScoreUI.GetComponent<Text>().text = "" + UserPlayer.Score;
        cdtime += Time.deltaTime;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            point = hit.point;
            point.y = 0f;
            if (Input.GetMouseButtonDown(1) && at > 10)
            {
                UserPlayer.Weapon.AddComponent<Rigidbody>();
                Rigidbody rigidbody = UserPlayer.Weapon.gameObject.GetComponent<Rigidbody>();
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                UserPlayer.GetComponent<Animator>().Play("Skill");
                at = 0;
                SetGameState(IDLE);
            }
            else if (at > 1)
            {
                UserPlayer.transform.LookAt(new Vector3(point.x, UserPlayer.transform.position.y, point.z));
                SetGameState(RUN);
            }
            time = Time.realtimeSinceStartup;
        }
        if (Input.GetMouseButtonDown(0) && cdtime > 0.45)
        {
            UserPlayer.GetComponent<Animator>().Play("Standby");
        }
        if (Input.GetMouseButtonUp(0) && cdtime > 0.15)
        {
            SetGameState(IDLE);
            UserPlayer.Weapon.AddComponent<Rigidbody>();
            Rigidbody rigidbody = UserPlayer.Weapon.gameObject.GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            UserPlayer.GetComponent<Animator>().Play("Attack");
            cdtime = 0;
        }
        if (cdtime > 1 && at > 1)
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
