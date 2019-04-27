

var v_shader =
    `
        attribute vec4 a_Position;
        attribute vec2 a_Texcoord;
      
        uniform mat4 u_MVPMatrix;
        varying vec2 v_Texcoord;
        varying vec4 v_Color;
        void main() {
            gl_Position = u_MVPMatrix * a_Position;
            v_Texcoord = a_Texcoord;
        }

    `;

var f_shader =
    `
        precision mediump float;

        uniform sampler2D u_Sampler;
        varying vec2 v_Texcoord;
        
        void main() {
            gl_FragColor = texture2D(u_Sampler,v_Texcoord);
        }

    `;

var plane = {};
var cube = {};

var programObj = {};

var OFFSCREEN_WIDTH = 256;
var OFFSCREEN_HEIGHT = 256;

function main() {

    var gl = document.getElementById('webgl').getContext('webgl');

    initShaders(gl, v_shader, f_shader);

    gl.clearColor(0.0, 0.4, 0.4, 1.0);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);

    programObj.positionLoc = gl.getAttribLocation(gl.program, 'a_Position');
    programObj.texcoordLoc = gl.getAttribLocation(gl.program, 'a_Texcoord');
    programObj.colorLoc = gl.getAttribLocation(gl.program, 'a_Color');
    programObj.mvpMatrixLoc = gl.getUniformLocation(gl.program, 'u_MVPMatrix');
    programObj.samplerLoc = gl.getUniformLocation(gl.program, 'u_Sampler');


    programObj.pMatrix = new Matrix4();
    programObj.vMatrix = new Matrix4();
    programObj.mMatrix = new Matrix4();
    programObj.MVPMatrix = new Matrix4();

    createCube(gl);
    createPlane(gl);
    var fbo = initFrameBufferObject(gl);
    function draw() {

        cube.angleCube += 0.1;
        plane.anglePlane+=0.1;
        drawCube(gl, cube, programObj, fbo);
        gl.bindFramebuffer(gl.FRAMEBUFFER,null);
        gl.viewport(0,0,gl.canvas.width,gl.canvas.height);
        gl.clearColor(0.0,0.0,1.0,1.0);
        gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);
        drawPlane(gl, plane, programObj, fbo);
        requestAnimationFrame(draw);
    }

    draw();

}


function drawCube(gl, cube, programObj, fbo) {

    gl.bindFramebuffer(gl.FRAMEBUFFER, fbo);
    gl.viewport(0, 0, OFFSCREEN_WIDTH,OFFSCREEN_HEIGHT);
    gl.clearColor(1.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    programObj.pMatrix.setPerspective(45, OFFSCREEN_WIDTH / OFFSCREEN_HEIGHT, 0.1, 100);
    programObj.vMatrix.setLookAt(0.0, 0.0, 10.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
    programObj.mMatrix.setRotate(cube.angleCube, 0.0, 1.0, 0.0).rotate(cube.angleCube, 1.0, 0.0, 0.0);

    programObj.MVPMatrix.set(programObj.pMatrix).concat(programObj.vMatrix).concat(programObj.mMatrix);

    gl.uniformMatrix4fv(programObj.mvpMatrixLoc, false, programObj.MVPMatrix.elements);
    gl.uniform1i(cube.samplerLoc, cube.textureUnit);

    gl.activeTexture(gl.TEXTURE0 + cube.textureUnit);
    gl.bindTexture(gl.TEXTURE_2D, cube.texture);

    gl.bindBuffer(gl.ARRAY_BUFFER, cube.positionBuffer);
    gl.vertexAttribPointer(programObj.positionLoc, cube.POSITIONS_COMPONENT_NUM, cube.POSITIONS_TYPE, false, 0, 0);
    gl.enableVertexAttribArray(programObj.positionLoc);

    gl.bindBuffer(gl.ARRAY_BUFFER, cube.texcoordBuffer);
    gl.vertexAttribPointer(programObj.texcoordLoc, cube.TEXCOORDS_COMPONENT_NUM, cube.TEXCOORDS_TYPE, false, 0, 0);
    gl.enableVertexAttribArray(programObj.texcoordLoc);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, cube.indicesBuffer);

    gl.drawElements(gl.TRIANGLES, cube.INDICES_LENGTH, cube.INDICES_TYPE, 0);

}


function drawPlane(gl, plane, programObj, fbo) {

    programObj.pMatrix.setPerspective(45, gl.canvas.width / gl.canvas.height, 0.1, 100);
    programObj.vMatrix.setLookAt(0.0, 0.0, 10.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
    programObj.mMatrix.setRotate(plane.anglePlane, 0.0, 1.0, 0.0)//.rotate(cube.anglePlane, 1.0, 0.0, 0.0);

    programObj.MVPMatrix.set(programObj.pMatrix).concat(programObj.vMatrix).concat(programObj.mMatrix);

    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, fbo.texture);

    gl.uniformMatrix4fv(programObj.mvpMatrixLoc, false, programObj.MVPMatrix.elements);
    

    gl.bindBuffer(gl.ARRAY_BUFFER, plane.positionBuffer);
    gl.vertexAttribPointer(programObj.positionLoc, plane.POSITIONS_COMPONENT_NUM, plane.POSITIONS_TYPE, false, 0, 0);
    gl.enableVertexAttribArray(programObj.positionLoc);

    gl.bindBuffer(gl.ARRAY_BUFFER, plane.texcoordBuffer);
    gl.vertexAttribPointer(programObj.texcoordLoc, plane.TEXCOORDS_COMPONENT_NUM, plane.TEXCOORDS_TYPE, false, 0, 0);
    gl.enableVertexAttribArray(programObj.texcoordLoc);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, plane.indicesBuffer);

    gl.drawElements(gl.TRIANGLES, plane.INDICES_LENGTH, plane.INDICES_TYPE, 0);

}


function createPlane(gl) {
    plane.positionBuffer = gl.createBuffer();
    plane.texcoordBuffer = gl.createBuffer();
    plane.indicesBuffer = gl.createBuffer();

    var positions = new Float32Array([

        2.0, 2.0, 2.0, -2.0, 2.0, 2.0, -2.0, -2.0, 2.0, 2.0, -2.0, 2.0,//0 1 2 3

    ]);

    var texcoords = new Float32Array([
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0
    ]);

    var indices = new Uint8Array([

        0, 1, 2, 0, 2, 3
    ]);

    gl.bindBuffer(gl.ARRAY_BUFFER, plane.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);

    plane.POSITIONS_COMPONENT_NUM = 3;
    plane.POSITIONS_TYPE = gl.FLOAT;



    gl.bindBuffer(gl.ARRAY_BUFFER, plane.texcoordBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, texcoords, gl.STATIC_DRAW);

    plane.TEXCOORDS_COMPONENT_NUM = 2;
    plane.TEXCOORDS_TYPE = gl.FLOAT;

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, plane.indicesBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    plane.INDICES_LENGTH = indices.length;
    plane.INDICES_TYPE = gl.UNSIGNED_BYTE;

    plane.anglePlane = 10;


    plane.texture = gl.createTexture();
    plane.textureUnit = 0;

}

function createCube(gl) {

    cube.positionBuffer = gl.createBuffer();
    cube.texcoordBuffer = gl.createBuffer();
    cube.indicesBuffer = gl.createBuffer();

    // Create a cube
    //    v6----- v5
    //   /|      /|
    //  v1------v0|
    //  | |     | |
    //  | |v7---|-|v4
    //  |/      |/
    //  v2------v3
    /**
     * 1.0,1.0,1.0,
     * -1.0,1.0,1.0,
     * -1.0,-1.0,1.0,
     * 1.0,-1.0,1.0,
     * 1.0,-1.0,-1.0,
     * 1.0,1.0,-1.0,
     * -1.0,1.0,-1.0,
     * -1.0,-1.0,-1.0,
     */
    var positions = new Float32Array([

        1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0,//0 1 2 3
        1.0, 1.0, 1.0, 1.0, -1.0, 1.0, 1.0, -1.0, -1.0, 1.0, 1.0, -1.0,//0 3 4 5
        1.0, 1.0, 1.0, 1.0, 1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, 1.0,//0 5 6 1
        -1.0, 1.0, 1.0, -1.0, 1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0,//1 6 7 2
        -1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, -1.0, -1.0, 1.0, //7 4 3 2
        1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0 //4 7 6 5

    ]);



    var texcoords = new Float32Array([
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,
        0.0, 1.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0,
        1.0, 0.0, 1.0, 1.0, 0.0, 1.0, 0.0, 0.0,
        1.0, 1.0, 0.0, 1.0, 0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0,
        0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0
    ]);



    var indices = new Uint8Array([

        0, 1, 2, 0, 2, 3,
        4, 5, 6, 4, 6, 7,
        8, 9, 10, 8, 10, 11,
        12, 13, 14, 12, 14, 15,
        16, 17, 18, 16, 18, 19,
        20, 21, 22, 20, 22, 23

    ]);



    gl.bindBuffer(gl.ARRAY_BUFFER, cube.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);

    cube.POSITIONS_COMPONENT_NUM = 3;
    cube.POSITIONS_TYPE = gl.FLOAT;



    gl.bindBuffer(gl.ARRAY_BUFFER, cube.texcoordBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, texcoords, gl.STATIC_DRAW);

    cube.TEXCOORDS_COMPONENT_NUM = 2;
    cube.TEXCOORDS_TYPE = gl.FLOAT;

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, cube.indicesBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    cube.INDICES_LENGTH = indices.length;
    cube.INDICES_TYPE = gl.UNSIGNED_BYTE;

    cube.angleCube = 0.0;


    cube.texture = gl.createTexture();
    cube.textureUnit = 0;

    var img = new Image();
    img.onload = function () {
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, 1);
        gl.activeTexture(gl.TEXTURE0 + cube.textureUnit);
        gl.bindTexture(gl.TEXTURE_2D, cube.texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, gl.RGB, gl.UNSIGNED_BYTE, img);

        gl.uniform1i(cube.samplerLoc, cube.textureUnit);
    }
    img.src = '../resources/orange.jpg';

}


function initFrameBufferObject(gl) {

    var frameBuffer = gl.createFramebuffer();
   
    var texture = gl.createTexture();
  
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    frameBuffer.texture = texture;

    var depthBuffer = gl.createRenderbuffer();
    
    gl.bindRenderbuffer(gl.RENDERBUFFER, depthBuffer);
    gl.renderbufferStorage(gl.RENDERBUFFER, gl.DEPTH_COMPONENT16, OFFSCREEN_WIDTH, OFFSCREEN_HEIGHT);
    gl.bindFramebuffer(gl.FRAMEBUFFER, frameBuffer);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, texture, 0);
    gl.framebufferRenderbuffer(gl.FRAMEBUFFER, gl.DEPTH_ATTACHMENT, gl.RENDERBUFFER, depthBuffer);


    var error = function () {
        gl.deleteFramebuffer(frameBuffer);
        gl.deleteTexture(texture);
        gl.deleteRenderbuffer(depthBuffer);
    }

    var e = gl.checkFramebufferStatus(gl.FRAMEBUFFER);
    // if (gl.FRAMEBUFFER_COMPLETE !== 0) {
    //     console.log('Frame buffer object is incomplete: ' + e.toString());
    //     return error();
    // }

    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    gl.bindTexture(gl.TEXTURE_2D, null);
    gl.bindRenderbuffer(gl.RENDERBUFFER, null);

    return frameBuffer;
}