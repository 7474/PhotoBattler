<template>
  <div id="app">
    <header id="header">
      <img class="icon" src="./assets/logo.png">
      <menu class="menu"></menu>
      <user class="user"></user>
    </header>
    <div id="contents">
      <router-view />
    </div>
    <footer id="footer">
      © 2018 @koudenpa
    </footer>
  </div>
</template>

<script>
import User from './components/User.vue'
import Menu from './components/Menu.vue'
export default {
  name: 'App',
  components: {
    'User': User,
    'Menu': Menu
  }
}
</script>

<style>
  /* これSASSにまとめるのとどっちがいいねん。変数とかで色を管理したいよ。 */
  #app {
    position: relative;
    display: flex;
    flex-direction: column;

    /* https://www.colordic.org/ */
    /*background-color: #236641;*/
    background-color: #113320;
    color: #00ff73;
    text-shadow:
      #00ff73 0 0 1px;
    min-height: 100%;
  }
  #header {
    margin:   0;
    padding: 0.1em;
    height: 2em;
    display: flex;
    background-color: #081810;
  }
  #header .icon {
    height: 1.8em;
    width: 1.8em;
    flex: none;
  }
  #header .menu {
    flex: auto;
  }
  #header .user {
    flex: initial;
  }
  #contents {
    margin: 0;
    padding: 0;
  }
  #footer {
    margin: 0;
    padding: 0;
  }
</style>
