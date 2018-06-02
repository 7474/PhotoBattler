'use strict'
const merge = require('webpack-merge')
const prodEnv = require('./prod.env')

module.exports = merge(prodEnv, {
  NODE_ENV: '"development"',
  apiBase: '"http://localhost:7071/api"',
  baseUrl: '"http://127.0.0.1:5841"',
  twitter: {
    clientId: '"NuASgfCzS6aGB7ZFRthRx8LMP"'
  }
})
