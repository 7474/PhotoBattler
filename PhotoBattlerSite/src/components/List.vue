<template>
  <div class="images-list">
    <ul class="image-list-container">
      <li v-for="image in list" v-bind:key="image.name">
        <router-link  :to="'/display/' + image.name">
          <div class="image-list-item">
            <div class="image-container">
              <img class="image-thumbnail model-photo" :src="image.url" />
            </div>
            <div class="image-description">{{ image.modelName || image.name }}</div>
          </div>
        </router-link>
      </li>
    </ul>
    <div class="next">
      <a href="#" v-on:click.prevent="next()">Next</a>
    </div>
  </div>
</template>

<script>
export default {
  name: 'List',
  data () {
    return {
      current: null,
      list: []
    }
  },
  computed: {
  },
  watch: {
  },
  methods: {
    next () {
      let _this = this
      window.api
        .imagesPredictedList(this.current)
        .then(response => {
          console.log(response)
          _this.current = response.data.endName
          _this.list = response.data.list
        })
        .catch(error => {
          console.error(error)
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
.image-list-item {
  display: flex;
  margin: 0.4em;
}
.image-container {
  width: 80px;
  height: 80px;
  flex: none;
}
.image-thumbnail {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.image-description {
  padding: 0.2em 0.5em;
  word-wrap: break-word;
}
.next {
  text-align: center;
  margin: 2em;
}
</style>
