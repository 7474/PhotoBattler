<template>
  <div class="images-display">
    <img :src="url" />
    <ul>
      <li v-for="tag in info.Predictions" v-bind:key="tag.TagId">
        <span>{{ tag.TagName }}</span><span>{{ tag.Probability }}%</span>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'Display',
  data () {
    return {
      name: null,
      url: null,
      info: {}
    }
  },
  computed: {
  },
  watch: {
    name () {
      this.updateInfo()
    }
  },
  methods: {
    updateInfo () {
      let _this = this
      window.api
        .imagesPredicted(this.name)
        .then(response => {
          console.log(response)
          _this.url = response.data.url
          _this.info = response.data.result
        })
        .catch(error => {
          console.error(error)
        })
    }
  },
  mounted () {
    this.name = this.$route.params.name
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
