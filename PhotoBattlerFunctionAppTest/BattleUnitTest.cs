using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionAppTest
{
    [TestClass]
    public class BattleUnitTest
    {
        IEnumerable<Tag> GetTags()
        {
            return new List<Tag>()
            {
                new Tag()
                {
                    TagName = "A",
                    HP = 100,
                    Attack = 0,
                    Mobility = 0
                },
                new Tag()
                {
                    TagName = "B",
                    HP = 0,
                    Attack = 100,
                    Mobility = 0
                },
                new Tag()
                {
                    TagName = "C",
                    HP = 0,
                    Attack = 0,
                    Mobility = 100
                },
                new Tag()
                {
                    TagName = "D",
                    HP = 100,
                    Attack = 100,
                    Mobility = 100
                },
            };
        }

        [TestMethod]
        public void TestBuild()
        {
            IEnumerable<Tag> tags = GetTags();
            PredictedInfo image = new PredictedInfo()
            {
                Result = new ImagePrediction(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue, new List<PredictionModel>())
            };
            image.Result.Predictions.Add(new PredictionModel(0.5, Guid.Empty, "A"));
            image.Result.Predictions.Add(new PredictionModel(0.25, Guid.Empty, "B"));
            image.Result.Predictions.Add(new PredictionModel(0.1, Guid.Empty, "C"));

            var unit = BattleUnit.Build(image, tags);

            var test1 = "各要素は指定した image のラベル名が一致するタグの数値の確度を乗算した値を採用する。";
            Assert.AreEqual(unit.HP, 50, test1);
            Assert.AreEqual(unit.Attack, 25, test1);
            Assert.AreEqual(unit.Mobility, 10, test1);

            var test2 = "各要素は指定した image のラベル名が一致するタグの数値の合算である。";
            image.Result.Predictions.Add(new PredictionModel(0.1, Guid.Empty, "D"));
            var unit2 = BattleUnit.Build(image, tags);
            Assert.AreEqual(unit2.HP, 60, test2);
            Assert.AreEqual(unit2.Attack, 35, test2);
            Assert.AreEqual(unit2.Mobility, 20, test2);

            // TODO 重複排除とか色々再考する
            // TODO HPが低すぎてバトルがすぐ終わるので数値の重みづけを再考する
            // XXX HPだけ別途係数をかけるとか考える
        }

        [TestMethod]
        public void TestCopyFrom()
        {
            IEnumerable<Tag> tags = GetTags();
            PredictedInfo image = new PredictedInfo()
            {
                Result = new ImagePrediction(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue, new List<PredictionModel>())
            };
            var unit = BattleUnit.Build(image, tags);
            var unit2 = new BattleUnit().CopyFrom(unit);

            var test = "CopyFrom した要素はコピー元と各要素の値が一致する。";
            Assert.AreEqual(unit.HP, unit2.HP, test);
            Assert.AreEqual(unit.Attack, unit2.Attack, test);
            Assert.AreEqual(unit.Mobility, unit2.Mobility, test);
        }

        [TestMethod]
        public void TestCopyFromIsSameElement()
        {
            IEnumerable<Tag> tags = GetTags();
            PredictedInfo image = new PredictedInfo()
            {
                Result = new ImagePrediction(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue, new List<PredictionModel>())
            };
            var unit = BattleUnit.Build(image, tags);
            var unit2 = new BattleUnit().CopyFrom(unit);

            Assert.IsTrue(unit.IsSameElement(unit2), "CopyFrom した要素はコピー元と IsSameElement で同一視する。");
        }

        [TestMethod]
        public void TestIsSameElementSameSource()
        {
            IEnumerable<Tag> tags = GetTags();
            PredictedInfo image = new PredictedInfo()
            {
                Result = new ImagePrediction(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue, new List<PredictionModel>())
            };
            var unit = BattleUnit.Build(image, tags);
            var unit2 = BattleUnit.Build(image, tags);

            Assert.IsFalse(unit.IsSameElement(unit2), "同一のデータソースから Build した要素は IsSameElement で同一視しない。");
        }
    }
}
