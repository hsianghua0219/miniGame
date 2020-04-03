using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{


    public Vector3 positionShake;//震動幅度
    public Vector3 angleShake;   //震動角度
    public float cycleTime = 0.2f;//震動週期
    public int cycleCount = 6;    //震動次數
    public bool fixShake = false; //為真時每次幅度相同，反之則遞減
    public bool unscaleTime = false;//不考慮縮放時間
    public bool bothDir = true;//雙向震動
    public float fCycleCount = 0;//設定此引數，以此震動次數為主
    public bool autoDisable = true;//自動disbale


    float currentTime;
    int curCycle;
    Vector3 curPositonShake;
    Vector3 curAngleShake;
    float curFovShake;
    Vector3 startPosition;
    Vector3 startAngles;
    Transform myTransform;

    float frameCount_;
    public Transform Player;
    public float distance = 10.0f, height = 7.0f;

    void OnEnable()
    {
        currentTime = 0f;
        curCycle = 0;
        curPositonShake = positionShake;
        curAngleShake = angleShake;
        myTransform = transform;
        startPosition = myTransform.localPosition;
        startAngles = myTransform.localEulerAngles;
        if (fCycleCount > 0)
            cycleCount = Mathf.RoundToInt(fCycleCount);
    }

    void OnDisable()
    {
        myTransform.localPosition = startPosition;
        myTransform.localEulerAngles = startAngles;
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        if (fCycleCount > 0)
            cycleCount = Mathf.RoundToInt(fCycleCount);
#endif

        if (curCycle >= cycleCount)
        {
            if (autoDisable)
                enabled = false;
            return;
        }

        float deltaTime = unscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
        currentTime += deltaTime;
        while (currentTime >= cycleTime)
        {
            currentTime -= cycleTime;
            curCycle++;
            if (curCycle >= cycleCount)
            {
                myTransform.localPosition = startPosition;
                myTransform.localEulerAngles = startAngles;
                return;
            }

            if (!fixShake)
            {
                if (positionShake != Vector3.zero)
                    curPositonShake = (cycleCount - curCycle) * positionShake / cycleCount;
                if (angleShake != Vector3.zero)
                    curAngleShake = (cycleCount - curCycle) * angleShake / cycleCount;
            }
            
        }

        if (curCycle < cycleCount)
        {
            float offsetScale = Mathf.Sin((bothDir ? 2 : 1) * Mathf.PI * currentTime / cycleTime);
            if (positionShake != Vector3.zero)
                myTransform.localPosition = startPosition + curPositonShake * offsetScale;
            if (angleShake != Vector3.zero)
                myTransform.localEulerAngles = startAngles + curAngleShake * offsetScale;
        }

        if (frameCount_ % 1 == 0)
        {
            if (!Player) return;
            transform.position = Player.position;
            transform.position -= Vector3.forward * distance;
            transform.position = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
            transform.LookAt(Player);
        }
        frameCount_++;
    }
    
    //重置
    public void Restart()
    {
        if (enabled)
        {
            currentTime = 0f;
            curCycle = 0;
            curPositonShake = positionShake;
            curAngleShake = angleShake;
            myTransform.localPosition = startPosition;
            myTransform.localEulerAngles = startAngles;
            if (fCycleCount > 0)
                cycleCount = Mathf.RoundToInt(fCycleCount);
        }
        else
            enabled = true;
    }
}