# PhotoBattlerSite

Photo Battlerを構成するフロントエンドアプリケーションです。

ASP.NET Coreのプロジェクトになっていますが、実態は Vue.js の静的アプリケーションです。


## CD

[../.vsts-ci.yml](../.vsts-ci.yml) に基づいてVSTSのBuildでデプロイしています。


## Local Execution

[PhotoBattlerFunctionApp](../PhotoBattlerFunctionApp) をサーバサイドアプリケーションとして実行します。

現状はCustom Visionとの接続は構成していないため、Uploadは失敗します。

See https://github.com/7474/PhotoBattler/issueSee


## Build Setup

``` bash
# install dependencies
npm install

# serve with hot reload at localhost:8080
npm run dev

# build for production with minification
npm run build

# build for production and view the bundle analyzer report
npm run build --report
```

For a detailed explanation on how things work, check out the [guide](http://vuejs-templates.github.io/webpack/) and [docs for vue-loader](http://vuejs.github.io/vue-loader).
