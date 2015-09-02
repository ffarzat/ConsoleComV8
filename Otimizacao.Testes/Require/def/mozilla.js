require("core");

require("types");
var types = exports;

var def = types.Type.def;
var or = types.Type.or;


require("shared");
var shared = exports;
var defaults = shared.defaults;
var geq = shared.geq;


def("Function")
    // SpiderMonkey allows expression closures: function(x) x+1
    .field("body", or(def("BlockStatement"), def("Expression")));

def("ForInStatement")
    .build("left", "right", "body", "each")
    .field("each", Boolean, defaults["false"]);

def("ForOfStatement")
    .bases("Statement")
    .build("left", "right", "body")
    .field("left", or(
        def("VariableDeclaration"),
        def("Expression")))
    .field("right", def("Expression"))
    .field("body", def("Statement"));

def("LetStatement")
    .bases("Statement")
    .build("head", "body")
    // TODO Deviating from the spec by reusing VariableDeclarator here.
    .field("head", [def("VariableDeclarator")])
    .field("body", def("Statement"));

def("LetExpression")
    .bases("Expression")
    .build("head", "body")
    // TODO Deviating from the spec by reusing VariableDeclarator here.
    .field("head", [def("VariableDeclarator")])
    .field("body", def("Expression"));

def("GraphExpression")
    .bases("Expression")
    .build("index", "expression")
    .field("index", geq(0))
    .field("expression", def("Literal"));

def("GraphIndexExpression")
    .bases("Expression")
    .build("index")
    .field("index", geq(0));
