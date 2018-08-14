using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Models
{
    // XXX BattleLogicと合わせて見直していく（ただし、HP,Attack,Mobilityの3基本構成要素は変更予定なし）
    public class BattleElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Mobility { get; set; }
        public string Remark { get; set; }

        public BattleElement CopyFrom(BattleElement source)
        {
            Id = source.Id;
            Name = source.Name;
            HP = source.HP;
            Attack = source.Attack;
            Mobility = source.Mobility;
            return this;
        }
        public bool IsSameElement(BattleElement target)
        {
            return this.Id == target.Id;
        }
    }
    public class BattleUnit : BattleElement
    {
        public string PredictedInfoKey { get; set; }
        public ICollection<string> Attributes { get; set; }
    }
    public class BattleResult
    {
        public BattleUnit UnitX { get; set; }
        public BattleUnit UnitY { get; set; }
        public BattleUnit Winner { get; set; }
        public BattleUnit Loser { get; set; }

        public IList<BattleAction> Actions { get; set; }
    }
    public class BattleAction
    {
        public BattleElement UnitX { get; set; }
        public BattleElement UnitY { get; set; }
        public BattleElement Attacker { get; set; }
        public double HitRatio { get; set; }
        public int Damage { get; set; }
        public string Message { get; set; }
        public string Remark { get; set; }
        public bool IsEnd { get { return UnitX.HP == 0 || UnitY.HP == 0; } }
    }
}
