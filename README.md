# Photo Battler

Photo Battler は [Azure Functionos](https://azure.microsoft.com/ja-jp/services/functions/) と [Azure Storage](https://azure.microsoft.com/ja-jp/services/storage/) のみでホスティングする Cognitive Services [Custom Vision](https://azure.microsoft.com/ja-jp/services/cognitive-services/custom-vision-service/) のフロントエンドです。

趣味と実益を兼ねた構成になっています。

当面は https://photo-battler.azurewebsites.net/ にホスティング予定です。
飽きたりコストが辛くなったりしたら消滅します。


## [PhotoBattlerFunctionApp](./PhotoBattlerFunctionApp)

システムを構成する Function App です。


## [PhotoBattlerSite](./PhotoBattlerSite)

システムを構成するフロントエンドアプリケーションです。

ASP.NET Coreのプロジェクトになっていますが、実態は Vue.js の静的アプリケーションです。

