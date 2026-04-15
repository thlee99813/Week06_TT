using UnityEngine;

public class SquadScoreSystem : MonoBehaviour
{
    [SerializeField] private SquadController squadController;
    [SerializeField] private int scorePerPattern = 1000;
    [SerializeField] private int totalScore = 0;

    public int TotalScore => totalScore;

    public int EvaluateOnFireAndLog()
    {
        if (squadController == null)
        {
            Debug.LogWarning("[Score] SquadController is null.");
            return 0;
        }

        int gained = 0;

        for (int i = 0; i < squadController.SquadCount; i++)
        {
            if (!squadController.TryGetSquadCards(i, out Card a, out Card b, out Card c))
            {
                continue;
            }

            if (IsKkeut(a, b, c))
            {
                gained += scorePerPattern;
            }
            else if (IsShuntsu(a, b, c))
            {
                gained += scorePerPattern;
            }
            else
            {
                Debug.Log($"[Score] Squad{i + 1}: 족보 없음 (0점)");
            }
        }

        totalScore += gained;
        Debug.Log($"[Score] 이번 Fire +{gained}, 누적 {totalScore}");
        return gained;
    }

    private bool IsSameColor(Card a, Card b, Card c)
    {
        return a.color == b.color && b.color == c.color;
    }

    private bool IsKkeut(Card a, Card b, Card c)
    {
        if (!IsSameColor(a, b, c)) return false;
        return a.number == b.number && b.number == c.number;
    }

    private bool IsShuntsu(Card a, Card b, Card c)
    {
        if (!IsSameColor(a, b, c)) return false;

        int min = Mathf.Min(a.number, Mathf.Min(b.number, c.number));
        int max = Mathf.Max(a.number, Mathf.Max(b.number, c.number));
        int mid = a.number + b.number + c.number - min - max;

        return (min + 1 == mid) && (mid + 1 == max);
    }
}
