var extend = function(obj) {
  obj = obj || {}
  Array.prototype.slice.call(arguments, 1).forEach(function(source) {
    Object.keys(source || {}).forEach(function(prop) {
      if (source[prop] != null && typeof source[prop] === 'object') {
        if (obj[prop] == null || typeof obj[prop] !== 'object') {
          obj[prop] = {}
        }
        extend(obj[prop], source[prop])
      } else {
        obj[prop] = source[prop]
      }
    })
  })
  return obj
}

module.exports = function(precedence) {
  var envVars = {}

  if (typeof process === 'object' && process != null && !!process.env) {
    // Node.js
    envVars = process.env
  } else if (typeof phantom === 'object' && phantom != null) {
    // PhantomJS and it's clones (SlimerJS, TrifleJS)
    envVars = require('system').env;
  }

  precedence = envVars['Its.Configuration.Precedence'] || envVars.precedence || precedence

  var aspects = [null].concat(precedence ? precedence.split('|') : [])

  return function(settings) {
    return extend.apply(null, [{}].concat(aspects.map(function(aspect) {
      try {
        return require('../../.config/' + (aspect ? aspect + '/' + settings : settings))
      } catch (e) { }
    })))
  }
}