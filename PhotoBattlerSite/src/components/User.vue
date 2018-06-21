<template>
  <div class="user-identity">
    <div v-show="!loading">
      <div v-show="isAuthenticated">
        <a href="#" @click.prevent="usermenuVisible = !usermenuVisible">{{ name }}</a>
        <div v-show="usermenuVisible" class="user-menu">
          <button type="button" @click.prevent="logout()">logout</button>
        </div>
      </div>
      <div v-show="!isAuthenticated">
        <button type="button" @click.prevent="authenticate('twitter')">auth Twitter</button>
      </div>
    </div>
    <i v-show="loading" class="fa fa-spinner fa-spin fa-lg fa-fw"></i>
  </div>
</template>

<script>
export default {
  name: 'User',
  data () {
    return {
      loading: false,
      usermenuVisible: false
    }
  },
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
      let _this = this
      _this.loading = true
      // XXX twitter only
      let returnUrl = this.$root.env.baseUrl + location.pathname + location.hash
      window.api.authTwitterRequestToken(returnUrl)
        .then(response => {
          console.log(response)
          window.location = 'https://api.twitter.com/oauth/authenticate?oauth_token=' + response.data.oauthToken
        })
        .catch(error => {
          console.error(error)
          _this.$root.noticeError('認証処理に失敗しました。')
          _this.loading = false
        })
    },
    logout () {
      // TODO /.auth/logout
      this.$root.updateZumo(null)
    },
    updateAuthInfo () {
      let _this = this
      _this.loading = true
      window.api
        .getPrincipal()
        .then(response => {
          console.log(response)
          _this.$root.state.isAuthenticated = response.data.isAuthenticated
          _this.$root.state.user = response.data.identity
          _this.loading = false
        })
        .catch(error => {
          // ignore
          console.error(error)
          _this.loading = false
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
.user-menu {
  position: fixed;
  right: 0.5em;
  top: 2em;
}
</style>
