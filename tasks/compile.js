var path = require('path');

module.exports = function compile(grunt) {
  grunt.loadNpmTasks('grunt-browserify');
  grunt.loadNpmTasks('grunt-eslint');

  grunt.registerTask('compile', function(target) {
    var options = grunt.option('ct')[target];
    grunt.task.run('compile-config:' + target);
    grunt.task.run('compile-run:' + target);
  });

  grunt.registerTask('compile-run', function(target) {
    var options = grunt.option('ct')[target];

    grunt.task.run('browserify:ct-index-' + target);
    grunt.task.run('browserify:ct-vendor-' + target);
  });

  grunt.registerTask('compile-config', function(target) {
    var options = grunt.option('ct')[target];

    // main app
    grunt.config('browserify.ct-index-' + target, {
      src: path.join(options.buildSrc, 'index.js'),
      dest: path.join(options.buildTarget, 'index.bundle.js'),
      options: {
        external: options.vendorLibs,

        browserifyOptions: {
          debug: options.debug,
        },

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
    });

    // 3rd party libs
    grunt.config('browserify.ct-vendor-' + target, {
      src: [],
      dest: path.join(options.buildTarget, 'vendor.bundle.js'),
      options: {
        require: options.vendorLibs,

        browserifyOptions: {
          debug: options.debug,
        },

        configure: function config(b) {

          // use the shim on all this shit
          b.transform('browserify-shim', { global: true });
        },
      },
    });

    // Only watch for the client
    grunt.config('watch.compile-' + target, {
      files: [
        './lib/**/*.{js,json,html}',
        path.join(options.buildSrc, '**/*.{js,json,html}'),
      ],
      tasks: [
        'ct:' + target,
        'compile-config:' + target,
        'browserify:ct-index-' + target,
      ],
      options: { livereload: true },
    });
  });

};
