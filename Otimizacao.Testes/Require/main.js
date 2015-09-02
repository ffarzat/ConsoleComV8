
require("types");
var types = exports;

// This core module of AST types captures ES5 as it is parsed today by
// git://github.com/ariya/esprima.git#master.
require("core");

// Feel free to add to or remove from this list of extension modules to
// configure the precise type hierarchy that you need.
//require("es6");
//require("es7");
//require("mozilla");
//require("e4x");
//require("fbharmony");
//require("esprima");
//require("babel");

types.finalize();

exports.Type = types.Type;
exports.builtInTypes = types.builtInTypes;
exports.namedTypes = types.namedTypes;
exports.builders = types.builders;
exports.defineMethod = types.defineMethod;
exports.getFieldNames = types.getFieldNames;
exports.getFieldValue = types.getFieldValue;
exports.eachField = types.eachField;
exports.someField = types.someField;
exports.getSupertypeNames = types.getSupertypeNames;

exports.finalize = types.finalize;

//exports.NodePath = require("nodepath");
//exports.PathVisitor = require("pathvisitor");
//exports.visit = exports.PathVisitor.visit;
//exports.astNodesAreEquivalent = require("equiv");


types = exports;
