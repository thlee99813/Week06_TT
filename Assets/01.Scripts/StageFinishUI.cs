using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageFinishUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject stageFinishImage;

    [Header("Blocks")]
    [SerializeField] private TMP_Text baseNameText;
    [SerializeField] private TMP_Text baseValueText;
    [SerializeField] private TMP_Text[] yakuNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] yakuValueTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text destroyedEnemyText;

    [Header("Typewriter")]
    [SerializeField] private float totalTypingDuration = 2.5f;
    [SerializeField] private float hideDelay = 1f;

    private Coroutine playRoutine;

    private struct TextTask
    {
        public TMP_Text target;
        public string value;

        public TextTask(TMP_Text target, string value)
        {
            this.target = target;
            this.value = value;
        }
    }

    private void Awake()
    {
        if (stageFinishImage != null)
            stageFinishImage.SetActive(false);
    }

    public void Play(SquadScoreSystem scoreSystem, int destroyedCount)
    {
        if (scoreSystem == null || stageFinishImage == null) return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(CoPlay(scoreSystem, destroyedCount));
    }

    private IEnumerator CoPlay(SquadScoreSystem scoreSystem, int destroyedCount)
    {
        stageFinishImage.SetActive(true);

        List<TextTask> tasks = BuildTasks(scoreSystem, destroyedCount);

        int totalChars = 0;
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].target == null) continue;
            tasks[i].target.text = string.Empty;
            totalChars += string.IsNullOrEmpty(tasks[i].value) ? 0 : tasks[i].value.Length;
        }

        float charDelay = totalChars > 0 ? totalTypingDuration / totalChars : 0f;

        for (int i = 0; i < tasks.Count; i++)
        {
            TMP_Text target = tasks[i].target;
            string value = tasks[i].value ?? string.Empty;
            if (target == null) continue;

            for (int c = 0; c < value.Length; c++)
            {
                target.text += value[c];
                if (charDelay > 0f)
                    yield return new WaitForSeconds(charDelay);
            }
        }

        yield return new WaitForSeconds(hideDelay);
        stageFinishImage.SetActive(false);
        playRoutine = null;
    }

    private List<TextTask> BuildTasks(SquadScoreSystem scoreSystem, int destroyedCount)
    {
        List<TextTask> tasks = new List<TextTask>(16);

        tasks.Add(new TextTask(baseNameText, "멘쯔"));
        tasks.Add(new TextTask(baseValueText, $"{scoreSystem.ScorePerPattern} * {scoreSystem.LastBaseMeldCount}"));

        IReadOnlyList<SquadScoreSystem.YakuHit> hits = scoreSystem.LastYakuHits;
        int slotCount = Mathf.Min(yakuNameTexts.Length, yakuValueTexts.Length);

        for (int i = 0; i < slotCount; i++)
        {
            bool has = i < hits.Count;

            string name = has ? hits[i].name : string.Empty;
            string value = has ? $"X {hits[i].addMultiplier + 1:0.0#}" : string.Empty;

            tasks.Add(new TextTask(yakuNameTexts[i], name));
            tasks.Add(new TextTask(yakuValueTexts[i], value));
        }

        tasks.Add(new TextTask(finalScoreText, scoreSystem.LastFireScore.ToString()));
        tasks.Add(new TextTask(destroyedEnemyText, $"파괴된 적 : {destroyedCount}"));

        return tasks;
    }
}
