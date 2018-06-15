<template>
  <div class="amazon-item" ref="container">
    <iframe v-if="iframeSrc" :src="iframeSrc" marginwidth="0" marginheight="0" scrolling="no" frameborder="0"></iframe>
  </div>
</template>

<script>
export default {
  name: 'Item',
  props: ['trackingId', 'asin'],
  data: () => {
    return {
      iframeSrc: null
    }
  },
  mounted () {
    this.iframeSrc = 'https://rcm-fe.amazon-adsystem.com/e/cm?ref=tf_til&t=' + this.trackingId + '&m=amazon&o=9&p=8&l=as1&IS1=1&detail=1&asins=' + this.asin + '&bc1=000000&lt1=_top&fc1=00ffff&lc1=00ff00&bg1=000000&f=ifr'
  }
}
</script>

<style>
.amazon-item {
  margin: 1em;
}
.amazon-item iframe {
  width: 120px;
  height: 240px;
  box-shadow: 0 0 10px #00ff00;
}
</style>
