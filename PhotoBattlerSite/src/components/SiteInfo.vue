<template>
  <div class="contents">
    <h1>注意事項</h1>
    <p>本サイトは未保障です。サイトの閲覧、利用によって発生したあらゆる出来事に関して一切の責任を負いません。</p>
    <p>@koudenpaは、Amazon.co.jpを宣伝しリンクすることによってサイトが紹介料を獲得できる手段を提供することを目的に設定されたアフィリエイト宣伝プログラムである、Amazonアソシエイト・プログラムの参加者です。</p>
    <p>※最適な商品をレコメンドするための学習データとしてProduct Advertising APIを使用しています。</p>
    <SearchWidgets class="ad" :trackingId="$root.env.amazon.trackingId"></SearchWidgets>
    <div class="hard-ad-list">
      <Item :trackingId="$root.env.amazon.trackingId" asin="B00RWXHD0S"></Item>
      <Item :trackingId="$root.env.amazon.trackingId" asin="B00XOQGYLG"></Item>
      <Item :trackingId="$root.env.amazon.trackingId" asin="B01ER5LPN4"></Item>
      <Item :trackingId="$root.env.amazon.trackingId" asin="B01LRMINQC"></Item>
    </div>
  </div>
</template>

<script>
import SearchWidgets from './amazon/SearchWidgets.vue'
import Item from './amazon/Item.vue'
export default {
  name: 'SiteInfo',
  components: {
    'SearchWidgets': SearchWidgets,
    'Item': Item
  }
}
</script>

<style scoped>
.ad {
  margin-top: 4em;
}
.hard-ad-list {
  display: flex;
}
</style>
