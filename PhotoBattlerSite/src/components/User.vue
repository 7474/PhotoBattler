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
      return this.$root.state.isAuthenticated
    }
  },
  watch: {
    zumo () {
      this.updateAuthInfo()
    }
  },
  methods: {
    authenticate (provider) {
      //         _this.axios.defaults.headers.common['X-ZUMO-AUTH'] = response.data.zumo_token
      //         _this.updateAuthInfo()
      // window.location =
      //   this.$root.env.baseUrl +
      //   '/.auth/login/twitter?ReturnUrl=' +
      //   location.pathname +
      //   location.hash
      let returnUrl = this.$root.env.baseUrl + '/'
      // + location.pathname + location.hash
      window.api.authTwitter(returnUrl)
        .then(response => {
          console.log(response)
          window.location = 'https://api.twitter.com/oauth/authenticate?oauth_token=' + response.data.oauth_token
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
