var mockery = require('mockery')
require('chai').should()

describe('its-config', function() {
  before(function() {
    mockery.enable({
      warnOnUnregistered: false,
      warnOnReplace: false
    })

    mockery.registerMock('../.config/car', {
      color: 'blue'
    })
    mockery.registerMock('../.config/bob/car', {
      make: 'Honda',
      model: 'Civic',
      options: {
        'power-steering': true,
        roof: 'moonroof'
      }
    })
    mockery.registerMock('../.config/jane/car', {
      make: 'Ford',
      model: 'Bronco',
      options: {
        '4x4': true,
        roof: 'sunroof'
      }
    })
    mockery.registerMock('../.config/low-on-gas/car', {
      gasRemaning: 0.1
    })
    mockery.registerMock('../.config/power-package/car', {
      options: {
        'power-steering': true,
        'power-windows': true
      }
    })
  })

  after(function() {
    mockery.disable()
  })

  describe('specifying precedence', function() {
    var bobsBlueCar = {
      color: 'blue',
      make: 'Honda',
      model: 'Civic',
      options: {
        'power-steering': true,
        roof: 'moonroof'
      }
    }

    afterEach(function() {
      delete process.env.precedence;
      delete process.env['Its.Configuration.Precedence'];
    })

    it('should load default configuration given no precedence', function() {
      var config = require('../its.config')()
      config('car').should.deep.equal({ color: 'blue' })
    })

    it('should load configuration given default precedence, merged with the default', function() {
      var config = require('../its.config')('jane')
      config('car').should.deep.equal({
        color: 'blue',
        make: 'Ford',
        model: 'Bronco',
        options: {
          '4x4': true,
          roof: 'sunroof'
        }
      })
    })

    it('should load configuration from the precedence environment variable', function() {
      process.env.precedence = 'bob'

      var config = require('../its.config')()
      config('car').should.deep.equal(bobsBlueCar)
    })

    it('should load configuration from the Its.Configuration.Precedence environment variable', function() {
      process.env['Its.Configuration.Precedence'] = 'bob'

      require('../its.config')()('car').should.deep.equal(bobsBlueCar)
    })

    it('should load an environment precedence over the default precedence', function() {
      process.env['Its.Configuration.Precedence'] = 'bob'

      require('../its.config')('jane')('car').should.deep.equal(bobsBlueCar)
    })
  })

  describe('merging configuration', function() {
    it('should merge configurations given pipe-delimited aspects', function() {
      require('../its.config')('low-on-gas|jane')('car').should.deep.equal({
        color: 'blue',
        make: 'Ford',
        model: 'Bronco',
        options: {
          '4x4': true,
          roof: 'sunroof'
        },
        gasRemaning: 0.1
      })
    })

    it('should merge configurations preserving desired order of precedence', function() {
      require('../its.config')('bob|jane')('car').should.deep.equal({
        color: 'blue',
        make: 'Ford',
        model: 'Bronco',
        options: {
          '4x4': true,
          roof: 'sunroof',
          'power-steering': true
        }
      })
    })

    it('should merge a bunch of stuff correctly', function() {
      require('../its.config')('low-on-gas|power-package|jane')('car').should.deep.equal({
        color: 'blue',
        make: 'Ford',
        model: 'Bronco',
        options: {
          '4x4': true,
          roof: 'sunroof',
          'power-steering': true,
          'power-windows': true
        },
        gasRemaning: 0.1
      })
    })
  })
})