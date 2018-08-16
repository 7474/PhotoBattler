using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoBattlerFunctionApp.Models;

namespace PhotoBattlerFunctionAppTest
{
    [TestClass]
    public class BattleActionTest
    {
        TestDataFactory Data = new TestDataFactory();

        // XXX Actionの生成をどこに実装するか、UnitにActionメソッドを生やすのがいいか
        // XXX ダメージ算出をどこに実装するか
        // XXX 算出式の見直し
        // ダメージは なにがし 計算の結果である。

        // UnitX, Y両方のHPが1以上の場合Endではない。
        // UnitXのHPが0以下の場合Endである。
        // UnitYのHPが0以下の場合Endである。
    }
}
