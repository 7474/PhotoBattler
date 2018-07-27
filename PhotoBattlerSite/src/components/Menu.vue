<template>
  <div class="menu">
    <router-link to="/upload">Upload</router-link>
    <router-link to="/list">Images</router-link>
    <router-link to="/battles">Battles</router-link>
  </div>
</template>

<script>
export default {
  name: 'Menu'
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.menu a {
  width: 5em;
  text-align: center;
  display: inline-block;
  margin: auto 0.4em;
}
</style>
