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
}
