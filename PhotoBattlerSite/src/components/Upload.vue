<template>
  <div class="images-upload">
    <input type="file" name="image" v-on:change="detectFile" />
    <div class="imates-upload-tags">
      <select v-model="inputTags" multiple>
        <!--http://www.htmq.com/html5/optgroup.shtml-->
        <option v-for="option in tags" v-bind:key="option.Name" v-bind:value="option.Name">
          {{ option.Name }}
        </option>
      </select>
      <ul>
        <li v-for="tag in inputTags" v-bind:key="tag">{{ tag }}</li>
      </ul>
    </div>
    <div>
      <button type="button" v-on:click="upload">Upload</button>
    </div>
    <div>
      <img :src="imageData"/>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Upload',
  data () {
    return {
      tags: [],
      inputTags: [],
      selectedFile: null,
      imageData: 'data:image/gif;base64,R0lGODlhAQABAAAAACw='
    }
  },
  computed: {
    name () {
      return this.isAuthenticated ? this.$root.state.user.name : '-'
    },
    isAuthenticated () {
      return this.$root.state.isAuthenticated
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
      let tags = this.inputTags
      window.api
        .imagesUpload(image, tags)
        .then(response => {
          console.log(response)
          _this.$router.push('/display/' + response.data.name)
        })
        .catch(error => {
          console.error(error)
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
  margin: 0 auto;
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
  height: 10em;
}
.imates-upload-tags ul {
  list-style: none;
  margin: 0.1em;
  padding: 0;
}
.imates-upload-tags li {
  margin: 0.1em;
}
</style>
