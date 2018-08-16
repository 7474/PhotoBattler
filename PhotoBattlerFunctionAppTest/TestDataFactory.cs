using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using PhotoBattlerFunctionApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoBattlerFunctionAppTest
{
    // とりあえず静的にデータをコードで作る
    class TestDataFactory
    {
        public PredictedInfo GetEmptyPredictedInfo()
        {
            return new PredictedInfo()
            {
                Result = new ImagePrediction(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue, new List<PredictionModel>())
            };
        }
        public IEnumerable<Tag> GetTags()
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
    }
}
