<template>
  <div class="images-display">
    <div class="image-info">
      <h1>{{ image.ModelName || name }}</h1>
      <ModelOwner :owner="image.User"></ModelOwner>
    </div>
    <div class="images-spec">
      <div class="image-container">
        <img v-show="name" class="image-main model-photo" :src="url" />
      </div>
      <ul class="image-attributes">
        <li v-for="tag in info.predictions" v-bind:key="tag.tagId">
          <span class="attribute-name">{{ tag.tagName }}</span><span class="attribute-value"><ICountUp
            :startVal="0"
            :endVal="(tag.probability * 100)"
            :decimals="2"
            :duration="2.5"
            :options="{}"
          /></span><span class="attribute-unit">%</span>
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Display',
  data () {
    return {
      name: null,
      url: 'data:image/gif;base64,R0lGODlhAQABAAAAACw=',
      image: {},
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
          _this.image = response.data.result
          _this.info = response.data.result.Result
        })
        .catch(error => {
          console.error(error)
        })
    }
  },
  mounted () {
    this.name = this.$route.params.name
  },
  metaInfo () {
    return {
      title: this.image.ModelName || this.name,
      titleTemplate: '%s | Photo Battler',
      meta: [
        { name: 'twitter:card', content: 'summary' },
        // 男のハードコーディング
        { name: 'twitter:site', content: '@kudenpa' },
        // { name: 'twitter:title', content: (this.image.ModelName || this.name) + ' | Photo Battler' },
        // { name: 'twitter:description', content: '' },
        { name: 'twitter:image', content: this.url }
      ]
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.images-display {
  display: flex;
  flex-direction: column;
  max-width: 600px;
  margin: 1em auto;
}
.images-spec {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
}
img.image-main {
  max-width: 320px;
  max-height: 480px;
}
ul.image-attributes {
  max-width: 260px;
  list-style: none;
  margin: 0 0 0 0.5em;
  padding: 0;
  font-size: 0.8em;
}
ul.image-attributes li {
  margin: 0.1em;
}
.attribute-name {
  font-size: 1.1em;
  margin-right: 0.5em;
}
.attribute-value {
  font-size: 1.0em;
}
.attribute-unit {
  font-size: 0.7em;
}
</style>
