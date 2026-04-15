using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MonsterLineController : MonoBehaviour
{
    [System.Serializable]
    private class EnemyRowData
    {
        public Transform rowRoot;
        public List<GameObject> aliveBlocks = new List<GameObject>();
    }

    [Header("Row Spawn")]
    [SerializeField] private Transform rowsRoot;
    [SerializeField] private GameObject monsterRowPrefab;
    [SerializeField] private Vector3 topRowLocalPosition = new Vector3(0f, 0f, 4.7f);
    [SerializeField] private float rowGap = 2.5f;

    [Header("Row Move Tween")]
    [SerializeField] private float rowMoveDuration = 0.2f;
    [SerializeField] private Ease rowMoveEase = Ease.OutCubic;

    [Header("Enemy HP Rule")]
    [SerializeField, Min(1)] private int scorePerEnemyHp = 1000;

    [Header("Game Over Rule")]
    [SerializeField] private int maxRowsBeforeGameOver = 5;

    private readonly List<EnemyRowData> activeRows = new List<EnemyRowData>();
    private bool isGameOver;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        ClearAllRows();
        isGameOver = false;
        SpawnTopRow();
    }

    // DeckController에서 Fire 시 호출
    public void OnFirePressed(int gainedScore)
    {
        if (isGameOver) return;

        MoveAllRowsDown();
        SpawnTopRow();
        ApplyDamageByScore(gainedScore); // 점수 데미지
        CheckGameOver();
    }

    public void OnFirePressed()
    {
        OnFirePressed(0);
    }

    public void ApplyDamageByScore(int gainedScore)
    {
        int destroyCount = Mathf.Max(0, gainedScore / scorePerEnemyHp);
        if (destroyCount <= 0) return;

        for (int i = 0; i < destroyCount; i++)
        {
            if (!TryDestroyOneBottomBlock())
                break;
        }
    }

    private bool TryDestroyOneBottomBlock()
    {
        for (int rowIndex = activeRows.Count - 1; rowIndex >= 0; rowIndex--)
        {
            EnemyRowData row = activeRows[rowIndex];
            if (row == null || row.rowRoot == null)
            {
                activeRows.RemoveAt(rowIndex);
                continue;
            }

            row.aliveBlocks.RemoveAll(x => x == null);

            if (row.aliveBlocks.Count == 0)
            {
                RemoveRow(rowIndex);
                continue;
            }

            int blockIndex = row.aliveBlocks.Count - 1;
            GameObject target = row.aliveBlocks[blockIndex];
            row.aliveBlocks.RemoveAt(blockIndex);

            if (target != null)
                Destroy(target);

            if (row.aliveBlocks.Count == 0)
                RemoveRow(rowIndex); // 한 줄 10개 다 깨졌을때 줄 제거

            return true;
        }

        return false;
    }

    private void MoveAllRowsDown()
    {
        Vector3 delta = new Vector3(0f, 0f, -rowGap);

        for (int i = 0; i < activeRows.Count; i++)
        {
            EnemyRowData row = activeRows[i];
            if (row == null || row.rowRoot == null) continue;

            Vector3 targetLocalPos = row.rowRoot.localPosition + delta;
            row.rowRoot.DOKill();
            row.rowRoot
                .DOLocalMove(targetLocalPos, rowMoveDuration)
                .SetEase(rowMoveEase)
                .SetLink(row.rowRoot.gameObject, LinkBehaviour.KillOnDestroy);

        }
    }

    private void SpawnTopRow()
    {
        if (monsterRowPrefab == null) return;
        if (rowsRoot == null) rowsRoot = transform;

        GameObject rowObj = Instantiate(monsterRowPrefab, rowsRoot);
        Transform t = rowObj.transform;
        t.localPosition = topRowLocalPosition;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        EnemyRowData rowData = new EnemyRowData { rowRoot = t };
        CacheEnemyBlocks(rowData);

        activeRows.Insert(0, rowData);
    }

    private void CacheEnemyBlocks(EnemyRowData rowData)
    {
        rowData.aliveBlocks.Clear();

        Transform root = rowData.rowRoot;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child != null)
                rowData.aliveBlocks.Add(child.gameObject);
        }
    }

    private void CheckGameOver()
    {
        if (activeRows.Count < maxRowsBeforeGameOver) return;

        isGameOver = true;

        if (UIManager.Instance != null)
            UIManager.Instance.GameOverImagePopup();
    }

    private void RemoveRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= activeRows.Count) return;

        EnemyRowData row = activeRows[rowIndex];
        if (row != null && row.rowRoot != null)
        {
            row.rowRoot.DOKill();
            Destroy(row.rowRoot.gameObject);
        }
        activeRows.RemoveAt(rowIndex);
    }

    private void ClearAllRows()
    {
        for (int i = 0; i < activeRows.Count; i++)
        {
            EnemyRowData row = activeRows[i];
            if (row != null && row.rowRoot != null)
            {
                row.rowRoot.DOKill();
                Destroy(row.rowRoot.gameObject);
            }

        }

        activeRows.Clear();
    }
}
