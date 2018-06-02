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
    }
  },
  methods: {
    authenticate (provider) {
      window.location =
        this.$root.env.baseUrl +
        '/.auth/login/twitter?ReturnUrl=' +
        location.pathname +
        location.hash
    },
    updateAuthInfo () {
      window.api
        .getPrincipal()
        .then(response => {
          console.log(response)
          this.$root.state.isAuthenticated = response.isAuthenticated
          this.$root.state.user = response.user
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
