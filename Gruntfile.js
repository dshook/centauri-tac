module.exports = function gruntfile(grunt) {
  grunt.initConfig({
    browserify: {
      portal: {

      },
    },
  });

  // plugins
  require('load-grunt-tasks')(grunt);

  grunt.registerTask('portal', [
    'browserify',
  ]);
};

