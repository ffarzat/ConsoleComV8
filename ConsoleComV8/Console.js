'use strict';

function console() {
  if (!(this instanceof console)) {
    return new Console();
  }
}

console.prototype.log = function(args, args1, args2) {
  JavascriptHelper.Escrever(args, args1, args2);
};

/*

Console.prototype.info = Console.prototype.log;


Console.prototype.warn = function() {
  this._stderr.write(util.format.apply(this, arguments) + '\n');
};


Console.prototype.error = Console.prototype.warn;


Console.prototype.dir = function(object, options) {
  this._stdout.write(util.inspect(object, util._extend({
    customInspect: false
  }, options)) + '\n');
};


Console.prototype.time = function(label) {
  this._times[label] = Date.now();
};


Console.prototype.timeEnd = function(label) {
  var time = this._times[label];
  if (!time) {
    throw new Error('No such label: ' + label);
  }
  var duration = Date.now() - time;
  this.log('%s: %dms', label, duration);
};


Console.prototype.trace = function trace() {
  // TODO probably can to do this better with V8's debug object once that is
  // exposed.
  var err = new Error;
  err.name = 'Trace';
  err.message = util.format.apply(this, arguments);
  Error.captureStackTrace(err, trace);
  this.error(err.stack);
};


Console.prototype.assert = function(expression) {
  if (!expression) {
    var arr = Array.prototype.slice.call(arguments, 1);
    require('assert').ok(false, util.format.apply(this, arr));
  }
};


module.exports = new Console(process.stdout, process.stderr);
module.exports.Console = Console;
*/