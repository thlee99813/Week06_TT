using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MonsterLineController : MonoBehaviour
{
    [Header("Row Spawn")]
    [SerializeField] private Transform rowsRoot;
    [SerializeField] private GameObject monsterRowPrefab;
    [SerializeField] private Vector3 topRowLocalPosition = new Vector3(0f, 0f, 4.7f);
    [SerializeField] private float rowGap = 2.5f;

    [SerializeField] private float rowMoveDuration = 0.2f;
    [SerializeField] private Ease rowMoveEase = Ease.OutCubic;


    [Header("Rule")]
    [SerializeField] private int maxRowsBeforeGameOver = 5;

    private readonly List<Transform> activeRows = new List<Transform>();
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

    public void OnFirePressed()
    {
        if (isGameOver) return;

        MoveAllRowsDown();
        SpawnTopRow();

        if (activeRows.Count >= maxRowsBeforeGameOver)
        {
            isGameOver = true;
            UIManager.Instance.GameOverImagePopup();
        }
    }

    private void MoveAllRowsDown()
    {
        Vector3 delta = new Vector3(0f, 0f, -rowGap);

        for (int i = 0; i < activeRows.Count; i++)
        {
            Transform row = activeRows[i];
            if (row == null) continue;

            Vector3 targetLocalPos = row.localPosition + delta;
            row.DOKill();
            row.DOLocalMove(targetLocalPos, rowMoveDuration).SetEase(rowMoveEase);
        }
    }


    private void SpawnTopRow()
    {
        if (monsterRowPrefab == null) return;
        if (rowsRoot == null) rowsRoot = transform;

        GameObject row = Instantiate(monsterRowPrefab, rowsRoot);
        Transform t = row.transform;
        t.localPosition = topRowLocalPosition;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        activeRows.Insert(0, t);
    }

    private void ClearAllRows()
    {
        for (int i = 0; i < activeRows.Count; i++)
        {
            if (activeRows[i] != null)
                Destroy(activeRows[i].gameObject);
        }
        activeRows.Clear();
    }
}
