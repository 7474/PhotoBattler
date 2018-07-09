using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Models
{
    public class BattleElement
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Mobility { get; set; }
        public string Remark { get; set; }

        public BattleElement CopyFrom(BattleElement source)
        {
            Name = source.Name;
            HP = source.HP;
            Attack = source.Attack;
            Mobility = source.Mobility;
            return this;
        }
    }
    public class BattleUnit : BattleElement
    {
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
        public string Message { get; set; }
        public string Remark { get; set; }
        public bool IsEnd { get { return UnitX.HP == 0 || UnitY.HP == 0; } }
    }
}
