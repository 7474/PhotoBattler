import Vue from 'vue'
export default class {
  getPrincipal () {
    return new Promise((resolve, reject) => {
      Vue.axios.get('/utils/checkprincipal')
        .then(function (response) {
          resolve(response)
        })
        .catch(function (error) {
          reject(error)
        })
    })
  }
  tags () {
    return new Promise((resolve, reject) => {
      Vue.axios.get('/tags')
        .then(function (response) {
          resolve(response)
        })
        .catch(function (error) {
          reject(error)
        })
    })
  }
  imagesUpload (dataUrl, tags) {
    let paramDataUrl = dataUrl
    let paramTags = tags
    return new Promise((resolve, reject) => {
      Vue.axios.post('/images/upload',
        {
          image: paramDataUrl,
          tags: paramTags
        })
        .then(function (response) {
          resolve(response)
        })
        .catch(function (error) {
          reject(error)
        })
    })
  }
  imagesPredicted (name) {
    return new Promise((resolve, reject) => {
      Vue.axios.get('/images/predicted/' + name)
        .then(function (response) {
          resolve(response)
        })
        .catch(function (error) {
          reject(error)
        })
    })
  }
}
