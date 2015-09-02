require("es6");

require("types");
var types = exports;

var def = types.Type.def;
var or = types.Type.or;
var builtin = types.builtInTypes;


require("shared");
var shared = exports;
var defaults = shared.defaults;

def("Function")
    .field("async", Boolean, defaults["false"]);

def("SpreadProperty")
    .bases("Node")
    .build("argument")
    .field("argument", def("Expression"));

def("ObjectExpression")
    .field("properties", [or(def("Property"), def("SpreadProperty"))]);

def("SpreadPropertyPattern")
    .bases("Pattern")
    .build("argument")
    .field("argument", def("Pattern"));

def("ObjectPattern")
    .field("properties", [or(
        def("Property"),
        def("PropertyPattern"),
        def("SpreadPropertyPattern")
    )]);

def("AwaitExpression")
    .bases("Expression")
    .build("argument", "all")
    .field("argument", or(def("Expression"), null))
    .field("all", Boolean, defaults["false"]);
