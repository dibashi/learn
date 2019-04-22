

var f_shader =
    `
        precision mediump float;

        const vec4 color = vec4(1.0,0.0,0.0,1.0);
        void main() {
            gl_FragColor = color;
        }
    
    `;



var v_shader =
    `
        attribute vec4 a_Position;
        attribute vec4 a_BonesIndex;
        attribute vec4 a_Weight;

        uniform mat4 u_ProjectionMatrix;
        uniform mat4 u_ViewMatrix;
        uniform mat4 u_Bones[2];

        void main() {

            gl_Position = u_ProjectionMatrix * u_ViewMatrix * 
                          (
                            u_Bones[int(a_BonesIndex[0])] * a_Position * a_Weight[0] +
                            u_Bones[int(a_BonesIndex[1])] * a_Position * a_Weight[1] +
                            u_Bones[int(a_BonesIndex[2])] * a_Position * a_Weight[2] +
                            u_Bones[int(a_BonesIndex[3])] * a_Position * a_Weight[3]
                          );
                         
        }
    
    `;

/**
 * 
 * gl_Position = u_ProjectionMatrix * u_ViewMatrix * 
                          (
                            u_Bones[int(a_BonesIndex[0])] * a_Position * a_Weight[0] +
                            u_Bones[int(a_BonesIndex[1])] * a_Position * a_Weight[1] +
                            u_Bones[int(a_BonesIndex[2])] * a_Position * a_Weight[2] +
                            u_Bones[int(a_BonesIndex[3])] * a_Position * a_Weight[3]
                          );
 * 
 */
var gl = null;
var skinProgram = null;

var skinProgramParameter = {};

var i = 0;
function computeBoneMatrices(bones, angle) {
    var m = new Matrix4();
    m.setTranslate(angle*10,0,0);
    // m.rotate(angle*180/Math.PI,0,0,1);
    bones[0].set(m);
    m.rotate(angle*180/Math.PI,0,0,1);
    bones[1].set(m);
}



function main() {


    initGL();
    initProgram();

    setAttribute();
    gl.useProgram(skinProgram);


    var numBones = 2;
    var boneMatrices = [];
    boneMatrices[0] = new Matrix4();
    boneMatrices[1] = new Matrix4();
    for (var i = 0; i < numBones; ++i) {

    }


    function render(time) {

        gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);

        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

        setUniform();


        var t = time * 0.001;
        var angle = Math.sin(t);

        computeBoneMatrices(boneMatrices, angle);

        gl.uniformMatrix4fv(skinProgramParameter.u_BonesLoc0, false, boneMatrices[0].elements);
        gl.uniformMatrix4fv(skinProgramParameter.u_BonesLoc1, false, boneMatrices[1].elements);

        gl.drawElements(gl.LINES, skinProgramParameter.indicesNum, gl.UNSIGNED_BYTE, 0);
        requestAnimationFrame(render);
    }

    requestAnimationFrame(render);
}

function initGL() {
    gl = document.getElementById("webgl").getContext('webgl');
    gl.clearColor(0.3, 0.3, 0.3, 1.0);
    gl.enable(gl.DEPTH_TEST);

}

function initProgram() {


    skinProgram = createProgram(gl, v_shader, f_shader);

    skinProgramParameter.a_PositionLoc = gl.getAttribLocation(skinProgram, 'a_Position');
    skinProgramParameter.a_BonesIndexLoc = gl.getAttribLocation(skinProgram, 'a_BonesIndex');
    skinProgramParameter.a_WeightLoc = gl.getAttribLocation(skinProgram, 'a_Weight');

    skinProgramParameter.u_ProjectionMatrixLoc = gl.getUniformLocation(skinProgram, 'u_ProjectionMatrix');
    skinProgramParameter.u_ViewMatrixLoc = gl.getUniformLocation(skinProgram, 'u_ViewMatrix');
    skinProgramParameter.u_BonesLoc0 = gl.getUniformLocation(skinProgram, 'u_Bones[0]');
    skinProgramParameter.u_BonesLoc1 = gl.getUniformLocation(skinProgram, 'u_Bones[1]');

    skinProgramParameter.positionBuffer = gl.createBuffer();
    skinProgramParameter.weightBuffer = gl.createBuffer();
    skinProgramParameter.bonesIndexBuffer = gl.createBuffer();

    skinProgramParameter.indicesBuffer = gl.createBuffer();
    skinProgramParameter.indicesNum = 0;
    //console.log(skinProgramParameter);
}

function setAttribute() {

    gl.bindBuffer(gl.ARRAY_BUFFER, skinProgramParameter.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        0, 1,
        0, -1,
        2, 1,
        2, -1,
        4, 1,
        4, -1
    ]), gl.STATIC_DRAW);
    gl.vertexAttribPointer(skinProgramParameter.a_PositionLoc, 2, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(skinProgramParameter.a_PositionLoc);


    gl.bindBuffer(gl.ARRAY_BUFFER, skinProgramParameter.bonesIndexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        0, 1, 0, 0,
        0, 1, 0, 0,
        0, 1, 0, 0,
        0, 1, 0, 0,
        0, 1, 0, 0,
        0, 1, 0, 0,
    ]), gl.STATIC_DRAW);
    gl.vertexAttribPointer(skinProgramParameter.a_BonesIndexLoc, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(skinProgramParameter.a_BonesIndexLoc);


    gl.bindBuffer(gl.ARRAY_BUFFER, skinProgramParameter.weightBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
        1, 0, 0, 0,
        1, 0, 0, 0,
        0.3, 0.7, 0, 0,
        0.3, 0.7, 0, 0,
        0, 1, 0, 0,
        0, 1, 0, 0,
        // 1, 0, 0, 0,
        // 1, 0, 0, 0,
        // 1, 0, 0, 0,
        // 1, 0, 0, 0,
        // 1, 0, 0, 0,
        // 1, 0, 0, 0,
    ]), gl.STATIC_DRAW);
    gl.vertexAttribPointer(skinProgramParameter.a_WeightLoc, 4, gl.FLOAT, false, 0, 0);
    gl.enableVertexAttribArray(skinProgramParameter.a_WeightLoc);

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, skinProgramParameter.indicesBuffer);
    var ib = new Uint8Array([
        0, 1,
        0, 2,
        1, 3,
        2, 3,
        2, 4,
        3, 5,
        4, 5,
    ]);

    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, ib, gl.STATIC_DRAW);
    skinProgramParameter.indicesNum = ib.length;
}

function setUniform() {

    var projectMatrix = new Matrix4();
    //projectMatrix.setPerspective(45, gl.canvas.width / gl.canvas.height, 0.1, 100);
    projectMatrix.setOrtho(-10,10,-10,10,0,10);
    gl.uniformMatrix4fv(skinProgramParameter.u_ProjectionMatrixLoc, false, projectMatrix.elements);

    var viewMatrix = new Matrix4();
    viewMatrix.setLookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);
    gl.uniformMatrix4fv(skinProgramParameter.u_ViewMatrixLoc, false, viewMatrix.elements);

}

