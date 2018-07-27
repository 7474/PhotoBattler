<template>
  <div class="battle-list">
    <ul class="battle-list-container">
      <li v-for="battle in list" v-bind:key="battle.name">
        <Summary :battle="battle" v-on:select-item="selectItem"></Summary>
      </li>
    </ul>
    <div class="next">
      <a v-show="!loading" href="#" v-on:click.prevent="next()">Next</a>
      <i v-show="loading" class="fa fa-spinner fa-spin fa-lg fa-fw"></i>
    </div>
  </div>
</template>

<script>
import Summary from './Summary.vue'
export default {
  name: 'List',
  components: {
    'Summary': Summary
  },
  props: ['name'],
  data () {
    return {
      current: null,
      list: [],
      loading: false
    }
  },
  computed: {
  },
  watch: {
  },
  methods: {
    selectItem (event) {
      this.$emit('select-item', event)
    },
    next () {
      let _this = this
      _this.loading = true
      window.api
        .battleResultList(this.current, this.name)
        .then(response => {
          console.log(response)
          _this.current = response.data.endName
          _this.list = response.data.list
          _this.loading = false
        })
        .catch(error => {
          console.error(error)
          _this.$root.noticeError('データの取得に失敗しました。')
          _this.loading = false
        })
    }
  },
  mounted () {
    this.next()
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.battle-list {
  display: flex;
  flex-direction: column;
  max-width: 800px;
  margin: 0 auto;
}
ul.battle-list-container {
  margin: 0;
  padding: 0;
  display: inline-flex;
  flex-wrap: wrap;
  justify-content: space-evenly;
}
ul.battle-list-container li {
  padding: 0.2em;
  width: 360px;
  margin: 24px 12px;
}
.next {
  text-align: center;
  margin: 2em;
}
</style>
