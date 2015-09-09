var path = require('path');

module.exports = function staticFiles(grunt)
{
  grunt.loadNpmTasks('grunt-contrib-copy');

  grunt.registerTask('static-files', function(target) {
    var options = grunt.option('ct')[target];

    grunt.config('copy.ct-index-' + target, {
      src: path.join(options.buildSrc, 'index.html'),
      dest: path.join(options.buildTarget, 'index.html'),
    });

    grunt.config('copy.ct-lib-assets-' + target, {
      files: [
        {
          expand: true,
          src: options.libAssets,
          dest: path.join(options.buildTarget, 'lib-assets'),
        },
      ],
    });

    grunt.task.run('copy:ct-index-' + target);
    grunt.task.run('copy:ct-lib-assets-' + target);
  });

};
