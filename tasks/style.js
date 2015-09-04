var path = require('path');

module.exports = function style(grunt)
{
  grunt.loadNpmTasks('grunt-contrib-less');

  grunt.registerTask('style', function(target) {
    var options = grunt.option('ct')[target];

    grunt.config('less.ct-' + target, {
      src: path.join(options.buildSrc, 'style.less'),
      dest: path.join(options.buildTarget, 'style.bundle.css'),
      options: {
        paths: ['./node_modules', './lib'],
        sourceMap: options.debug,
        sourceMapFileInline: true,
        outputSourceFiles: true,
      },
    });

    grunt.config('watch.style-' + target, {
      files: [
        path.join(options.buildSrc, '**/*.less'),
        path.join('./lib/**/*.less')
      ],
      tasks: [
        'ct:' + target, 
        'style:' + target
      ],
      options: { livereload: true }
    });

    grunt.task.run('less:ct-' + target);
  });
};
