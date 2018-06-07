import Vue from 'vue'
export default class {
  getPrincipal () {
    return Vue.axios.get('/auth/principal')
  }
  authTwitter (returnUrl) {
    return Vue.axios.post('/auth/twitter', {
      returnUrl: returnUrl
    })
  }
  tags () {
    return Vue.axios.get('/tags')
  }
  imagesUpload (dataUrl, tags, modelName) {
    let paramDataUrl = dataUrl
    let paramTags = tags
    let paramModelName = modelName
    return Vue.axios.post('/images/upload',
      {
        image: paramDataUrl,
        tags: paramTags,
        modelName: paramModelName
      })
  }
  imagesPredicted (name) {
    return Vue.axios.get('/images/predicted/' + name)
  }
  imagesPredictedList (startName) {
    return Vue.axios.get('/images/predicted', {
      params: {
        startName: startName || ''
      }
    })
  }
}
