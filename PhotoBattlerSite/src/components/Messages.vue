<template>
  <div class="messages-list">
    <ul class="message-list-container">
      <li v-for="message in messages" v-bind:key="message.message" :class="message.type">
        {{ message.message }}
        <a v-on:click.prevent="close(message.message)" href="#"><i class="far fa-window-close"></i></a>
      </li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'Messages',
  props: ['messages'],
  computed: {
  },
  watch: {
  },
  methods: {
    close (message) {
      this.$root.removeNotice(message)
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.messages-list {
  position: fixed;
  margin: 1em;
}
.messages-list li {
  margin: 0.5em;
  padding: 0.5em;
}
#app .messages-list li.info {
  background-color: #20b2aa;
  border: 1px solid #f0fff8;
  box-shadow: 0 0 10px #00ff00;
}
#app .messages-list li.error {
  background-color: #d82323;
  border: 1px solid #fff8f0;
  box-shadow: 0 0 10px #ff0000;
  color: #fff8f0;
  text-shadow: #ff0000 0 0 1px, #a00000 0 0 2px;
}
</style>
