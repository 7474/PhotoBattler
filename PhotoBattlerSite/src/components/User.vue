<template>
  <div class="user-identity">
    <div v-show="isAuthenticated">{{ name }}</div>
    <div v-show="!isAuthenticated">
      <button @click="authenticate('twitter')">auth Twitter</button>
    </div>
  </div>
</template>

<script>

export default {
  name: 'User',
  computed: {
    name () {
      return this.isAuthenticated ? this.$root.state.user.name : '-'
    },
    isAuthenticated () {
      return this.$root.state.isAuthenticated
    },
    zumo () {
      return this.$root.state.zumo
    }
  },
  watch: {
    zumo () {
      let _this = this
      // XXX 初回認証直後はStorageTableのCollector処理が完了していない可能性があるため若干待ってからリクエストする。ダサ味。
      setTimeout(() => {
        _this.updateAuthInfo()
      }, 100)
    }
  },
  methods: {
    authenticate (provider) {
      // XXX twitter only
      let returnUrl = this.$root.env.baseUrl + location.pathname + location.hash
      window.api.authTwitterRequestToken(returnUrl)
        .then(response => {
          console.log(response)
          window.location = 'https://api.twitter.com/oauth/authenticate?oauth_token=' + response.data.oauthToken
        })
        .catch(error => {
          console.error(error)
        })
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
