using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionAppTest
{
    [TestClass]
    public class BattleTest
    {
        TestDataFactory Data = new TestDataFactory();

        Battle GetBattle()
        {
            var tags = Data.GetTags();
            var image = Data.GetEmptyPredictedInfo();
            var unitX = BattleUnit.Build(image, tags);
            var unitY = BattleUnit.Build(image, tags);
            return new Battle(unitX, unitY);
        }

        [TestMethod]
        public void TestProcess()
        {
            var battle = GetBattle();

            Assert.AreEqual(0, battle.Actions.Count, "Actions は初期0個である。");

            battle.Process();

            Assert.AreEqual(1, battle.Actions.Count, "Process すると Actions が1個増える。");
        }
        [TestMethod]
        public void TestProcessFirst()
        {
            var battle = GetBattle();

            Assert.AreEqual(0, battle.Actions.Count, "Actions は初期0個である。");

            battle.Process();

            // XXX 実装
            Assert.IsFalse(true, "1回目の Process の Action は XXX が Attacker である。");
        }
        [TestMethod]
        public void TestProcessN()
        {
            var battle = GetBattle();

            Assert.AreEqual(0, battle.Actions.Count, "Actions は初期0個である。");

            battle.Process();

            // XXX 実装
            Assert.IsFalse(true, "Process の Action の Attacker は実行毎に交互である。");
        }

        [TestMethod]
        public void TestIsEndActions0()
        {
            var battle = GetBattle();

            Assert.AreEqual(0, battle.Actions.Count, "Actions は初期0個である。");
            Assert.IsFalse(battle.IsEnd(), "Actions が0個の場合Endではない。");
        }

        [TestMethod]
        public void TestIsEnd()
        {
            var battle = GetBattle();

            // XXX 実装
            Assert.IsFalse(battle.IsEnd(), "Actions に End なものが含まれていない場合Endではない。");

            // XXX 実装
            Assert.IsTrue(battle.IsEnd(), "Actions に End なものが含まれている場合Endである。");
        }
    }
}
