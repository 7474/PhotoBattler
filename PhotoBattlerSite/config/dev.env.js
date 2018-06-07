'use strict'
const merge = require('webpack-merge')
const prodEnv = require('./prod.env')

module.exports = merge(prodEnv, {
  NODE_ENV: '"development"',
  apiRoot: '"http://localhost:7071"',
  apiBase: '"http://localhost:7071/api"',
  // baseUrl: '"http://127.0.0.1:5841"',
  baseUrl: '"http://127.0.0.1:8080"'
})
