<template>
  <div class="battle-result" v-bind:style="containerStyle">
    <div v-if="loaded" class="battle-result-inner">
      <div class="battle-result-units">
        <div class="battle-result-unit-container">
          <UnitStatus :image="imageX" :unit="result.unitX" :current="currentX"></UnitStatus>
          <div class="result"><span class="winner" v-show="finished && isXWin">Winner</span></div>
        </div>
        <div class="battle-result-unit-container">
          <UnitStatus :image="imageY" :unit="result.unitY" :current="currentY"></UnitStatus>
          <div class="result"><span class="winner" v-show="finished && isYWin">Winner</span></div>
        </div>
      </div>
      <div class="battle-result-actions">
        <ul>
          <li class="battle-result-action" v-for="action in actions" v-bind:key="action.message">
            {{ action.message }}
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import Item from '../amazon/Item.vue'
import UnitStatus from './UnitStatus.vue'
export default {
  name: 'BattleResult',
  components: {
    UnitStatus: UnitStatus,
    Item: Item
  },
  data () {
    return {
      battleId: null,
      result: null,
      imageX: null,
      imageY: null,
      currentX: null,
      currentY: null,
      actions: [],
      actionsQueue: []
    }
  },
  computed: {
    loaded () {
      return !!this.result
    },
    finished () {
      return this.loaded && this.actionsQueue.length === 0 && this.actions.length > 0
    },
    shareUrl () {
      return this.$root.env.apiBase + '/battles/share/' + this.battleId
    },
    shareText () {
      return ''
    },
    isXWin () {
      return this.result.unitX.id === this.result.winner.id
    },
    isYWin () {
      return this.result.unitY.id === this.result.winner.id
    },
    containerStyle () {
      if (this.loaded) {
        return ({
          'background-image': 'url("' + this.imageX.url + '"), url("' + this.imageY.url + '")',
          'background-repeat': 'no-repeat, no-repeat',
          'background-position': 'left top, right top',
          'background-size': '50%, 50%'
        })
      } else {
        return {}
      }
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
          _this.imageX = response.data.imageX
          _this.imageY = response.data.imageY
          _this.currentX = response.data.result.unitX
          _this.currentY = response.data.result.unitY
          _this.result = response.data.result
          _this.actionsQueue = response.data.result.actions.concat()
          setTimeout(_this.processActionsQueue, 1000)
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
    },
    processActionsQueue () {
      let action = this.actionsQueue.shift()
      this.actions.push(action)
      this.currentX = action.unitX
      this.currentY = action.unitY
      if (this.actionsQueue.length > 0) {
        setTimeout(this.processActionsQueue, 500)
      }
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
.battle-result-inner {
  background-color: rgba(0, 0, 0, 0.6);
  min-height: 100vh;
}
.battle-result-units {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
}
.battle-result-unit-container {
  flex: 1;
}
.battle-result-units .result {
  height: 2em;
  font-size: 1.6em;
  font-weight: bold;
  text-align: center;
}
.battle-result-units .winner {
}
.battle-result-actions {
  margin: 1em;
}
.battle-result-actions ul {
  list-style-type: none;
  margin: 0;
  padding: 0;
}
.battle-result-action {
  margin: 0.5em;
}
</style>
