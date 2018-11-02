
VSHADER_SOURCE = '';
FSHADER_SOURCE = '';



function main() {
    var canvas = document.getElementById('webgl');
    var gl = canvas.getContext("webgl");
    console.log(gl);
    var program = initShaders(gl,VSHADER_SOURCE,FSHADER_SOURCE);
    
}