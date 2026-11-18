using System;
using System.ComponentModel;
using System.Windows.Input;
using MSN_Money_Survival_Simulator.Common;

namespace MSN_Money_Survival_Simulator
{
    /// <summary>A purchasable upgrade. Level grows, price scales exponentially.</summary>
    public sealed class Upgrade : INotifyPropertyChanged
    {
        private readonly Game game;
        private int level;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public double BaseCost { get; private set; }
        public double CostMultiplier { get; private set; }
        public double ClickAdd { get; private set; }
        public double DpsAdd { get; private set; }
        public double ClickMult { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Upgrade(Game game, string id, string name, string description,
            double baseCost, double costMultiplier, double clickAdd, double dpsAdd, double clickMult)
        {
            this.game = game;
            Id = id;
            Name = name;
            Description = description;
            BaseCost = baseCost;
            CostMultiplier = costMultiplier;
            ClickAdd = clickAdd;
            DpsAdd = dpsAdd;
            ClickMult = clickMult;
            BuyCommand = new RelayCommand(() => game.Buy(this), () => game.Cash >= Cost);
        }

        public ICommand BuyCommand { get; private set; }

        public int Level
        {
            get { return level; }
            set
            {
                if (level == value) return;
                level = value;
                OnPropertyChanged("Level");
                OnPropertyChanged("LevelText");
                RaiseCostChanged();
            }
        }

        public double Cost { get { return Math.Floor(BaseCost * Math.Pow(CostMultiplier, level)); } }
        public string CostText { get { return "$" + Game.Format(Cost); } }
        public string LevelText { get { return level == 0 ? "NEW" : "Lv " + level; } }
        public string EffectText
        {
            get
            {
                if (ClickMult > 1) return "x" + ClickMult.ToString("0.##") + " all output / level";
                if (ClickAdd > 0) return "+" + Game.Format(ClickAdd) + " / click";
                return "+" + Game.Format(DpsAdd) + " / sec";
            }
        }

        public void RaiseCostChanged()
        {
            OnPropertyChanged("Cost");
            OnPropertyChanged("CostText");
            OnPropertyChanged("LevelText");
            OnPropertyChanged("EffectText");
            RelayCommand cmd = BuyCommand as RelayCommand;
            if (cmd != null) cmd.RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
