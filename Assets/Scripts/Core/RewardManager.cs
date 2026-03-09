using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    public enum RewardType  { Funds, Seeds, WaterCapacity }
    public enum TriggerType { CropsHarvested, DaysSurvived, SeasonCompleted, AllTilesWatered, FundsEarned }

    [System.Serializable]
    public class RewardGrant
    {
        public RewardType type;
        public float      amount;

        public string GetDescription()
        {
            switch (type)
            {
                case RewardType.Funds:         return $"+${amount} Funds";
                case RewardType.Seeds:         return $"+{(int)amount} Seeds";
                case RewardType.WaterCapacity: return $"+{amount} Max Water";
                default:                       return type.ToString();
            }
        }
    }

    [System.Serializable]
    public class Reward
    {
        public string            rewardName;
        public string            description;
        public TriggerType       trigger;
        public int               milestone;
        public List<RewardGrant> grants = new List<RewardGrant>();
        public AudioClip         milestoneSound;
        public Sprite            rewardIcon;
        [HideInInspector] public bool claimed = false;
    }

    [Header("Rewards — edit freely in Inspector")]
    public List<Reward> rewards = new List<Reward>()
    {
        new Reward
        {
            rewardName  = "Achievement completed: Getting Started",
            description = "Harvested your first 5 crops!",
            trigger     = TriggerType.CropsHarvested,
            milestone   = 5,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds, amount = 50f },
                new RewardGrant { type = RewardType.Seeds, amount = 3f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Seasoned Farmer",
            description = "Harvested 10 crops!",
            trigger     = TriggerType.CropsHarvested,
            milestone   = 10,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds,         amount = 150f },
                new RewardGrant { type = RewardType.WaterCapacity, amount = 25f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Master Harvester",
            description = "Harvested 25 crops!",
            trigger     = TriggerType.CropsHarvested,
            milestone   = 25,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds,         amount = 500f },
                new RewardGrant { type = RewardType.WaterCapacity, amount = 25f  },
                new RewardGrant { type = RewardType.Seeds,         amount = 10f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: First Week",
            description = "Survived 7 days on the farm!",
            trigger     = TriggerType.DaysSurvived,
            milestone   = 7,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds, amount = 75f },
                new RewardGrant { type = RewardType.Seeds, amount = 5f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Veteran Farmer",
            description = "Survived 28 days on the farm!",
            trigger     = TriggerType.DaysSurvived,
            milestone   = 28,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds,         amount = 200f },
                new RewardGrant { type = RewardType.WaterCapacity, amount = 50f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Spring Graduate",
            description = "Completed your first season!",
            trigger     = TriggerType.SeasonCompleted,
            milestone   = 1,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds, amount = 100f },
                new RewardGrant { type = RewardType.Seeds, amount = 10f  },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Thorough Gardener",
            description = "Watered every tile in one day!",
            trigger     = TriggerType.AllTilesWatered,
            milestone   = 1,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Funds,         amount = 50f },
                new RewardGrant { type = RewardType.WaterCapacity, amount = 10f },
            }
        },
        new Reward
        {
            rewardName  = "Achievement completed: Small Business",
            description = "Earned $500 total!",
            trigger     = TriggerType.FundsEarned,
            milestone   = 500,
            grants      = new List<RewardGrant>
            {
                new RewardGrant { type = RewardType.Seeds,         amount = 10f },
                new RewardGrant { type = RewardType.WaterCapacity, amount = 15f },
            }
        },
    };

    [Header("References")]
    [SerializeField] private RewardUI             rewardUI;
    [SerializeField] private CelebrationParticles celebration;
    [SerializeField] private AudioSource          audioSource;

    private int _seasonsCompleted = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persists across scenes — keeps claimed flags intact
    }

    // ── Public triggers ───────────────────────────────────────────────────────
    public void CheckMilestones()    => CheckTrigger(TriggerType.CropsHarvested, GameManager.Instance != null ? GameManager.Instance.harvestedCrops : 0);
    public void CheckDayMilestones() => CheckTrigger(TriggerType.DaysSurvived,   GameManager.Instance != null ? GameManager.Instance.savedDay        : 0);

    public void OnSeasonCompleted()
    {
        _seasonsCompleted++;
        Debug.Log($"[RewardManager] Seasons completed: {_seasonsCompleted}");
        CheckAll();
    }

    public void OnAllTilesWatered()   => CheckTrigger(TriggerType.AllTilesWatered, 1);

    // ── Internal ──────────────────────────────────────────────────────────────
    private void CheckAll()
    {
        if (GameManager.Instance == null) return;

        int   crops  = GameManager.Instance.harvestedCrops;
        int   day    = GameManager.Instance.savedDay;
        float funds  = GameManager.Instance.currentFunds;

        Debug.Log($"[RewardManager] CheckAll — Day={day} Crops={crops} Funds={funds}");

        foreach (Reward reward in rewards)
        {
            if (reward.claimed) continue;

            bool triggered = false;
            switch (reward.trigger)
            {
                case TriggerType.CropsHarvested:  triggered = crops  >= reward.milestone; break;
                case TriggerType.DaysSurvived:     triggered = day    >= reward.milestone; break;
                case TriggerType.SeasonCompleted:  triggered = _seasonsCompleted >= reward.milestone; break;
                case TriggerType.FundsEarned:      triggered = funds  >= reward.milestone; break;
            }

            if (triggered)
            {
                ClaimReward(reward);
                break; // Stop after one reward per check — prevents cascade
            }
        }
    }

    private void CheckTrigger(TriggerType type, int value)
    {
        foreach (Reward reward in rewards)
        {
            if (!reward.claimed && reward.trigger == type && value >= reward.milestone)
            {
                ClaimReward(reward);
                break; // One reward at a time
            }
        }
    }

    private void ClaimReward(Reward reward)
    {
        reward.claimed = true;

        foreach (RewardGrant grant in reward.grants)
            ApplyGrant(grant);

        List<string> grantLines = new List<string>();
        foreach (RewardGrant grant in reward.grants)
            grantLines.Add(grant.GetDescription());

        Debug.Log($"🏆 [{reward.rewardName}] {reward.description} — {string.Join(", ", grantLines)}");

        if (audioSource != null && reward.milestoneSound != null)
            audioSource.PlayOneShot(reward.milestoneSound);

        if (rewardUI != null)
            rewardUI.ShowReward(reward.rewardName, reward.description, reward.rewardIcon, grantLines);

        if (celebration != null)
            celebration.Celebrate();
    }

    private void ApplyGrant(RewardGrant grant)
    {
        var gm = GameManager.Instance;
        switch (grant.type)
        {
            case RewardType.Funds:
                gm.currentFunds += grant.amount;
                break;
            case RewardType.Seeds:
                gm.seedCount += (int)grant.amount;
                break;
            case RewardType.WaterCapacity:
                gm.maxWater     += grant.amount;
                gm.currentWater  = Mathf.Min(gm.currentWater + grant.amount, gm.maxWater);
                break;
        }
    }
}