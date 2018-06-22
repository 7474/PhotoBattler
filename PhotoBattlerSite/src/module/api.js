import Vue from 'vue'
export default class {
  getPrincipal () {
    return Vue.axios.get('/auth/principal')
  }
  authTwitterRequestToken (returnUrl) {
    return Vue.axios.post('/auth/twitter/request_token', {
      returnUrl: returnUrl
    })
  }
  authTwitterAccessToken (oauthToken, oauthVerifier) {
    return Vue.axios.post('/auth/twitter/access_token', {
      oauthToken: oauthToken,
      oauthVerifier: oauthVerifier
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
  imagesAsin (name) {
    return Vue.axios.get('/images/asin/' + name)
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
