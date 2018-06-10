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
//
let zumo = window.localStorage.getItem('zumo')
if (zumo) {
  axios.defaults.headers.common['X-ZUMO-AUTH'] = zumo
}
// 他の処理をする前にコールバックされた認証情報を取得してAPIに送出する
const parsedQueryString = queryString.parse(location.search)
if (parsedQueryString.oauth_token) {
  // ハッシュベースのVue-routerだとQuerystringのReplaceが適正に働かない
  window.history.replaceState(null, null, window.location.pathname + location.hash)
  window.api.authTwitterAccessToken(parsedQueryString.oauth_token, parsedQueryString.oauth_verifier)
    .then((response) => {
      console.log(response.data)
      axios.post(process.env.apiRoot + '/.auth/login/twitter', {
        access_token: response.data.oauthToken,
        access_token_secret: response.data.oauthTokenSecret
      })
        .then((response) => {
          console.log(response.data)
          let zumo = response.data.authenticationToken
          axios.defaults.headers.common['X-ZUMO-AUTH'] = zumo
          if (vm) {
            vm.state.zumo = zumo
          }
          window.localStorage.setItem('zumo', zumo)
        })
        .catch((error) => {
          console.error(error)
          if (vm) {
            vm.state.zumo = null
          }
        })
    })
    .catch((error) => {
      console.error(error)
    })
}

var state = {
  isAuthenticated: false,
  zumo: zumo,
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
