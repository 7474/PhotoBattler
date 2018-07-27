<template>
  <div class="battle-list-item">
    <router-link class="unit unit-x" :to="'/display/' + battle.result.unitX.predictedInfoKey">
      <img class="image-thumbnail model-photo" :src="battle.thumbnailUrlX" />
      <div>{{ battle.result.unitX.name }}</div>
    </router-link>
    <router-link class="battle-result-link" tag="button" :to="'/battles/' + battle.rowKey">
      Battle<br>
      Result
    </router-link>
    <router-link class="unit unit-y" :to="'/display/' + battle.result.unitY.predictedInfoKey">
      <div>{{ battle.result.unitY.name }}</div>
      <img class="image-thumbnail model-photo" :src="battle.thumbnailUrlY" />
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'BattleSummary',
  props: ['battle'],
  methods: {
    selectItem (event, item) {
      console.log(event)
      event.item = item
      this.$emit('select-item', event)
      console.log(event)
      if (!event.defaultPrevented) {
        event.preventDefault()
        console.log(item)
        this.$router.push('/battles/' + item.rowKey)
      }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.battle-list-item {
  display: flex;
  margin: 0.4em;
  text-align: left;
}
.battle-result-link {
  width: 4em;
  height: 4em;
  text-align: center;
  margin-top: 2.5em;
}
.image-thumbnail {
  width: 80px;
  height: 80px;
  object-fit: cover;
}
.unit {
  width: 8em;
  padding: 0.2em 0.5em;
  word-wrap: break-word;
}
.unit-x {
  text-align: left;
}
.unit-y {
  text-align: right;
}
</style>
