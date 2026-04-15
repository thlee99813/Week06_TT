using System.Collections.Generic;
using UnityEngine;

public class SquadScoreSystem : MonoBehaviour
{
    private enum MeldType
    {
        None,
        Shuntsu,
        Kkeut
    }

    private struct MeldInfo
    {
        public bool hasThreeCards;
        public MeldType meldType;
        public CardColor color;
        public int runStart;
        public int tripletNumber;
    }

    [SerializeField] private SquadController squadController;
    [SerializeField, Min(0)] private int scorePerPattern = 1000;
    [SerializeField] private int totalScore = 0;

    public int TotalScore => totalScore;

    [System.Serializable]
    public struct YakuHit
    {
        public string name;
        public float addMultiplier;

        public YakuHit(string name, float addMultiplier)
        {
            this.name = name;
            this.addMultiplier = addMultiplier;
        }
    }

    private readonly List<YakuHit> lastYakuHits = new List<YakuHit>();

    public IReadOnlyList<YakuHit> LastYakuHits => lastYakuHits;
    public int LastBaseMeldCount { get; private set; }
    public int LastBaseScore { get; private set; }
    public float LastMultiplier { get; private set; } = 1f;
    public int LastFireScore { get; private set; }
    public int ScorePerPattern => scorePerPattern;

    public int EvaluateOnFireAndLog()
    {
        return EvaluateOnFire();
    }

    public int EvaluateOnFire()
    {
        lastYakuHits.Clear();
        LastBaseMeldCount = 0;
        LastBaseScore = 0;
        LastMultiplier = 1f;
        LastFireScore = 0;

        if (squadController == null) return 0;

        int squadCount = squadController.SquadCount;
        MeldInfo[] meldInfos = new MeldInfo[squadCount];
        List<Card> allCards = new List<Card>(squadCount * 3);

        int baseScore = 0;
        int baseMeldCount = 0;

        for (int i = 0; i < squadCount; i++)
        {
            if (!squadController.TryGetSquadCards(i, out Card a, out Card b, out Card c))
            {
                meldInfos[i] = new MeldInfo { hasThreeCards = false, meldType = MeldType.None };
                continue;
            }

            allCards.Add(a);
            allCards.Add(b);
            allCards.Add(c);

            MeldInfo info = AnalyzeMeld(a, b, c);
            info.hasThreeCards = true;
            meldInfos[i] = info;

            if (info.meldType == MeldType.Kkeut || info.meldType == MeldType.Shuntsu)
            {
                baseScore += scorePerPattern;
                baseMeldCount++;
            }
        }

        float multiplier = 1.0f;

        bool hasAllNineCards = (squadCount == 3 && allCards.Count == 9);
        bool allSquadsAreValidMeld = AreAllSquadsValidMeld(meldInfos);
        bool canApplyYaku = hasAllNineCards && allSquadsAreValidMeld;

        if (canApplyYaku)
        {
            ApplyYaku(IsTanyao(allCards), "탕야오", 0.3f, ref multiplier);
            ApplyYaku(IsIipeko(meldInfos), "이페코", 0.7f, ref multiplier);
            ApplyYaku(IsToitoi(meldInfos), "또이또이", 1.0f, ref multiplier);
            ApplyYaku(IsSanshokuDoujun(meldInfos), "삼색동순", 1.5f, ref multiplier);
            ApplyYaku(IsChinitsu(allCards), "청일색", 2.0f, ref multiplier);
            ApplyYaku(IsIkkitsukan(allCards), "일기통관", 2.5f, ref multiplier);
            ApplyYaku(IsRyanpekoCustom(meldInfos), "량페코", 3.3f, ref multiplier);
            ApplyYaku(IsSanshokuDoukou(meldInfos), "삼색동각", 4.0f, ref multiplier);
            ApplyYaku(IsChinroutou(meldInfos, allCards), "청노두", 4.2f, ref multiplier);
        }

        int fireScore = Mathf.RoundToInt(baseScore * multiplier);

        LastBaseMeldCount = baseMeldCount;
        LastBaseScore = baseScore;
        LastMultiplier = multiplier;
        LastFireScore = fireScore;

        totalScore += fireScore;
        return fireScore;
    }

    private MeldInfo AnalyzeMeld(Card a, Card b, Card c)
    {
        MeldInfo info = new MeldInfo
        {
            meldType = MeldType.None,
            color = a.color,
            runStart = -1,
            tripletNumber = -1
        };

        bool sameColor = (a.color == b.color && b.color == c.color);
        if (!sameColor)
            return info;

        info.color = a.color;

        if (a.number == b.number && b.number == c.number)
        {
            info.meldType = MeldType.Kkeut;
            info.tripletNumber = a.number;
            return info;
        }

        int n1 = a.number;
        int n2 = b.number;
        int n3 = c.number;
        Sort3(ref n1, ref n2, ref n3);

        if (n1 + 1 == n2 && n2 + 1 == n3)
        {
            info.meldType = MeldType.Shuntsu;
            info.runStart = n1;
            return info;
        }

        return info;
    }

    private void Sort3(ref int a, ref int b, ref int c)
    {
        if (a > b) Swap(ref a, ref b);
        if (b > c) Swap(ref b, ref c);
        if (a > b) Swap(ref a, ref b);
    }

    private void Swap(ref int a, ref int b)
    {
        int t = a;
        a = b;
        b = t;
    }

    private void ApplyYaku(bool condition, string name, float addMultiplier, ref float multiplier)
    {
        if (!condition) return;

        multiplier += addMultiplier;
        lastYakuHits.Add(new YakuHit(name, addMultiplier));
    }


    private bool IsTanyao(List<Card> allCards)
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            int n = allCards[i].number;
            if (n == 1 || n == 9) return false;
        }
        return true;
    }

    private bool IsIipeko(MeldInfo[] meldInfos)
    {
        Dictionary<string, int> countByKey = new Dictionary<string, int>();

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (meldInfos[i].meldType != MeldType.Shuntsu) continue;
            string key = BuildSequenceKey(meldInfos[i]);

            if (!countByKey.ContainsKey(key))
                countByKey[key] = 0;

            countByKey[key]++;
        }

        foreach (KeyValuePair<string, int> kv in countByKey)
        {
            if (kv.Value >= 2) return true;
        }

        return false;
    }

    private bool IsRyanpekoCustom(MeldInfo[] meldInfos)
    {
        Dictionary<string, int> countByKey = new Dictionary<string, int>();

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (meldInfos[i].meldType != MeldType.Shuntsu) continue;
            string key = BuildSequenceKey(meldInfos[i]);

            if (!countByKey.ContainsKey(key))
                countByKey[key] = 0;

            countByKey[key]++;
        }

        foreach (KeyValuePair<string, int> kv in countByKey)
        {
            if (kv.Value >= 3) return true;
        }

        return false;
    }

    private string BuildSequenceKey(MeldInfo info)
    {
        return ((int)info.color).ToString() + "_" + info.runStart.ToString();
    }

    private bool IsToitoi(MeldInfo[] meldInfos)
    {
        if (meldInfos.Length != 3) return false;

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (!meldInfos[i].hasThreeCards) return false;
            if (meldInfos[i].meldType != MeldType.Kkeut) return false;
        }

        return true;
    }

    private bool IsSanshokuDoujun(MeldInfo[] meldInfos)
    {
        if (meldInfos.Length != 3) return false;

        int shuntsuCount = 0;
        int runStart = -1;
        HashSet<CardColor> colors = new HashSet<CardColor>();

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (!meldInfos[i].hasThreeCards) return false;
            if (meldInfos[i].meldType != MeldType.Shuntsu) return false;

            shuntsuCount++;
            if (runStart < 0) runStart = meldInfos[i].runStart;
            else if (runStart != meldInfos[i].runStart) return false;

            colors.Add(meldInfos[i].color);
        }

        return shuntsuCount == 3 && colors.Count == 3;
    }

    private bool IsSanshokuDoukou(MeldInfo[] meldInfos)
    {
        if (meldInfos.Length != 3) return false;

        int kkeutCount = 0;
        int sameNumber = -1;
        HashSet<CardColor> colors = new HashSet<CardColor>();

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (!meldInfos[i].hasThreeCards) return false;
            if (meldInfos[i].meldType != MeldType.Kkeut) return false;

            kkeutCount++;
            if (sameNumber < 0) sameNumber = meldInfos[i].tripletNumber;
            else if (sameNumber != meldInfos[i].tripletNumber) return false;

            colors.Add(meldInfos[i].color);
        }

        return kkeutCount == 3 && colors.Count == 3;
    }

    private bool IsChinitsu(List<Card> allCards)
    {
        if (allCards.Count == 0) return false;

        CardColor c = allCards[0].color;
        for (int i = 1; i < allCards.Count; i++)
        {
            if (allCards[i].color != c) return false;
        }

        return true;
    }

    private bool IsIkkitsukan(List<Card> allCards)
    {
        if (allCards.Count != 9) return false;

        int[] counts = new int[10];

        for (int i = 0; i < allCards.Count; i++)
        {
            int n = allCards[i].number;
            if (n < 1 || n > 9) return false;
            counts[n]++;
        }

        for (int n = 1; n <= 9; n++)
        {
            if (counts[n] != 1) return false;
        }

        return true;
    }

    private bool IsChinroutou(MeldInfo[] meldInfos, List<Card> allCards)
    {
        if (!IsToitoi(meldInfos)) return false;

        for (int i = 0; i < allCards.Count; i++)
        {
            int n = allCards[i].number;
            if (n != 1 && n != 9) return false;
        }

        return true;
    }
    private bool AreAllSquadsValidMeld(MeldInfo[] meldInfos)
    {
        if (meldInfos == null || meldInfos.Length != 3) return false;

        for (int i = 0; i < meldInfos.Length; i++)
        {
            if (!meldInfos[i].hasThreeCards) return false;

            bool isValid = meldInfos[i].meldType == MeldType.Shuntsu || meldInfos[i].meldType == MeldType.Kkeut;

            if (!isValid) return false;
        }

        return true;
    }
}
