module.exports = function gruntfile(grunt) {
  grunt.loadTasks('tasks');

  grunt.initConfig({
    ct: {

      portal: { },

      options: {

        // copied into $webroot$/lib-assets/
        libAssets: [
          './node_modules/font-awesome/fonts/**',
          './node_modules/bootstrap/fonts/**',
        ],

        // fat 3rd party libs packaged externally to speed up build
        vendorLibs: [
          'angular',
          'angular-cookies',
          'angular-moment',
          'angular-resource',
          'angular-ui-bootstrap',
          'angular-ui-router',
          'babel/polyfill',
          'bluebird',
          'core-decorators',
          'debug',
          'lodash',
          'loglevel',
          'moment',
          'ng-table',
          'request',
        ],
      },

    },

  });

};

