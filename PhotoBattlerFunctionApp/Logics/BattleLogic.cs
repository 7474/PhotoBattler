using PhotoBattlerFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Logics
{
    class BattleLogic
    {
        // XXX 静的にこれはないなー
        static Random randomizer = new Random();

        public static BattleUnit AnalyzeParameter(PredictedInfo image, IEnumerable<Tag> tags)
        {
            // TODO 重複排除とか色々
            var elements = image.Result.Predictions.Join(
                tags,
                x => x.TagName,
                y => y.TagName,
                (x, y) => new BattleElement()
                {
                    Name = x.TagName,
                    HP = (int)Math.Floor(y.HP * x.Probability),
                    Attack = (int)Math.Floor(y.Attack * x.Probability),
                    Mobility = (int)Math.Floor(y.Mobility * x.Probability),
                    Remark = $"{x.TagName}: {x.Probability}"
                });

            return new BattleUnit()
            {
                Name = image.ModelName,
                HP = elements.Sum(x => x.HP),
                Attack = elements.Sum(x => x.Attack),
                Mobility = elements.Sum(x => x.Mobility),
                Attributes = elements.Select(x => x.Name).ToList(),
                Remark = string.Join(",", elements.Select(x => x.Remark))
            };
        }

        public static BattleResult Battle(BattleUnit unitX, BattleUnit unitY)
        {
            var attacker = unitX.Mobility >= unitY.Mobility ? unitX : unitY;

            var actions = new List<BattleAction>();
            var action = BattleAction(unitX, unitY, attacker);
            actions.Add(action);
            while (!action.IsEnd)
            {
                action = BattleAction(
                    action.UnitX,
                    action.UnitY,
                    action.Attacker == action.UnitX ? action.UnitY : action.UnitX);
                actions.Add(action);
            }
            var winner = action.UnitX.HP == 0 ? unitY : unitX;
            var loser = action.UnitX.HP == 0 ? unitX : unitY;
            return new BattleResult()
            {
                UnitX = unitX,
                UnitY = unitY,
                Winner = winner,
                Loser = loser
            };
        }
        private static BattleAction BattleAction(BattleElement unitX, BattleElement unitY, BattleElement attackUnit)
        {
            var newX = new BattleElement().CopyFrom(unitX);
            var newY = new BattleElement().CopyFrom(unitY);
            // XXX こういう比較できるんだっけ？
            var attacker = attackUnit == unitX ? newX : newY;
            var diffrencer = attacker == newX ? newY : newX;
            var hitRand = randomizer.NextDouble();
            var hitRatio = (attacker.Mobility / diffrencer.Mobility) * hitRand;
            var damage = Math.Max(10, (int)(attacker.Attack * Math.Min(1.0, hitRatio)));
            diffrencer.HP = Math.Max(0, diffrencer.HP - damage);
            var message = $"{attacker.Name} -> {diffrencer.Name} ... {damage} damage. remaining HP {diffrencer.HP}.";

            return new BattleAction()
            {
                UnitX = newX,
                UnitY = newY,
                Attacker = attacker,
                Message = message
            };
        }
    }
}
