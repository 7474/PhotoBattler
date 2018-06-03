// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import VueAxios from 'vue-axios'
import axios from 'axios'
import App from './App'
import router from './router'
import Api from './module/api'
import ICountUp from 'vue-countup-v2'
// XXX これはありなのか？
window.api = new Api()

console.log(process.env)

axios.defaults.baseURL = process.env.apiBase

Vue.use(VueAxios, axios)

Vue.config.productionTip = false

var state = {
  isAuthenticated: false,
  user: {
    name: '',
    type: ''
  }
}

Vue.component('ICountUp', ICountUp)
/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  data: {
    env: process.env,
    state: state
  },
  components: { App },
  template: '<App/>'
})
