<template>
  <div class="images-upload">
    <div>
      <input type="text" name="modelName" v-model="modelName" placeholder="Model Name" />
    </div>
    <div>
      <input type="file" name="image" v-on:change="detectFile" />
    </div>
    <div class="imates-upload-tags">
      <select v-model="inputCategory" placeholder="Category">
        <option valule="">該当なし</option>
        <option v-for="option in categoryTags" v-bind:key="option.name" v-bind:value="option.name">
          {{ option.name }}
        </option>
      </select>
      <div>(Category)</div>
    </div>
    <div class="imates-upload-tags">
      <select v-model="inputItem" placeholder="Base Model">
        <option valule="">該当なし</option>
        <option v-for="option in itemTags" v-bind:key="option.name" v-bind:value="option.name">
          {{ option.name }}
        </option>
      </select>
      <div>(Base Model)</div>
    </div>
    <div class="imates-upload-tags">
      <select v-model="inputAttributes" multiple>
        <option v-for="option in attributeTags" v-bind:key="option.name" v-bind:value="option.name">
          {{ option.name }}
        </option>
      </select>
      <div>(Attributes)
        <ul>
          <li v-for="tag in inputAttributes" v-bind:key="tag">{{ tag }}</li>
        </ul>
      </div>
    </div>
    <div>
      <button v-show="!loading" type="button" v-on:click="upload">Upload</button>
      <i v-show="loading" class="fa fa-spinner fa-spin fa-lg fa-fw"></i>
    </div>
    <div>
      <img v-show="selectedFile" class="model-photo" :src="imageData"/>
    </div>
    <div class="alert alert-info">
      <ul>
        <li>指定したモデルの要素を選択してください。今後の評価データとさせていただきます。モデルの評価はAIが行います。</li>
        <li>選択候補が不足している場合は、その旨をお知らせください。</li>
      </ul>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Upload',
  data () {
    return {
      tags: [],
      inputCategory: null,
      inputItem: null,
      inputAttributes: [],
      selectedFile: null,
      imageData: 'data:image/gif;base64,R0lGODlhAQABAAAAACw=',
      modelName: null,
      loading: false
    }
  },
  computed: {
    name () {
      return this.isAuthenticated ? this.$root.state.user.name : '-'
    },
    isAuthenticated () {
      return this.$root.state.isAuthenticated
    },
    categoryTags () {
      return this.tags.filter(x => x.category === 'Category')
    },
    itemTags () {
      return this.tags.filter(x => x.category === 'Item')
    },
    attributeTags () {
      return this.tags.filter(x => x.category === 'Attribute')
    }
  },
  methods: {
    upload () {
      if (!this.selectedFile) {
        alert('画像を選択してください。')
        return
      }
      let _this = this
      let image = this.imageData
      let tags = [].concat(this.inputCategory, this.inputItem, this.inputAttributes).filter(x => !!x)
      let modelName = this.modelName
      _this.loading = true
      window.api
        .imagesUpload(image, tags, modelName)
        .then(response => {
          console.log(response)
          _this.$router.push('/display/' + response.data.name)
          _this.loading = false
        })
        .catch(error => {
          console.error(error)
          _this.$root.noticeError('アップロードに失敗しました。')
          _this.loading = false
        })
    },
    detectFile (e) {
      let _this = this
      e.preventDefault()
      let files = e.target.files
      var reader = new FileReader()

      reader.onload = (e) => {
        _this.imageData = e.target.result
        _this.selectedFile = files[0]
      }
      reader.readAsDataURL(files[0])
    }
  },
  mounted () {
    let _this = this
    window.api
      .tags()
      .then(response => {
        console.log(response)
        _this.tags = response.data
      })
      .catch(error => {
        console.error(error)
      })
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.images-upload {
  display: flex;
  flex-direction: column;
  max-width: 600px;
  margin: 1em auto;
}
.images-upload > * {
  padding: 0.5em;
}
.images-upload img {
  max-height: 320px;
}
.imates-upload-tags {
  display: flex;
}
.imates-upload-tags select {
  width: 12em;
  min-height: 2em;
  margin-right: 0.5em;
}
.imates-upload-tags select[multiple] {
  height: 10em;
}
.imates-upload-tags ul {
  list-style: none;
  margin: 0.1em 0.5em;
  padding: 0;
}
.imates-upload-tags li {
  margin: 0.1em 0.5em 0.1em 0.1em;
  display: inline-block;
}
</style>
