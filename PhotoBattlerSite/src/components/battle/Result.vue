<template>
  <div class="battle-result">
    <div v-if="loaded">
      <div>Winner: {{ result.winner.name }}
        <br>
        {{ result.winner.hp }},
        {{ result.winner.attack }},
        {{ result.winner.mobility }},
      </div>
      <div>Loser: {{ result.loser.name }}
        <br>
        {{ result.loser.hp }},
        {{ result.loser.attack }},
        {{ result.loser.mobility }},
      </div>
      <ol>
        <li v-for="action in result.actions" v-bind:key="action">
          {{ action.message }}
        </li>
      </ol>
    </div>
  </div>
</template>

<script>
import Item from '../amazon/Item.vue'
export default {
  name: 'BattleResult',
  components: {
    'Item': Item
  },
  data () {
    return {
      battleId: null,
      result: null
    }
  },
  computed: {
    loaded () {
      return !!this.result
    },
    shareUrl () {
      return this.$root.env.apiBase + '/battles/share/' + this.battleId
    },
    shareText () {
      return ''
    }
  },
  watch: {
    battleId () {
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
        .battleResult(this.battleId)
        .then(response => {
          console.log(response)
          _this.result = response.data
        })
        .catch(error => {
          console.error(error)
        })
        // TODO Winnerを出す？
      // window.api
      //   .imagesAsin(this.name)
      //   .then(response => {
      //     console.log(response)
      //     _this.adItems = response.data
      //   })
      //   .catch(error => {
      //     console.error(error)
      //   })
    }
  },
  mounted () {
    this.battleId = this.$route.params.battleId
  },
  metaInfo () {
    return {
      // TODO Fix
      title: this.battleId,
      titleTemplate: '%s | Photo Battler'
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
