using PhotoBattlerFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionApp.Logics
{
    // TODO バトル仕様の再考と合わせてテストを書いていく
    // BattleLogic を潰して各クラスのメソッドに分配、乱数を必要とするメソッドでは乱数生成器を引数として受けるようにしてテスタブルにする感じだろうか
    class BattleLogic
    {
        // XXX 静的にこれはないなー
        static Random randomizer = new Random();

        public static BattleUnit AnalyzeParameter(PredictedInfo image, IEnumerable<Tag> tags)
        {
            // TODO 重複排除とか色々再考する
            // TODO HPが低すぎてバトルがすぐ終わるので数値の重みづけを再考する
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
                Id = Guid.NewGuid(),
                PredictedInfoKey = image.RowKey,
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
            // XXX イニシアティブメソッドに切り出す
            var attacker = unitX.Mobility >= unitY.Mobility ? unitX : unitY;

            var actions = new List<BattleAction>();
            var action = BattleAction(unitX, unitY, attacker);
            actions.Add(action);
            while (!action.IsEnd)
            {
                action = BattleAction(
                    action.UnitX,
                    action.UnitY,
                    action.Attacker.IsSameElement(action.UnitX) ? action.UnitY : action.UnitX);
                actions.Add(action);
            }
            // XXX 先頭不能判定をカプセル化する
            // XXX 勝敗判定をメソッドに切り出す
            var winner = action.UnitX.HP == 0 ? unitY : unitX;
            var loser = action.UnitX.HP == 0 ? unitX : unitY;
            return new BattleResult()
            {
                UnitX = unitX,
                UnitY = unitY,
                Winner = winner,
                Loser = loser,
                Actions = actions
            };
        }
        private static BattleAction BattleAction(BattleElement unitX, BattleElement unitY, BattleElement attackUnit)
        {
            var newX = new BattleElement().CopyFrom(unitX);
            var newY = new BattleElement().CopyFrom(unitY);

            // XXX ダメージを算出する処理だけを切り出して仕様を再考、テストする
            var attacker = attackUnit.IsSameElement(unitX) ? newX : newY;
            var diffrencer = attacker.IsSameElement(newX) ? newY : newX;
            var hitRand = randomizer.NextDouble();
            var hitRatio = ((double)attacker.Mobility / (double)diffrencer.Mobility) * hitRand;
            var damage = Math.Max(10, (int)(attacker.Attack * Math.Min(1.0, hitRatio)));
            diffrencer.HP = Math.Max(0, diffrencer.HP - damage);
            var message = $"{attacker.Name} -> {diffrencer.Name} ... {damage} damage. remaining HP {diffrencer.HP}.";
            var detailMessage = $"{hitRand} -> {hitRatio} * {attacker.Attack} -> {damage}";

            return new BattleAction()
            {
                UnitX = newX,
                UnitY = newY,
                Attacker = attacker,
                HitRatio = hitRatio,
                Damage = damage,
                Message = message,
                Remark = detailMessage
            };
        }
    }
}
