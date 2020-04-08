using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCullingGroup : MonoBehaviour
{
    private CullingGroup group = null;
    private BoundingSphere[] bounds;

    [SerializeField] Transform[] targets = null;

    float time = 1;

    void Start()
    {
        group = new CullingGroup();

        //　カリングを行うカメラを設定
        group.targetCamera = Camera.main;

        // 距離を測る用の中心となる座標と、距離のレベルを設定
        // 1:1m 2:5m 3:10m, 4:30m, 5:100m それ以上：見えない扱い
        group.SetDistanceReferencePoint(Camera.main.transform);
        group.SetBoundingDistances(new float[] { 1, 5, 10, 30, 100 });

        // 視界判定を行う一覧をセットアップ
        bounds = new BoundingSphere[targets.Length];
        for (int i = 0; i < bounds.Length; i++)
        {
            bounds[i].radius = 150f;
        }

        // 一覧への参照を登録
        group.SetBoundingSpheres(bounds);
        group.SetBoundingSphereCount(targets.Length);

        // オブジェクトの視認状態が変化した際のコールバックを登録
        group.onStateChanged = OnChange;
    }

    void Update()
    {
        if (time > 0) time -= Time.deltaTime;
        // 登録したオブジェクトの座標を更新
        if (time <= 0)
        {
            for (int i = 0; i < bounds.Length; i++)
            {
                bounds[i].position = targets[i].position;
            }
        }
    }

    void OnDestroy()
    {
        // 終了時は必ずdispose
        group.onStateChanged -= OnChange;
        group.Dispose();
        group = null;
    }

    void OnChange(CullingGroupEvent ev)
    {
        // 視界外内のオブジェクトのみアクティブにする
        targets[ev.index].gameObject.SetActive(ev.isVisible);

        // レンジが2以上の場合は非アクティブにする
        if (ev.currentDistance > 2)
        {
            targets[ev.index].gameObject.SetActive(false);
        }
    }
}
