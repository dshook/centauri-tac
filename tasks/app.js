module.exports = function app(grunt)
{
  grunt.registerTask('app', function(target) {
    var options = grunt.option('ct')[target];

    grunt.task.run('compile:' + target);
    grunt.task.run('style:' + target);
    grunt.task.run('static-files:' + target);

    if (options.dev) {
      grunt.task.run('watch');
    }
  });
};
