using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>
    /// The single source of truth for the clicker game. Shared by every page so
    /// navigation never loses progress. All numbers are doubles so the game can
    /// scale comfortably into trillions without switching types mid-run.
    /// </summary>
    public sealed class Game : INotifyPropertyChanged
    {
        private static readonly Game Singleton = new Game();
        public static Game Current { get { return Singleton; } }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Achievement> AchievementUnlocked;
        public event EventHandler<double> BossDefeated;

        public ObservableCollection<Upgrade> Upgrades { get; private set; }
        public ObservableCollection<Achievement> Achievements { get; private set; }

        private readonly HashSet<string> unlocked = new HashSet<string>();
        private DateTime lastClick = DateTime.MinValue;
        private DateTime lastSave = DateTime.UtcNow;

        private double cash;
        private double totalEarned;
        private double totalClicks;
        private int kills;
        private int bossLevel;
        private double bossHp;
        private double combo;
        private int ascensions;
        private bool soundEnabled = true;
        private bool reducedMotion;
        private string welcomeMessage = "Click the target to terminate MSN Money.";

        private Game()
        {
            Upgrades = new ObservableCollection<Upgrade>();
            Achievements = new ObservableCollection<Achievement>();
            BuildUpgrades();
            BuildAchievements();
            bossLevel = 1;
            bossHp = BossMaxHp;
            Load();
        }

        #region Definitions

        private void BuildUpgrades()
        {
            Upgrades.Add(new Upgrade(this, "headline", "Clickbait Headline", "+1 damage per click", 15, 1.15, 1, 0, 0));
            Upgrades.Add(new Upgrade(this, "ads", "Sponsored Ad", "+0.6 cash per second", 60, 1.16, 0, 0.6, 0));
            Upgrades.Add(new Upgrade(this, "analytics", "Analytics Dashboard", "+3 damage per click", 180, 1.17, 3, 0, 0));
            Upgrades.Add(new Upgrade(this, "bots", "Automated Bots", "+4 cash per second", 420, 1.18, 0, 4, 0));
            Upgrades.Add(new Upgrade(this, "insider", "Insider Tip", "+8 damage per click", 950, 1.19, 8, 0, 0));
            Upgrades.Add(new Upgrade(this, "algorithm", "Trading Algorithm", "+16 cash per second", 2400, 1.2, 0, 16, 0));
            Upgrades.Add(new Upgrade(this, "meme", "Viral Meme", "+24 damage per click", 6000, 1.21, 24, 0, 0));
            Upgrades.Add(new Upgrade(this, "takeover", "Hostile Takeover", "+55 cash per second", 15000, 1.22, 0, 55, 0));
            Upgrades.Add(new Upgrade(this, "keyboard", "Golden Keyboard", "x1.3 to ALL damage and income", 45000, 1.55, 0, 0, 1.3));
            Upgrades.Add(new Upgrade(this, "hedge", "Quantum Hedge Fund", "+240 cash per second", 120000, 1.23, 0, 240, 0));
        }

        private void BuildAchievements()
        {
            Achievements.Add(new Achievement("firstclick", "First Blood", "Click the target once", "\uE7C9", g => g.TotalClicks >= 1));
            Achievements.Add(new Achievement("pocket", "Pocket Change", "Earn $100 in total", "\uE7BF", g => g.TotalEarned >= 100));
            Achievements.Add(new Achievement("buyer", "Ad Buyer", "Buy your first upgrade", "\uE7AF", g => g.Upgrades.Any(u => u.Level > 0)));
            Achievements.Add(new Achievement("slayer", "Boss Slayer", "Defeat MSN Money once", "\uE734", g => g.Kills >= 1));
            Achievements.Add(new Achievement("machine", "Click Machine", "Click 100 times", "\uE7C8", g => g.TotalClicks >= 100));
            Achievements.Add(new Achievement("passive", "Passive Income", "Reach 10 cash/second", "\uE9D2", g => g.Dps >= 10));
            Achievements.Add(new Achievement("streak", "On a Roll", "Reach a x2 combo", "\uE9D5", g => g.Combo >= 25));
            Achievements.Add(new Achievement("million", "Paper Millionaire", "Earn $10,000 in total", "\uE7B0", g => g.TotalEarned >= 10000));
            Achievements.Add(new Achievement("serial", "Serial Terminator", "Defeat 10 bosses", "\uE7E9", g => g.Kills >= 10));
            Achievements.Add(new Achievement("tycoon", "Tycoon", "Buy 50 upgrades in one run", "\uE8B0", g => g.TotalUpgradeLevels >= 50));
            Achievements.Add(new Achievement("ascended", "Timeline Rewritten", "Ascend at least once", "\uE9A1", g => g.Ascensions >= 1));
        }

        #endregion

        #region Core numbers

        public double Cash { get { return cash; } }
        public double TotalEarned { get { return totalEarned; } }
        public double TotalClicks { get { return totalClicks; } }
        public int Kills { get { return kills; } }
        public int BossLevel { get { return bossLevel; } }
        public double BossHp { get { return bossHp; } }
        public double BossMaxHp { get { return Math.Floor(90 * Math.Pow(1.62, bossLevel - 1)); } }
        public double BossHpPercent { get { return BossMaxHp <= 0 ? 0 : Math.Max(0, Math.Min(100, bossHp / BossMaxHp * 100)); } }
        public string BossName { get { return BossNames[(bossLevel - 1) % BossNames.Length]; } }
        public double Combo { get { return combo; } }
        public double ComboPercent { get { return Math.Min(100, combo / 50.0 * 100); } }
        public double ComboMultiplier { get { return 1 + Math.Min(combo, 50) * 0.04; } }
        public int Ascensions { get { return ascensions; } }
        public double PrestigeMultiplier { get { return Math.Pow(1.3, ascensions); } }
        public bool SoundEnabled { get { return soundEnabled; } set { soundEnabled = value; OnPropertyChanged("SoundEnabled"); Save(); } }
        public bool ReducedMotion { get { return reducedMotion; } set { reducedMotion = value; OnPropertyChanged("ReducedMotion"); Save(); } }
        public string WelcomeMessage { get { return welcomeMessage; } set { welcomeMessage = value; OnPropertyChanged("WelcomeMessage"); } }
        public int TotalUpgradeLevels { get { return Upgrades.Sum(u => u.Level); } }

        private static readonly string[] BossNames =
        {
            "MSN Money — Ad Edition",
            "MSN Money — Premium",
            "MSN Money — Auto-Renew",
            "MSN Money — Pop-Up Swarm",
            "MSN Money — Clickbait Prime",
            "MSN Money — The Final Tab"
        };

        public double ClickPower
        {
            get
            {
                double flat = 1 + Upgrades.Sum(u => u.ClickAdd * u.Level);
                double mult = Upgrades.Aggregate(1.0, (acc, u) => acc * (u.ClickMult > 1 ? Math.Pow(u.ClickMult, u.Level) : 1.0));
                return flat * mult * PrestigeMultiplier;
            }
        }

        public double Dps
        {
            get
            {
                double flat = Upgrades.Sum(u => u.DpsAdd * u.Level);
                double mult = Upgrades.Aggregate(1.0, (acc, u) => acc * (u.ClickMult > 1 ? Math.Pow(u.ClickMult, u.Level) : 1.0));
                return flat * mult * PrestigeMultiplier;
            }
        }

        public string CashText { get { return Format(cash); } }
        public string TotalEarnedText { get { return Format(totalEarned); } }
        public string TotalClicksText { get { return Format(totalClicks); } }
        public string UpgradeCountText { get { return TotalUpgradeLevels.ToString(); } }
        public string DpsText { get { return Format(Dps); } }
        public string ClickText { get { return Format(ClickPower); } }
        public string KillsText { get { return kills.ToString(); } }
        public string BossHpText { get { return Format(bossHp) + " / " + Format(BossMaxHp); } }
        public string ComboText { get { return combo < 2 ? string.Empty : "x" + ComboMultiplier.ToString("0.00") + " COMBO"; } }
        public string MultiplierText { get { return "x" + PrestigeMultiplier.ToString("0.##"); } }
        public string OfflineText { get { return offlineEarned > 0 ? "Offline income: " + Format(offlineEarned) : string.Empty; } }

        private double offlineEarned;

        public static string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) value = 0;
            if (value >= 1e15) return (value / 1e15).ToString("0.##") + "Qa";
            if (value >= 1e12) return (value / 1e12).ToString("0.##") + "T";
            if (value >= 1e9) return (value / 1e9).ToString("0.##") + "B";
            if (value >= 1e6) return (value / 1e6).ToString("0.##") + "M";
            if (value >= 1e3) return (value / 1e3).ToString("0.##") + "K";
            if (value >= 10) return value.ToString("0");
            if (value >= 1) return value.ToString("0.#");
            return value.ToString("0.##");
        }

        #endregion

        #region Actions

        /// <summary>Player click. Returns damage dealt so the page can float the number.</summary>
        public double Click()
        {
            DateTime now = DateTime.UtcNow;
            combo = (now - lastClick).TotalSeconds <= 1.25 ? Math.Min(combo + 1, 50) : 1;
            lastClick = now;

            double damage = ClickPower * ComboMultiplier;
            Earn(damage);
            DamageBoss(damage);
            totalClicks++;

            OnPropertyChanged("TotalClicks");
            OnPropertyChanged("TotalClicksText");
            OnPropertyChanged("Combo");
            OnPropertyChanged("ComboPercent");
            OnPropertyChanged("ComboText");
            CheckAchievements();
            return damage;
        }

        /// <summary>Passive income tick. dt is seconds since previous tick.</summary>
        public void Tick(double dt)
        {
            if (combo > 0 && (DateTime.UtcNow - lastClick).TotalSeconds > 1.25)
            {
                combo = 0;
                OnPropertyChanged("Combo");
                OnPropertyChanged("ComboPercent");
                OnPropertyChanged("ComboText");
            }

            double income = Dps * dt;
            if (income <= 0) return;
            Earn(income);
            DamageBoss(income);
        }

        private void Earn(double amount)
        {
            cash += amount;
            totalEarned += amount;
            OnPropertyChanged("Cash");
            OnPropertyChanged("CashText");
            OnPropertyChanged("TotalEarned");
            OnPropertyChanged("TotalEarnedText");
        }

        private void DamageBoss(double amount)
        {
            bossHp -= amount;
            if (bossHp <= 0)
            {
                KillBoss();
            }
            else
            {
                OnPropertyChanged("BossHp");
                OnPropertyChanged("BossHpText");
                OnPropertyChanged("BossHpPercent");
            }
        }

        private void KillBoss()
        {
            double reward = 25 * Math.Pow(1.75, bossLevel - 1) * PrestigeMultiplier;
            Earn(reward);
            kills++;
            bossLevel++;
            bossHp = BossMaxHp;
            offlineEarned = 0;
            WelcomeMessage = "TARGET ELIMINATED — +" + Format(reward) + " collected. A new tab has appeared.";

            OnPropertyChanged("Kills");
            OnPropertyChanged("KillsText");
            OnPropertyChanged("CanAscend");
            OnPropertyChanged("BossLevel");
            OnPropertyChanged("BossHp");
            OnPropertyChanged("BossHpText");
            OnPropertyChanged("BossHpPercent");
            OnPropertyChanged("BossName");
            OnPropertyChanged("WelcomeMessage");

            if (BossDefeated != null) BossDefeated(this, reward);
            CheckAchievements();
        }

        public bool Buy(Upgrade upgrade)
        {
            if (upgrade == null || cash < upgrade.Cost) return false;
            cash -= upgrade.Cost;
            upgrade.Level++;
            WelcomeMessage = "Purchased " + upgrade.Name + ". Resistance is increasingly futile.";
            OnPropertyChanged("Cash");
            OnPropertyChanged("CashText");
            OnPropertyChanged("ClickPower");
            OnPropertyChanged("ClickText");
            OnPropertyChanged("Dps");
            OnPropertyChanged("DpsText");
            OnPropertyChanged("TotalUpgradeLevels");
            OnPropertyChanged("UpgradeCountText");
            OnPropertyChanged("WelcomeMessage");
            foreach (var u in Upgrades) u.RaiseCostChanged();
            CheckAchievements();
            return true;
        }

        public bool CanAscend { get { return kills >= 5; } }

        public void Ascend()
        {
            if (!CanAscend) return;
            ascensions++;
            cash = 0;
            kills = 0;
            bossLevel = 1;
            bossHp = BossMaxHp;
            combo = 0;
            offlineEarned = 0;
            foreach (var u in Upgrades) u.Level = 0;
            WelcomeMessage = "Timeline rewritten. All future damage x" + PrestigeMultiplier.ToString("0.##") + ".";
            RefreshAll();
            CheckAchievements();
            Save();
        }

        public void ResetAll()
        {
            cash = 0;
            totalEarned = 0;
            totalClicks = 0;
            kills = 0;
            bossLevel = 1;
            bossHp = BossMaxHp;
            combo = 0;
            ascensions = 0;
            offlineEarned = 0;
            unlocked.Clear();
            foreach (var u in Upgrades) u.Level = 0;
            foreach (var a in Achievements) a.Unlock(false);
            WelcomeMessage = "Fresh start. The cycle begins again.";
            RefreshAll();
            Save();
        }

        private void RefreshAll()
        {
            OnPropertyChanged("Cash");
            OnPropertyChanged("CashText");
            OnPropertyChanged("TotalEarned");
            OnPropertyChanged("TotalEarnedText");
            OnPropertyChanged("TotalClicks");
            OnPropertyChanged("TotalClicksText");
            OnPropertyChanged("Kills");
            OnPropertyChanged("KillsText");
            OnPropertyChanged("CanAscend");
            OnPropertyChanged("BossLevel");
            OnPropertyChanged("BossHp");
            OnPropertyChanged("BossHpText");
            OnPropertyChanged("BossHpPercent");
            OnPropertyChanged("BossName");
            OnPropertyChanged("Combo");
            OnPropertyChanged("ComboPercent");
            OnPropertyChanged("ComboText");
            OnPropertyChanged("ClickPower");
            OnPropertyChanged("ClickText");
            OnPropertyChanged("Dps");
            OnPropertyChanged("DpsText");
            OnPropertyChanged("PrestigeMultiplier");
            OnPropertyChanged("MultiplierText");
            OnPropertyChanged("TotalUpgradeLevels");
            OnPropertyChanged("UpgradeCountText");
            OnPropertyChanged("WelcomeMessage");
            foreach (var u in Upgrades) u.RaiseCostChanged();
        }

        #endregion

        #region Achievements

        private void CheckAchievements()
        {
            foreach (var a in Achievements)
            {
                if (a.IsUnlocked || !a.Predicate(this)) continue;
                a.Unlock(true);
                unlocked.Add(a.Id);
                WelcomeMessage = "ACHIEVEMENT UNLOCKED — " + a.Title;
                if (AchievementUnlocked != null) AchievementUnlocked(this, a);
            }
        }

        #endregion

        #region Persistence

        public void Save()
        {
            try
            {
                IsolatedStorageSettings c = IsolatedStorageSettings.ApplicationSettings;
                c["msn.cash"] = cash;
                c["msn.earned"] = totalEarned;
                c["msn.clicks"] = totalClicks;
                c["msn.kills"] = kills;
                c["msn.level"] = bossLevel;
                c["msn.hp"] = bossHp;
                c["msn.asc"] = ascensions;
                c["msn.sound"] = soundEnabled;
                c["msn.motion"] = reducedMotion;
                c["msn.ach"] = string.Join(",", unlocked);
                c["msn.saved"] = DateTime.UtcNow.Ticks.ToString();
                foreach (var u in Upgrades) c["msn.u_" + u.Id] = u.Level;
                c.Save();
                lastSave = DateTime.UtcNow;
            }
            catch (Exception)
            {
                // Storage can be unavailable in some hosted environments; the game keeps running.
            }
        }

        private void Load()
        {
            try
            {
                IsolatedStorageSettings c = IsolatedStorageSettings.ApplicationSettings;
                cash = GetDouble(c, "msn.cash", 0);
                totalEarned = GetDouble(c, "msn.earned", 0);
                totalClicks = GetDouble(c, "msn.clicks", 0);
                kills = (int)GetDouble(c, "msn.kills", 0);
                bossLevel = Math.Max(1, (int)GetDouble(c, "msn.level", 1));
                bossHp = GetDouble(c, "msn.hp", BossMaxHp);
                if (bossHp <= 0 || bossHp > BossMaxHp) bossHp = BossMaxHp;
                ascensions = (int)GetDouble(c, "msn.asc", 0);
                if (c.Contains("msn.sound")) soundEnabled = (bool)c["msn.sound"];
                if (c.Contains("msn.motion")) reducedMotion = (bool)c["msn.motion"];

                foreach (var u in Upgrades)
                    u.Level = (int)GetDouble(c, "msn.u_" + u.Id, 0);

                if (c.Contains("msn.ach"))
                {
                    string saved = c["msn.ach"] as string;
                    if (!string.IsNullOrEmpty(saved))
                    {
                        foreach (string id in saved.Split(','))
                        {
                            if (string.IsNullOrWhiteSpace(id)) continue;
                            unlocked.Add(id);
                            Achievement a = Achievements.FirstOrDefault(x => x.Id == id);
                            if (a != null) a.Unlock(true);
                        }
                    }
                }

                if (c.Contains("msn.saved"))
                {
                    long ticks;
                    if (long.TryParse(c["msn.saved"].ToString(), out ticks))
                    {
                        double seconds = (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds;
                        if (seconds > 60 && Dps > 0)
                        {
                            double capped = Math.Min(seconds, 8 * 3600);
                            offlineEarned = Dps * capped * 0.5;
                            Earn(offlineEarned);
                            WelcomeMessage = "Welcome back. Your bots earned " + Format(offlineEarned) + " while you were away.";
                        }
                    }
                }
            }
            catch (Exception)
            {
                // A corrupt save simply starts a fresh run.
            }
            lastSave = DateTime.UtcNow;
        }

        private static double GetDouble(IsolatedStorageSettings c, string key, double fallback)
        {
            object value;
            if (c.TryGetValue(key, out value) && value != null)
            {
                double result;
                if (double.TryParse(value.ToString(), out result)) return result;
            }
            return fallback;
        }

        public bool ShouldAutoSave
        {
            get { return (DateTime.UtcNow - lastSave).TotalSeconds >= 10; }
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
