﻿# PhotoBattlerFunctionApp

Photo Battlerを構成する Function App です。


## Local Execution

[local.settings.json（サンプル）](./local.settings.sample.json) を構成してVisual StudioでF5（Azure Function CLI ツールで実行）しています。

あらかじめ [Storage Emulator](https://docs.microsoft.com/ja-jp/azure/storage/common/storage-use-emulator) を実行しておきます。


## Snipets

ここにない関数は原則としてGUI [PhotoBattlerSite](../PhotoBattlerSite) からの呼び出しです。


### QueueImageFromAmazon

指定したASINの商品画像を教師データとする。

本リクエストの結果得られたデータを元に教師データを充実させていく。
 
```json
{
    "asin": "asin",
    "name": "name",
    "tags": ["FA:G","FA:G name","attribute","attribute"]
}
```
