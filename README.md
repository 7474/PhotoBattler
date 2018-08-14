# Photo Battler

Photo Battler は [Azure Functions](https://azure.microsoft.com/ja-jp/services/functions/) と
 [Azure Storage](https://azure.microsoft.com/ja-jp/services/storage/) のみでホスティングする
 Cognitive Services [Custom Vision](https://azure.microsoft.com/ja-jp/services/cognitive-services/custom-vision-service/) 
のフロントエンドの予定でしたがCDNなども使い始めました。

趣味と実益を兼ねた構成になっています。

当面は https://photo-battler.azureedge.net/index.html にホスティング予定です。
飽きたりコストが辛くなったりしたら消滅します。


## [PhotoBattlerFunctionApp](./PhotoBattlerFunctionApp)

システムを構成する Function App です。


## [PhotoBattlerSite](./PhotoBattlerSite)

システムを構成するフロントエンドアプリケーションです。

