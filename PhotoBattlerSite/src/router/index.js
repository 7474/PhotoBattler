import Vue from 'vue'
import Router from 'vue-router'
import HelloWorld from '@/components/HelloWorld'
import SiteInfo from '@/components/SiteInfo'
import Upload from '@/components/Upload'
import Display from '@/components/Display'
import List from '@/components/List'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'HelloWorld',
      component: HelloWorld
    },
    {
      path: '/siteinfo',
      name: 'SiteInfo',
      component: SiteInfo
    },
    {
      path: '/upload',
      name: 'Upload',
      component: Upload
    },
    {
      path: '/display/:name',
      name: 'Display',
      component: Display
    },
    {
      path: '/list',
      name: 'List',
      component: List
    }
  ]
})
