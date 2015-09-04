var DEBUG = true;
var VENDOR_LIBS = [
  'angular',
  'angular-ui-router',
  'angular-ui-bootstrap',
  'babel/polyfill',
  'bluebird',
  'core-decorators',
  'loglevel',
];

module.exports = function gruntfile(grunt) {
  grunt.initConfig({

    browserify: {

      // main app
      portal: {
        src: './web/portal/index.js',
        dest: './dist/portal/index.bundle.js',
        options: {
          external: VENDOR_LIBS,

          // compiler configuration
          configure: function config(b) {

            // babel setup
            b.transform('babelify', { stage: 1 });

            // local library files
            var localLibs = {};
            grunt.file.expand({ cwd: './lib' }, '*').forEach(function(mod) {
              localLibs[mod] = './lib/' + mod;
            });
            b.transform('aliasify', {
              aliases: localLibs,
            });
          },
        },
      },

      // 3rd party libs
      vendor: {
        src: [],
        dest: './dist/portal/vendor.bundle.js',
        options: {
          require: VENDOR_LIBS,
          configure: function config(b) {

            // use the shim on all this shit
            b.transform('browserify-shim', { global: true });
          },
        },
      },

      // common options
      options: {
        browserifyOptions: {
          debug: DEBUG,
        },
      },
    },

    copy: {
      index: {
        src: './web/portal/index.html',
        dest: './dist/portal/index.html',
      },
    },

    less: {
      portal: {
        src: './web/portal/style.less',
        dest: './dist/portal/style.bundle.css',
        options: {
          paths: ['./node_modules', './lib'],
          sourceMap: DEBUG,
          sourceMapFileInline: true,
          outputSourceFiles: true,
        },
      },
    },

  });

  // plugins
  require('load-grunt-tasks')(grunt);

  grunt.registerTask('portal', [
    'browserify:vendor',
    'browserify:portal',
    'copy:index',
    'less:portal',
  ]);
};

