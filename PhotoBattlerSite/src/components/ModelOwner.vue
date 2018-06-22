<template>
  <div class="model-owner">
    <div v-if="owner" v-show="owner.type == 'twitter'">
      By <a :href="'https://twitter.com/' + owner.name">@{{ owner.name }}</a>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ModelOwner',
  props: [
    'owner'
  ],
  computed: {
  },
  methods: {
  },
  mounted () {
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.model-owner {
  margin: 0.4em;
}
</style>
