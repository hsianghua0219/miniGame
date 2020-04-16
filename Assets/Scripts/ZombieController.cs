using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieController : MonoBehaviour
{
    //利用現實時間及陣列長度產生偽同步及偽隨機的座標值給予NPC在設想範圍內移動

    public List<Vector3> ZombieDatumV3; //初始座標並且作為活動範圍的定位中心
    public List<Vector3> ZombieUpdateV3; //更新用的座標
    public int ZombieMax, RoadMax, RoadLength, RoadWidth; //依序為殭屍數量,道路數量,路長,路寬
    int ZombieMaxOnRoad; //每條路的殭屍數量
    float time = 10;

    void Start()
    {
        if(ZombieMax % 6 != 0) ZombieMax = (ZombieMax / RoadMax + 1) * RoadMax; //上調到路數量的倍數
        ZombieMaxOnRoad = ZombieMax / RoadMax; //除以路數量（平均到每條路）

        for (int i = RoadMax; i > 0; i--) //每條路個別運行一次程式
        {
            float RoadV3float = RoadLength / (RoadMax / 2 - 1) * (i / 2 + i % 2) - RoadLength; //計算路所在的座標軸
            for (int j = ZombieMaxOnRoad; j > 0; j--) //每個殭屍運行一次程式
            {
                float SpacingV3float = RoadLength / ZombieMaxOnRoad * j - RoadLength / 2; //按路長平均
                if (i % 2 == 1) ZombieDatumV3.Add(new Vector3(RoadV3float, 1.5f, SpacingV3float)); //平均在道路Z軸
                else ZombieDatumV3.Add(new Vector3(SpacingV3float, 1.5f, RoadV3float)); //平均在道路X軸
            }
        }
        ZombieUpdateV3 = new List<Vector3>(ZombieDatumV3); //初始座標寫入更新座標
    }

    void Update()
    {
        float sec = System.DateTime.Now.Second; //客戶端現實時間
        time += Time.deltaTime;
        if (time > 10) //每十秒刷新一次座標
        {
            for (int i = 0; i < ZombieUpdateV3.Count; i++) //循環更新座標
            {
                switch ((i + sec) % 4) //平均朝四個方向在路寬內有序的打散座標
                {
                    case 0:
                        ZombieUpdateV3[i] = new Vector3(ZombieDatumV3[i].x + (sec * i % RoadWidth), 0f, ZombieDatumV3[i].z + (sec * i % RoadWidth));
                        break;
                    case 2:
                        ZombieUpdateV3[i] = new Vector3(ZombieDatumV3[i].x + (sec * i % RoadWidth), 0f, ZombieDatumV3[i].z - (sec * i % RoadWidth));
                        break;
                    case 1:
                        ZombieUpdateV3[i] = new Vector3(ZombieDatumV3[i].x - (sec * i % RoadWidth), 0f, ZombieDatumV3[i].z + (sec * i % RoadWidth));
                        break;
                    case 3:
                        ZombieUpdateV3[i] = new Vector3(ZombieDatumV3[i].x - (sec * i % RoadWidth), 0f, ZombieDatumV3[i].z - (sec * i % RoadWidth));
                        break;
                    default:
                        ZombieUpdateV3[i] = new Vector3(ZombieDatumV3[i].x, 0f, ZombieDatumV3[i].z);
                        break;
                }
            }
            time = 0;
        }
    }
}
