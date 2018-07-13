# PhotoBattlerFunctionApp

Photo Battlerを構成する Function App です。


## Local Execution

[local.settings.json（サンプル）](./local.settings.sample.json) を構成してVisual StudioでF5（Azure Function CLI ツールで実行）しています。

あらかじめ [Storage Emulator](https://docs.microsoft.com/ja-jp/azure/storage/common/storage-use-emulator) を実行しておきます。


## Snipets

TODO Swagger.yml

See https://github.com/7474/PhotoBattler/issues/50


### QueueImageFromAmazon
 
```json
{
    "asin": "asin",
    "name": "name",
    "tags": ["FA:G","FA:G name","attribute","attribute"]
}
```
