/*
		Fabio Farzat - ffarzat@cos.yfrj.break
		Analisador de nós para javascript
		27/01/2016
*/
//=================================================================================>Globais

var fs = require('fs');
var esprima = require('esprima');
var walk = require( 'esprima-walk' );


//=================================================================================>Inicio execução

if (process.argv.length < 4) {
    console.log('Usage: analyze.js file.js');
    process.exit(1);
}

var filename = process.argv[2];
var csvName = process.argv[3];
//console.log('Reading ' + filename);
//console.log('CSV ' + csvName);
var code = fs.readFileSync(filename);

var typesList= analyzeCode(code);
processResults(typesList, csvName);

console.log('Concluído');


//=================================================================================>Funções

function traverse(node, func) {
    func(node);
    for (var key in node) {
        if (node.hasOwnProperty(key)) {
            var child = node[key];
            if (typeof child === 'object' && child !== null) {

                if (Array.isArray(child)) {
                    child.forEach(function(node) {
                        traverse(node, func);
                    });
                } else {
                    traverse(child, func);
                }
            }
        }
    }
}

function analyzeCode(code) {
	var types = {};
	
    var ast = esprima.parse(code);
	
	walk( ast, function ( node ) { 
									if(isNaN(types[node.type]))
										types[node.type] =1; 
									else
										types[node.type] +=1; 
								 }   
		);
	
	return types;
}

function processResults(results, csvFile) {
	
	var csvContent = "sep=,\n";
	csvContent +="Tipo,Frequencia\n";
	

	for (var key in results) {
		csvContent += key + "," + results[key] + "\n";
	}

	//console.log(csvContent);
	var stream = fs.createWriteStream(csvFile);
	stream.write(csvContent);
	stream.end();
	
}