var path = require('path');

var SRC_DIR = './web';
var DIST_DIR = './dist';

module.exports = function ct(grunt)
{
  grunt.registerMultiTask('ct', function() {
    var options = this.options({
      vendorLibs: [],
      libAssets: [],
    });

    options.target = this.target;

    options.buildSrc = path.join(SRC_DIR, this.target);
    options.buildTarget = path.join(DIST_DIR, this.target);

    // CLI
    options.dev = !!grunt.option('dev');
    options.debug = !grunt.option('release');

    // write out all options
    var opts = grunt.option('ct');
    if (!opts) {
      opts = {};
      grunt.option('ct', opts);
    }
    opts[this.target] = options;
  });
};
