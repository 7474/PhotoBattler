<template>
  <transition name="fade">
    <div class="battle-confirm">
      <Summary :image="unitX"></Summary>
      <div>VS</div>
      <Summary :image="unitY"></Summary>
      <div class="battle-confirm-buttons">
        <button type="button" @click="ok">OK</button>
        <button type="button" @click="cancel">Cancel</button>
      </div>
    </div>
  </transition>
</template>

<script>
import Summary from '../model/Summary.vue'
export default {
  name: 'BattleConfirm',
  components: {
    'Summary': Summary
  },
  props: ['unitX', 'unitY'],
  methods: {
    ok () {
      this.$emit('ok')
    },
    cancel () {
      this.$emit('cancel')
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.battle-confirm {
  text-align: center;
}
.battle-confirm-buttons {
  margin-top: 2em;
}
.battle-confirm-buttons button {
  margin: 0.5em;
  min-width: 4em;
}
</style>
