using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiarySpawnManager : MonoBehaviour
{
    [Header("生成配置")]
    public GameObject diaryPrefab;            // 日记预制体
    public int diaryCount = 3;                // 要生成的日记数量
    private float checkRadius = 0.5f;         // 检测生成位置是否合法的半径
    public LayerMask groundLayer;             // 地面层（避免生成在空中）
    public LayerMask obstacleLayer;           // 障碍物层（避免生成在墙里）
    public List<Transform> spawnAreas;        // 可生成区域（手动拖入场景中的 SpawnArea）
    private Coroutine checkCoroutine;         // 判断是否有掉出场景的日记协程
    private List<GameObject> spawns = new (); // 已生成的预制体（避免重叠）

    private void Awake()
    {
        // 默认关闭
        GetComponent<DiarySpawnManager>().enabled = false;
    }

    private void Start()
    {
        // 游戏启动时生成日记
        // 校验配置（避免空引用）
        if (diaryPrefab == null || spawnAreas.Count == 0)
        {
            Debug.LogError("日记预制体或生成区域未配置！");
            return;
        }
        // 循环生成指定数量的日记
        for (int i = 0; i < diaryCount; i++) { SpawnDiary(); }
        // 清除异常位置日记，重新生成
        checkCoroutine = StartCoroutine(CheckSpawnPos());
    }

    private void OnDestroy()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }

    private IEnumerator CheckSpawnPos()
    {
        // 等待 3 s 开始检测日记有没有掉出场景
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < spawns.Count; i++)
        {
            if (!IsOnGround(spawns[i].transform.position))
            {
                print($"{spawns[i].gameObject.name}位置异常");
                Destroy(spawns[i].gameObject); // 销毁原来的异常日记
                SpawnDiary();
            }
        }
    }

    /// <summary>
    /// 生成日记
    /// </summary>
    private void SpawnDiary()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            // 生成日记并设置位置
            GameObject diary = ResMgr.Instance.load<GameObject>("Model/itemModel/Diary");
            diary.gameObject.name = "Diary";
            diary.transform.position = spawnPosition;
            diary.transform.rotation = Quaternion.Euler(-90, 0, 0);
            spawns.Add(diary);
        }
        else { Debug.LogWarning("有日记生成失败：无合法位置"); }
    }


    /// <summary>
    /// 获取合法的生成位置（在生成区域内、在地面上、不重叠、不穿墙）
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        int maxTryCount = 50; // 最大尝试次数（避免死循环）
        for (int i = 0; i < maxTryCount; i++)
        {
            Transform randomArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
            // 获取区域尺寸（如果有 BoxCollider使用它的尺寸，否则使用 localScale）
            Vector3 areaSize = Vector3.one;
            BoxCollider boxCollider = randomArea.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                areaSize = Vector3.Scale(boxCollider.size, randomArea.lossyScale);
            }
            else { areaSize = randomArea.lossyScale; }
        
            // 计算随机位置
            Vector3 randomLocalPos = new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                0,
                Random.Range(-areaSize.z / 2, areaSize.z / 2)
            );
            Vector3 worldPos = randomArea.position + randomLocalPos;
        
            // 调整Y轴为区域的位置
            worldPos.y = randomArea.position.y;

            // 检测是否在地面上（向下射线检测）
            if (!IsOnGround(worldPos)) continue;
            // 检测是否与已生成的日记重叠
            if (IsOverlapWithSpawned(worldPos)) continue;
            // 检测是否在障碍物内（胶囊体检测）
            if (IsInObstacle(worldPos)) continue;

            // 所有检测通过，返回合法位置
            return worldPos;
        }
        // 尝试次数用尽，返回无效位置
        return Vector3.zero;
    }

    /// <summary>
    /// 检测位置是否在地面上
    /// </summary>
    private bool IsOnGround(Vector3 pos)
    {
        // 向下发射射线，检测是否命中地面
        if (Physics.Raycast(pos + Vector3.up*2, Vector3.down, out RaycastHit hit, 5, groundLayer))
        {
            // 日记生成在地面上（微调Y轴，避免嵌入地面）
            pos.y = hit.point.y + 0.3f;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检测位置是否与已生成的日记重叠
    /// </summary>
    private bool IsOverlapWithSpawned(Vector3 pos)
    {
        foreach (var spawnedPos in spawns)
        {
            if (Vector3.Distance(pos, spawnedPos.transform.position) < checkRadius * 2)
            {
                return true; // 距离过近，重叠
            }
        }
        return false;
    }

    /// <summary>
    /// 检测位置是否在障碍物内（墙、柜子等）
    /// </summary>
    private bool IsInObstacle(Vector3 pos)
    {
        // 用胶囊体检测是否与障碍物重叠
        return Physics.CheckCapsule(
            pos + Vector3.up * 0.1f,
            pos + Vector3.up * 0.5f,
            checkRadius,
            obstacleLayer
        );
    }
    
}
