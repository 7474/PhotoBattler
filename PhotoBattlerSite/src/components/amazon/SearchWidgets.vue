<template>
  <div class="amazon-search" ref="container">
  </div>
</template>

<script>
import postscribe from 'postscribe'
export default {
  name: 'SearchWidgets',
  props: ['trackingId', 'searchKey'],
  created () {
    window.amzn_assoc_ad_type = 'responsive_search_widget'
    window.amzn_assoc_marketplace = 'amazon'
    window.amzn_assoc_region = 'JP'
    window.amzn_assoc_placement = ''
    window.amzn_assoc_search_type = 'search_widget'
    window.amzn_assoc_width = 'auto'
    window.amzn_assoc_height = 'auto'
    window.amzn_assoc_default_search_category = 'Hobbies'
    window.amzn_assoc_theme = 'dark'
    window.amzn_assoc_bg_color = '000000'
    window.amzn_assoc_tracking_id = this.trackingId
    window.amzn_assoc_default_search_key = this.searchKey || ''
    console.log(this.trackingId)
  },
  mounted () {
    console.log(window.amzn_assoc_tracking_id)
    // let script = document.createElement('script')
    // script.setAttribute('src', '//z-fe.amazon-adsystem.com/widgets/q?ServiceVersion=20070822&Operation=GetScript&ID=OneJS&WS=1&Marketplace=JP')
    // this.$refs.container.appendChild(script)
    postscribe(this.$refs.container, '<script src="//z-fe.amazon-adsystem.com/widgets/q?ServiceVersion=20070822&Operation=GetScript&ID=OneJS&WS=1&Marketplace=JP"><\u002fscript>')
  }
}
</script>

<style>
.amazon-search {
  margin: 1em;
}
.amazon-search iframe {
  box-shadow: 0 0 10px #00ff00;
}
</style>
