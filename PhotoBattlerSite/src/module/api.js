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
  imagesUpload (dataUrl, tags, modelName) {
    let paramDataUrl = dataUrl
    let paramTags = tags
    let paramModelName = modelName
    return new Promise((resolve, reject) => {
      Vue.axios.post('/images/upload',
        {
          image: paramDataUrl,
          tags: paramTags,
          modelName: paramModelName
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
  imagesPredictedList (startName) {
    return new Promise((resolve, reject) => {
      Vue.axios.get('/images/predicted', {
        params: {
          startName: startName || ''
        }
      })
        .then(function (response) {
          resolve(response)
        })
        .catch(function (error) {
          reject(error)
        })
    })
  }
}
