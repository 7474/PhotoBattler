<template>
  <div class="user-identity">
    <div v-show="isAuthenticated">{{ name }}</div>
    <div v-show="!isAuthenticated">
      <button @click="authenticate('twitter')">auth Twitter</button>
    </div>
  </div>
</template>

<script>
// import OAuthioWeb from 'oauthio-web'
// let OAuth = OAuthioWeb.OAuthio.OAuth
export default {
  name: 'User',
  computed: {
    name () {
      return this.isAuthenticated ? this.$root.state.user.name : '-'
    },
    isAuthenticated () {
      return this.$root.state.isAuthenticated
    }
  },
  methods: {
    authenticate (provider) {
      // let _this = this
      // XXX OAuth.js中間サービスじゃん。分かりづらいねん。。。
      // OAuth.initialize(this.$root.env.twitter.clientId)
      // OAuth.popup(provider)
      //   .done(result => {
      //     _this.axios.post(_this.$root.env.baseUrl + '/.auth/login/' + provider, {
      //       access_token: result.access_token
      //     })
      //       .then(response => {
      //         // XXX なにがくんの？
      //         console.log(response)
      //         // XXX Cookieなりに入れて再利用する
      //         _this.axios.defaults.headers.common['X-ZUMO-AUTH'] = response.data.zumo_token
      //         _this.updateAuthInfo()
      //       })
      //       .catch(error => {
      //         console.error(error)
      //       })
      //   })
      //   .fail(error => {
      //     console.error(error)
      //   })
      // とりあえずこの辺を見るのがいいんじゃないかな？
      // https://docs.microsoft.com/ja-jp/azure/app-service/app-service-authentication-overview
      // しかし
      // > さらには Azure Functions でも、最小限のコードを記述するだけで、またはまったく記述せずに、ユーザーのサインインとデータへのアクセスを可能にできます。
      // これは嘘だろう。。。
      // とりあえず ReturnUrl というパラメータは存在しないし、インタフェースのリファレンスも存在しない
      window.location =
        this.$root.env.baseUrl +
        '/.auth/login/twitter?ReturnUrl=' +
        location.pathname +
        location.hash
    },
    updateAuthInfo () {
      let _this = this
      window.api
        .getPrincipal()
        .then(response => {
          console.log(response)
          _this.$root.state.isAuthenticated = response.data.isAuthenticated
          _this.$root.state.user = response.data.identity
        })
        .catch(error => {
          console.error(error)
        })
    }
  },
  mounted () {
    this.updateAuthInfo()
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
