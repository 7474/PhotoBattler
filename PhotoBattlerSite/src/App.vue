<template>
  <div id="app">
    <header id="header">
      <router-link class="icon" to="/"><img class="icon" src="./assets/logo.png"></router-link>
      <app-menu class="menu"></app-menu>
      <user class="user"></user>
    </header>
    <div id="contents">
      <router-view />
    </div>
    <footer id="footer">
      <p><router-link to="/siteinfo">Caution</router-link></p>
      <p>© 2018 <a href="https://twitter.com/koudenpa">@koudenpa</a></p>
    </footer>
    <messages :messages="$root.messages"></messages>
  </div>
</template>

<script>
import User from './components/User.vue'
import AppMenu from './components/Menu.vue'
import Messages from './components/Messages.vue'
export default {
  name: 'App',
  components: {
    'User': User,
    'AppMenu': AppMenu,
    'Messages': Messages
  },
  metaInfo: {
    title: 'Photo Battler'
    // meta: [
    //   { name: 'twitter:card', content: 'summary' },
    //   // 男のハードコーディング
    //   { name: 'twitter:site', content: '@kudenpa' },
    //   { name: 'twitter:image', content: '/site/images/dream.png' }
    // ]
  }
}
</script>

<style>
  @import url(https://fonts.googleapis.com/earlyaccess/notosansjapanese.css);
  /* これSASSにまとめるのとどっちがいいねん。変数とかで色を管理したいよ。 */
  #app {
    position: relative;
    display: flex;
    flex-direction: column;

    /* https://www.colordic.org/ */
    /*background-color: #236641;*/
    background-color: #081810;
    font-family: 'Noto Sans Japanese', sans-serif;
    font-weight: 100;
    color: #f0fff8;
    text-shadow:
      #00ff00 0 0 1px,
      #00a000 0 0 2px;
    min-height: 100%;
  }
  #app a {
    color: #80ffc0;
  }
  #app .contents {
    max-width: 600px;
    margin: 0 auto;
    padding: 0.5em 0;
  }
  #header {
    margin: 0;
    padding: 0;
    min-height: 3em;
    display: flex;
    background-color: #043927;
  }
  #header .icon {
    height: 3em;
    flex: none;
  }
  #header .menu {
    padding: 0.4em;
    flex: auto;
  }
  #header .user {
    padding: 0.4em 0.4em;
    flex: initial;
  }
  #contents {
    margin: 0;
    padding: 0;
    min-height: 480px;
  }
  #footer {
    margin: 0;
    padding: 0;
    text-align: center;
    background-color: #043927;
  }
  .alert {
    border: 1px solid #f0fff8;
    box-shadow: 0 0 10px #00ff00;
    margin: 0.5em;
    padding: 0.5em;
  }
  /* .alert-info {
  } */
  .model-photo {
    border-radius: 10px;
    border: 1px solid #f0fff8;
    box-shadow: 0 0 10px #00ff00;
  }
  .model-photo-fade-enter-active {
    transition: opacity .5s;
  }
  .model-photo-fade-leave-active {
    transition: opacity .5s;
  }
  .model-photo-fade-enter, .model-photo-fade-leave-to {
    opacity: 0;
  }
  .fade-enter-active {
    transition: opacity .5s;
  }
  .fade-leave-active {
    transition: opacity .5s;
  }
  .fade-enter, .fade-leave-to {
    opacity: 0;
  }
  button, #app .menu a {
    background-color: transparent;
    border: 1px solid #f0fff8;
    box-shadow: 0 0 10px #00ff00;
    color: #f0fff8;
    text-shadow:
      #00ff00 0 0 1px,
      #00a000 0 0 2px;
    text-decoration-line: none;
    font-weight: 100;
    font-size: 1em;
    padding: 0.2em;
  }
  h1 {
    font-size: 1.6em;
  }
  h2 {
    font-size: 1.4em;
  }
  h3 {
    font-size: 1.2em;
  }
  h4 {
    font-size: 1.1em;
  }
  ::placeholder {
    color: #00a000;
    text-shadow: none;
    padding: 0.4em;
  }
  input, select {
    min-height: 1.8em;
  }
  input, select, textarea {
    background-color: #043927;
    border: 1px solid #f0fff8;
    box-shadow: 0 0 10px #00ff00;
    color: #f0fff8;
    text-shadow:
      #00ff00 0 0 1px,
      #00a000 0 0 2px;
    outline: 0;
  }
  *:focus {
    outline: solid 1px #f0fff8;
    box-shadow: 0 0 20px #80ff80;
  }
  input:focus, select:focus, textarea:focus {
    box-shadow: 0 0 20px #80ff80;
  }
  ul {
    list-style: none;
    margin: 0;
    padding: 0;
  }

</style>
