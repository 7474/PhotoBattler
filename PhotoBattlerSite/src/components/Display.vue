<template>
  <div class="images-display">
    <div class="image-info">
      <h1>{{ image.modelName || name }}</h1>
      <ModelOwner :owner="image.user"></ModelOwner>
    </div>
    <div class="images-param">
      <!-- XXX DL -->
      <div class="element">
        <div class="name">HP</div>
        <div class="value">{{ parameter.hp }}</div>
      </div>
      <div class="element">
        <div class="name">Attack</div>
        <div class="value">{{ parameter.attack }}</div>
      </div>
      <div class="element">
        <div class="name">Mobility</div>
        <div class="value">{{ parameter.mobility }}</div>
      </div>
    </div>
    <div class="images-spec">
      <div class="image-container">
        <transition name="model-photo-fade">
          <img v-show="url" class="image-main model-photo" :src="url" />
        </transition>
      </div>
      <ul class="image-attributes">
        <li v-for="tag in predictions" v-bind:key="tag.tagId">
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
    <transition name="fade">
      <div v-if="loaded"  class="sns">
        <a href="https//twitter.com/share" class="twitter-share-button" :data-text="shareText" :data-url="shareUrl" data-lang="ja">Tweet</a>
      </div>
    </transition>
    <transition name="fade">
      <div v-show="loaded" class="ad-list-container">
        <h2>Estimated Base Kit</h2>
        <div class="ad-list">
          <div class="ad-item" v-for="item in adItems" v-bind:key="item.asin">
            <div class="ad-item-p">{{ (item.probability * 100).toFixed(2) }}<span class="attribute-unit">%</span></div>
            <div class="ad-item-n">{{ item.name }}</div>
            <Item :trackingId="$root.env.amazon.trackingId" :asin="item.asin"></Item>
          </div>
        </div>
      </div>
    </transition>
    <transition name="fade">
      <div v-show="loaded">
        <h2>Select Opponent</h2>
        <List v-on:select-item.prevent="selectVsModel"></List>
      </div>
    </transition>
  </div>
</template>

<script>
import List from './List'
import Item from './amazon/Item.vue'
export default {
  name: 'Display',
  components: {
    'Item': Item,
    'List': List
  },
  data () {
    return {
      name: null,
      url: null,
      image: {},
      info: {},
      parameter: {},
      predictions: [],
      predictionsQueue: [],
      adItems: []
    }
  },
  computed: {
    loaded () {
      return this.predictionsQueue.length === 0 && this.predictions.length > 0
    },
    shareUrl () {
      return this.$root.env.apiBase + '/images/share/' + this.name
    },
    shareText () {
      return ''
    }
  },
  watch: {
    name () {
      this.updateInfo()
    },
    loaded () {
      this.$nextTick(() => {
        window.twttr.widgets.load()
      })
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
          _this.info = response.data.result.result
          _this.predictionsQueue = _this.info.predictions
          _this.processPredictionsQueue()
        })
        .catch(error => {
          console.error(error)
        })
      window.api
        .imagesParameter(this.name)
        .then(response => {
          console.log(response)
          _this.parameter = response.data
        })
        .catch(error => {
          console.error(error)
        })
      window.api
        .imagesAsin(this.name)
        .then(response => {
          console.log(response)
          _this.adItems = response.data
        })
        .catch(error => {
          console.error(error)
        })
    },
    processPredictionsQueue () {
      this.predictions.push(this.predictionsQueue.shift())
      if (this.predictionsQueue.length > 0) {
        setTimeout(this.processPredictionsQueue, 100)
      }
    },
    selectVsModel (event) {
      console.log(event.item)
      this.$root.noticeInfo('準備中です。選択: ' + event.item.result.modelName)
    }
  },
  mounted () {
    this.name = this.$route.params.name
  },
  metaInfo () {
    return {
      title: this.image.modelName || this.name,
      titleTemplate: '%s | Photo Battler'
      // meta: [
      //   { name: 'twitter:card', content: 'summary' },
      //   // 男のハードコーディング
      //   { name: 'twitter:site', content: '@kudenpa' },
      //   // { name: 'twitter:title', content: (this.image.ModelName || this.name) + ' | Photo Battler' },
      //   // { name: 'twitter:description', content: '' },
      //   { name: 'twitter:image', content: this.url }
      // ]
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
.sns {
  margin: 2em 1em 1em 1em;
}
.ad-list-container {
}
.ad-list {
  display: flex;
}
.ad-item {
  display: flex;
  flex-direction: column;
  text-align: center;
}
.ad-item-p {
  font-size: 2em;
}
.ad-item-n {

}
</style>
