module.exports = function app(grunt)
{
  grunt.registerTask('app', function(target) {
    var options = grunt.option('ct')[target];

    // make sure target is a component in our list
    var components = process.env.COMPONENTS;
    if (!components || !~components.indexOf(target)) {
      grunt.log.writeln(target +
          ' is not in COMPONENTS (' + components + '), skipping build');
      return;
    }

    grunt.task.run('compile:' + target);
    grunt.task.run('style:' + target);
    grunt.task.run('static-files:' + target);

    if (options.dev) {
      grunt.task.run('watch');
    }
  });
};
