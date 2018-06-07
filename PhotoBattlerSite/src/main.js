// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import VueAxios from 'vue-axios'
import Meta from 'vue-meta'
import axios from 'axios'
import App from './App'
import router from './router'
import Api from './module/api'
import ICountUp from 'vue-countup-v2'
import ModelOwner from './components/ModelOwner'
const queryString = require('query-string')
// import {JSO} from 'jso'
// import crypto from 'crypto'
// import OAuth from 'oauth-1.0a'
// XXX これはありなのか？
window.api = new Api()

console.log(process.env)
axios.defaults.baseURL = process.env.apiBase
Vue.use(VueAxios, axios)
Vue.config.productionTip = false

// 他の処理をする前にコールバックされた認証情報を取得してAPIに送出する
const parsedQueryString = queryString.parse(location.search)
if (parsedQueryString.oauth_token) {
  axios.post(process.env.apiRoot + '/.auth/login/twiter', {
    oauth_token: parsedQueryString.oauth_token,
    oauth_verifier: parsedQueryString.oauth_verifier
  })
    .then((response) => {
      console.log(response.data)
      // TODO ローカルストレージに書いて復元する
      let zumo = response.data.authenticationToken
      axios.defaults.headers.common['X-ZUMO-AUTH'] = zumo
      // XXX ルーティングをハッシュでなくしたらこれだと死ぬ予感
      router.replace('/')
      if (vm) {
        vm.state.zumo = zumo
      }
    })
    .catch((error) => {
      console.error(error)
      if (vm) {
        vm.state.zumo = null
      }
    })
}

var state = {
  isAuthenticated: false,
  zumo: null,
  user: {
    name: '',
    type: ''
  }
}

Vue.component('ICountUp', ICountUp)
Vue.component('ModelOwner', ModelOwner)
Vue.use(Meta)
/* eslint-disable no-new */
let vm = new Vue({
  el: '#app',
  router,
  data: {
    env: process.env,
    state: state
  },
  components: { App },
  template: '<App/>'
})
