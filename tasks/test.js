// since this task is going to be running our code -- setup similiar to the
// actual server

// local lib hack
// https://gist.github.com/branneman/8048520
process.env.NODE_PATH = './lib';
require('module').Module._initPaths();

require('dotenv').load();

// use bluebird instead of native promise
global.Promise = require('bluebird');

module.exports = function test(grunt) {
  grunt.loadNpmTasks('grunt-tape');

  grunt.registerTask('test', function testTask() {

    grunt.config('tape', {
      options: {},
      files: ['lib/**/*.spec.js', 'server/**/*.spec.js'],
    });

    grunt.task.run('tape');

  });

};
