<template>
  <div class="images-list">
    <ul class="image-list-container">
      <li v-for="image in list" v-bind:key="image.name">
        <Summary :image="image" v-on:select-item="selectItem"></Summary>
      </li>
    </ul>
    <div class="next">
      <a v-show="!loading" href="#" v-on:click.prevent="next()">Next</a>
      <i v-show="loading" class="fa fa-spinner fa-spin fa-lg fa-fw"></i>
    </div>
  </div>
</template>

<script>
import Summary from './model/Summary.vue'
export default {
  name: 'List',
  components: {
    'Summary': Summary
  },
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
        .imagesPredictedList(this.current)
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
.images-list {
  display: flex;
  flex-direction: column;
  max-width: 600px;
  margin: 0 auto;
}
ul.image-list-container {
  margin: 0;
  padding: 0;
  display: inline-flex;
  flex-wrap: wrap;
}
ul.image-list-container li {
  padding: 0.2em;
  width: 280px;
}
.next {
  text-align: center;
  margin: 2em;
}
</style>
