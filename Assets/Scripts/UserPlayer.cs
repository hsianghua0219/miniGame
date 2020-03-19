using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserPlayer : MonoBehaviour
{
    private const int IDLE = 0, WALK = 1, RUN = 2;
    private int gameState = 0;

    private Vector3 point;
    private float time, cdtime;
    Animator animator;
    public GameObject Blood, ScoreUI , canvas;

    public int UserId;
    public bool IsDash;
    public bool IsDead => HP <= 0;

    public int HP = 100, Score;
    Quaternion targetQuaternion_;
    Vector3 movePosition_;

    public float RotationSpeed = 100.0f;
    public float WalkSpeed = 1f;
    public float DashSpeed = 3f;

    public Animator Anima;
    public int Power;

    public GameObject Bullet;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.transform.GetChild(0).gameObject.transform.GetComponent<Animator>();
        SetGameState(IDLE);
    }

    // Update is called once per frame
    void Update()
    {

        if (IsDead) return;

        Blood.GetComponent<RectTransform>().sizeDelta = new Vector2(HP, 1f);
        ScoreUI.GetComponent<Text>().text = ""+Score;
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
                if (hit.collider.CompareTag("Zombie"))
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
        if (HP < 0)
        {
            Destroy(gameObject);
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// 行く先の座標を設定する
    /// </summary>
    /// <param name="movePosition"></param>
    public void SetMovePosition(Vector3 movePosition, bool isDash)
    {
        movePosition_ = movePosition;
        IsDash = isDash;
    }

    /// <summary>
    /// 向きの設定
    /// </summary>
    /// <param name="movePosition"></param>
    public void SetQuaternion(float angle)
    {
        targetQuaternion_ = Quaternion.Euler(0, angle, 0);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScoreBox"))
        {
            Score += 1;
            Destroy(other.gameObject);
        }
    }

    public IEnumerator Dead()
    {
        while (transform.eulerAngles.z < 80)
        {
            transform.Rotate(Vector3.forward * 1.5f);
            yield return null;
        }
        Destroy(gameObject);
    }
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="damage"></param>
    public void Damage(int damage)
    {
        HP -= damage;
    }
}
