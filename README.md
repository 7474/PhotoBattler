# Photo Battler

Photo Battler は [Azure Functions](https://azure.microsoft.com/ja-jp/services/functions/) と
 [Azure Storage](https://azure.microsoft.com/ja-jp/services/storage/) のみでホスティングする
 Cognitive Services [Custom Vision](https://azure.microsoft.com/ja-jp/services/cognitive-services/custom-vision-service/) 
のフロントエンドの予定でしたがCDNなども使い始めました。

趣味と実益を兼ねた構成になっています。

当面は https://photo-battler.azureedge.net/index.html にホスティング予定です。
飽きたりコストが辛くなったりしたら消滅します。


## アプリケーション概要

Photo Battlerは模型の写真から戦闘力を数値化して楽しむサービス……になる予定です。

往年の[バーコードバトラー](https://ja.wikipedia.org/wiki/%E3%83%90%E3%83%BC%E3%82%B3%E3%83%BC%E3%83%89%E3%83%90%E3%83%88%E3%83%A9%E3%83%BC)でバーコードを読んだように写真を読み込んで戦闘力化、バトルするようなものを志向しています。

現在は写真を読み込んで数値化、バトルできるところまでできていますが、あまりそれらしいバトルになっていないので数値化の仕方、バトルのルールなどを見直し予定です。

数値化に関しては「このモデルはこういう要素を持っている」といった情報が登録されていくことで、段々準備が進む見込みです。


## [PhotoBattlerFunctionApp](./PhotoBattlerFunctionApp)

システムを構成する Function App です。


## [PhotoBattlerSite](./PhotoBattlerSite)

システムを構成するフロントエンドアプリケーションです。
