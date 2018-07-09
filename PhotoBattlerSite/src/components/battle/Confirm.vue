<template>
  <transition name="fade">
    <div class="battle-confirm">
      <div>{{ unitX.modelName }}</div>
      <div>VS</div>
      <div>{{ unitY.modelName }}</div>
      <button type="button" @click="ok">OK</button>
      <button type="button" @click="cancel">Cancel</button>
    </div>
  </transition>
</template>

<script>
export default {
  name: 'BattleConfirm',
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

</style>
