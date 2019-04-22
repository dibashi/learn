
// var v_shader =
//     `   
//         attribute vec4 a_position;
//         attribute vec4 a_weight;
//         attribute vec4 a_boneNdx;

//         uniform mat4 projection;
//         uniform mat4 view;
//         uniform mat4 bones[4];

//         void main() {
//             gl_Position = projection * view * 
//                           (bones[int(a_boneNdx[0])] * a_position * a_weight[0] +
//                            bones[int(a_boneNdx[1])] * a_position * a_weight[1] +
//                            bones[int(a_boneNdx[2])] * a_position * a_weight[2] +
//                            bones[int(a_boneNdx[3])] * a_position * a_weight[3]);

//         }


//     `;

var v_shader =
    `   
        attribute vec4 a_position;
        attribute vec4 a_weight;
        attribute vec4 a_boneNdx;

        uniform mat4 projection;
        uniform mat4 view;
        uniform sampler2D boneMatrixTexture;
        uniform float numBones;

        #define ROW0_U ((0.5+0.0)/4.0)
        #define ROW1_U ((0.5+1.0)/4.0)
        #define ROW2_U ((0.5+2.0)/4.0)
        #define ROW3_U ((0.5+3.0)/4.0)

        mat4 getBoneMatrix(float boneNdx) {
            float v = (boneNdx + 0.5) / numBones;
            return mat4(
              texture2D(boneMatrixTexture, vec2(ROW0_U, v)),
              texture2D(boneMatrixTexture, vec2(ROW1_U, v)),
              texture2D(boneMatrixTexture, vec2(ROW2_U, v)),
              texture2D(boneMatrixTexture, vec2(ROW3_U, v)));
          }

        void main() {
            gl_Position = projection * view * 
                          (getBoneMatrix(a_boneNdx[0]) * a_position * a_weight[0] +
                           getBoneMatrix(a_boneNdx[1]) * a_position * a_weight[1] +
                           getBoneMatrix(a_boneNdx[2]) * a_position * a_weight[2] +
                           getBoneMatrix(a_boneNdx[3]) * a_position * a_weight[3]);

        }
    `;



var f_shader =
    `
        precision mediump float;

        uniform vec4 color;

        void main() {
            gl_FragColor = color;
        }
    `;


"use strict";

function main() {
    // Get A WebGL context
    /** @type {HTMLCanvasElement} */
    var canvas = document.getElementById("webgl");
    var gl = canvas.getContext("webgl");
    var ext = gl.getExtension('OES_texture_float');
    if (!ext) {
        return;  // the extension doesn't exist on this device
    }
    gl.clearColor(0.3, 0.3, 0.3, 1.0);
    // compiles and links the shaders, looks up attribute and uniform locations
    var programInfo = webglUtils.createProgramInfo(gl, [v_shader, f_shader]);

    var arrays = {
        position: {
            numComponents: 2,
            data: [
                0, 1,  // 0
                0, -1,  // 1
                2, 1,  // 2
                2, -1,  // 3
                4, 1,  // 4
                4, -1,  // 5
                6, 1,  // 6
                6, -1,  // 7
                8, 1,  // 8
                8, -1,  // 9
            ],
        },
        boneNdx: {
            numComponents: 4,
            data: [
                0, 0, 0, 0,  // 0
                0, 0, 0, 0,  // 1
                0, 1, 0, 0,  // 2
                0, 1, 0, 0,  // 3
                1, 0, 0, 0,  // 4
                1, 0, 0, 0,  // 5
                1, 2, 0, 0,  // 6
                1, 2, 0, 0,  // 7
                2, 0, 0, 0,  // 8
                2, 0, 0, 0,  // 9
            ],
        },
        weight: {
            numComponents: 4,
            data: [
                1, 0, 0, 0,  // 0
                1, 0, 0, 0,  // 1
                .5, .5, 0, 0,  // 2
                .5, .5, 0, 0,  // 3
                1, 0, 0, 0,  // 4
                1, 0, 0, 0,  // 5
                .5, .5, 0, 0,  // 6
                .5, .5, 0, 0,  // 7
                1, 0, 0, 0,  // 8
                1, 0, 0, 0,  // 9
            ],
        },

        indices: {
            numComponents: 2,
            data: [
                0, 1,
                0, 2,
                1, 3,
                2, 3, //
                2, 4,
                3, 5,
                4, 5,
                4, 6,
                5, 7, //
                6, 7,
                6, 8,
                7, 9,
                8, 9,
            ],
        },
    };
    // calls gl.createBuffer, gl.bindBuffer, gl.bufferData
    var bufferInfo = webglUtils.createBufferInfoFromArrays(gl, arrays);

    // 4 matrices, one for each bone
    var numBones = 4;
    var boneArray = new Float32Array(numBones * 16);

    var boneMatrixTexture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, boneMatrixTexture);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    var uniforms = {
        projection: m4.orthographic(-20, 20, -10, 10, -1, 1),
        view: m4.translation(0, 0, 0),
        boneMatrixTexture,
        numBones,
        color: [1, 0, 0, 1],
    };

    // make views for each bone. This lets all the bones
    // exist in 1 array for uploading but as separate
    // arrays for using with the math functions
    var boneMatrices = [];  // the uniform data
    var bones = [];         // the value before multiplying by inverse bind matrix
    var bindPose = [];      // the bind matrix
    for (var i = 0; i < numBones; ++i) {
        boneMatrices.push(new Float32Array(boneArray.buffer, i * 4 * 16, 16));
        bindPose.push(m4.identity());  // just allocate storage
        bones.push(m4.identity());     // just allocate storage
    }

    // rotate each bone by a and simulate a hierarchy
    function computeBoneMatrices(bones, angle) {
        var m = m4.identity();
        m4.zRotate(m, angle, bones[0]);
        m4.translate(bones[0], 4, 0, 0, m);
        m4.zRotate(m, angle, bones[1]);
        m4.translate(bones[1], 4, 0, 0, m);
        m4.zRotate(m, angle, bones[2]);
        // bones[3] is not used
    }

    // compute the initial positions of each matrix
    computeBoneMatrices(bindPose, 0);

    // compute their inverses
    var bindPoseInv = bindPose.map(function (m) {
        return m4.inverse(m);
        return m4.identity();
    });

    function render(time) {
        webglUtils.resizeCanvasToDisplaySize(gl.canvas);
        gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
        gl.clear(gl.COLOR_BUFFER_BIT);
        const aspect = gl.canvas.clientWidth / gl.canvas.clientHeight;
        m4.orthographic(-aspect * 10, aspect * 10, -10, 10, -1, 1, uniforms.projection);

        var t = time * 0.001;
        var angle = Math.sin(t) * 0.8;
        computeBoneMatrices(bones, angle);

        // multiply each by its bindPoseInverse
        bones.forEach(function (bone, ndx) {
            m4.multiply(bone, bindPoseInv[ndx], boneMatrices[ndx]);
        });

        gl.useProgram(programInfo.program);
        // calls gl.bindBuffer, gl.enableVertexAttribArray, gl.vertexAttribPointer
        webglUtils.setBuffersAndAttributes(gl, programInfo, bufferInfo);

        gl.bindTexture(gl.TEXTURE_2D, boneMatrixTexture);
        gl.texImage2D(
            gl.TEXTURE_2D,
            0,         // level
            gl.RGBA,   // internal format
            4,         // width 4 pixels, each pixel has RGBA so 4 pixels is 16 values
            numBones,  // one row per bone
            0,         // border
            gl.RGBA,   // format
            gl.FLOAT,  // type
            boneArray);

        // calls gl.uniformXXX, gl.activeTexture, gl.bindTexture
        webglUtils.setUniforms(programInfo, uniforms);

        // calls gl.drawArrays or gl.drawIndices
        webglUtils.drawBufferInfo(gl, bufferInfo, gl.LINES);

        //drawAxis(uniforms.projection, uniforms.view, bones);

        requestAnimationFrame(render);
    }
    requestAnimationFrame(render);


    // --- ignore below this line - it's not relevant to the exmample and it's kind of a bad example ---

    var axisProgramInfo;
    var axisBufferInfo;
    function drawAxis(projection, view, bones) {
        if (!axisProgramInfo) {
            axisProgramInfo = webglUtils.createProgramInfo(gl, ['vs2', 'fs']);
            axisBufferInfo = webglUtils.createBufferInfoFromArrays(gl, {
                position: {
                    numComponents: 2,
                    data: [
                        0, 0,
                        1, 0,
                    ],
                },
            });
        }

        var uniforms = {
            projection: projection,
            view: view,
        };

        gl.useProgram(axisProgramInfo.program);
        webglUtils.setBuffersAndAttributes(gl, axisProgramInfo, axisBufferInfo);

        for (var i = 0; i < 3; ++i) {
            drawLine(bones[i], 0, [0, 1, 0, 1]);
            drawLine(bones[i], Math.PI * 0.5, [0, 0, 1, 1]);
        }

        function drawLine(mat, angle, color) {
            uniforms.model = m4.zRotate(mat, angle);
            uniforms.color = color;
            webglUtils.setUniforms(axisProgramInfo, uniforms);
            webglUtils.drawBufferInfo(gl, axisBufferInfo, gl.LINES);
        }
    }
}

