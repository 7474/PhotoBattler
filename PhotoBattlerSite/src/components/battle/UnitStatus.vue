<template>
  <div class="battle-unit-status">
    <div class="image-info">
      <Summary :image="image"></Summary>
    </div>
    <div class="images-param">
      <div class="element">
        <div class="name">HP</div>
        <div class="value">{{ unit.hp }}</div>
      </div>
      <div class="element">
        <div class="name">Attack</div>
        <div class="value">{{ unit.attack }}</div>
      </div>
      <div class="element">
        <div class="name">Mobility</div>
        <div class="value">{{ unit.mobility }}</div>
      </div>
    </div>
  </div>
</template>

<script>
import Summary from '../model/Summary.vue'
export default {
  name: 'BattleUnitStatus',
  components: {
    'Summary': Summary
  },
  props: ['image', 'unit', 'current']
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

.images-param {
  display: flex;
  flex-direction: row;
}
.images-param .element {
  text-align: center;
  min-width: 5em;
  padding: 0.5em;
}
.images-param .element .value {
  font-size: 1.6em;
}
.images-param .element .name {
  font-size: 0.8em;
}
</style>
