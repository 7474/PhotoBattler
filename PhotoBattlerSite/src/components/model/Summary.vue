<template>
  <div class="image-list-item">
    <div class="image-container">
      <a :href="'#/display/' + image.name"
        v-on:click="selectItem($event, image, '/display/' + image.name)">
        <img class="image-thumbnail model-photo" :src="image.thumbnailUrl" />
      </a>
    </div>
    <div class="image-description">
      <a :href="'#/display/' + image.name"
        v-on:click="selectItem($event, image, '/display/' + image.name)">
        <div>{{ image.result.modelName || image.name }}</div>
      </a>
      <ModelOwner :owner="image.result.user"></ModelOwner>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Summary',
  props: ['image'],
  methods: {
    selectItem (event, item, path) {
      console.log(event)
      event.item = item
      this.$emit('select-item', event)
      console.log(event)
      if (!event.defaultPrevented) {
        event.preventDefault()
        console.log(path)
        this.$router.push(path)
      }
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.image-list-item {
  display: flex;
  margin: 0.4em;
  text-align: left;
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
</style>
