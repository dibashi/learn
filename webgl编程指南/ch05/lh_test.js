const canvas = document.getElementById('canvas');

const gl = canvas.getContext('webgl');

const extensions = {
    SRGB: gl.getExtension("EXT_SRGB"),
    TEX_LOD: gl.getExtension("EXT_shader_texture_lod")
};

const vs = `
attribute vec2 aPosition;
void main() {
  gl_Position = vec4(aPosition, 0.0, 1.0);
}`;
const fs = `
#extension GL_EXT_shader_texture_lod: enable

precision mediump float;
uniform vec2 uResolution;
uniform float uScale;
uniform sampler2D uTexture;
uniform int uPostProcess;
uniform int uExtLod;
uniform float uExtLodLevel;

void main() {
    vec2 st = gl_FragCoord.xy / uResolution;
    st = (st - 0.5 )/ uScale + 0.5;

    if(uExtLod == 1){
        gl_FragColor = texture2DLodEXT(uTexture, st, uExtLodLevel);
    }else{
        gl_FragColor = texture2D(uTexture, st);
    }

    if(uPostProcess == 1){
        gl_FragColor = vec4(pow(gl_FragColor.rgb, vec3(1.0/2.2)), 1.0);
    }else if(uPostProcess == 2){
        gl_FragColor = vec4(pow(gl_FragColor.rgb, vec3(2.2)), 1.0);
    }
}`;

gl.clearColor(0.0, 0.0, 0.0, 1.0);

const program = createShaderProgram(vs, fs, gl);
gl.useProgram(program);

const vBuffer = createBuffer(new Float32Array([-1, -1, -1, 1, 1, 1, 1, -1]), gl);
const eBuffer = createElementsBuffer(new Uint16Array([0, 1, 2, 0, 2, 3]), gl);

gl.bindBuffer(gl.ARRAY_BUFFER, vBuffer);
const posloc = gl.getAttribLocation(program, 'aPosition');
gl.vertexAttribPointer(posloc, 2, gl.FLOAT, false, 0, 0);
gl.enableVertexAttribArray(posloc);

gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, eBuffer);

const resLoc = gl.getUniformLocation(program, 'uResolution');
gl.uniform2fv(resLoc, new Float32Array([512, 512]));

const sclLoc = gl.getUniformLocation(program, 'uScale');
const sampLoc = gl.getUniformLocation(program, 'uTexture');
const ppLoc = gl.getUniformLocation(program, 'uPostProcess');
const elLoc = gl.getUniformLocation(program, 'uExtLod');
const ellLoc = gl.getUniformLocation(program, 'uExtLodLevel');

gl.activeTexture(gl.TEXTURE0);
gl.uniform1i(sampLoc, 0);

loadImages(function (imageLena_512, imageLena_300, imageUV_256) {

    var data = {
        flipY: true,
        scale: 1,
        size: '512',
        wrap: 'CLAMP',
        SRGB: false,
        postProcess: 'no-operation',
        MAG_FILTER: 'NEAREST',
        MIN_FILTER: 'NEAREST',
        generateMipMap: false,
        customMipmap: false,
        extLod: false,
        extLodLevel: 0
    };
    const gui = new dat.GUI({
        autoplace: false,
        width: 350
    });
    gui.add(data, 'flipY').onChange(recreateTexture);
    gui.add(data, 'scale', 0, 2);
    gui.add(data, 'size', [512, 300]).onChange(recreateTexture);
    gui.add(data, 'wrap', ['CLAMP', 'REPEAT']);
    gui.add(data, 'SRGB').onChange(recreateTexture);
    gui.add(data, 'postProcess', ['no-operation', 'c^(1.0/2.2)', 'c^2.2']);
    gui.add(data, 'MAG_FILTER', ['LINEAR', 'NEAREST']).onChange(recreateTexture);
    gui.add(data, 'MIN_FILTER', [
        'LINEAR', 'NEAREST',
        'NEAREST_MIPMAP_NEAREST', 'LINEAR_MIPMAP_NEAREST',
        'NEAREST_MIPMAP_LINEAR', 'LINEAR_MIPMAP_LINEAR'
    ]).onChange(recreateTexture);
    gui.add(data, 'generateMipMap').onChange(recreateTexture);
    gui.add(data, 'extLod');
    gui.add(data, 'extLodLevel', 0, 5);
    gui.add(data, 'customMipmap').onChange(recreateTexture);

    let texture;
    recreateTexture();

    function render() {

        // scale
        gl.uniform1fv(sclLoc, new Float32Array([Math.pow(data.scale, 2)]));

        if (data.wrap === 'CLAMP') {
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        } else {
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.REPEAT);
        }

        if (data.postProcess === 'no-operation') {
            gl.uniform1iv(ppLoc, new Uint16Array([0]));
        } else if (data.postProcess === 'c^(1.0/2.2)') {
            gl.uniform1iv(ppLoc, new Uint16Array([1]));
        } else if (data.postProcess === 'c^2.2') {
            gl.uniform1iv(ppLoc, new Uint16Array([2]))
        }

        if (data.extLod) {
            gl.uniform1iv(elLoc, new Uint16Array([1]));
            gl.uniform1fv(ellLoc, new Float32Array([data.extLodLevel]));
        } else {
            gl.uniform1iv(elLoc, new Uint16Array([2]));
        }

        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);
        requestAnimationFrame(render);
    }
    render();

    function recreateTexture() {
        if (texture) {
            gl.deleteTexture(texture);
        }
        const image = data.size === '512' ? imageLena_512 : imageLena_300;
        texture = createTextureImage(image, {
            flipY: data.flipY,
            sRGB: data.SRGB,
            magFilter: data.MAG_FILTER,
            minFilter: data.MIN_FILTER,
            generateMipMap: data.generateMipMap,
            customMipMap: data.customMipmap ? imageUV_256 : null
        }, gl);
        gl.bindTexture(gl.TEXTURE_2D, texture);
    }
});



function createShaderProgram(vShaderSource, fShaderSource, gl) {

    function loadShader(gl, type, source) {
        var shader = gl.createShader(type);
        gl.shaderSource(shader, source);
        gl.compileShader(shader);
        if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
            throw 'An error occurred compiling the shaders: ' + gl.getShaderInfoLog(shader);
        }
        return shader;
    }

    const fShader = loadShader(gl, gl.FRAGMENT_SHADER, fShaderSource);
    const vShader = loadShader(gl, gl.VERTEX_SHADER, vShaderSource);

    const program = gl.createProgram();
    gl.attachShader(program, vShader);
    gl.attachShader(program, fShader);
    gl.linkProgram(program);

    if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
        throw 'Unable to initialize the shader program: ' + gl.getProgramInfoLog(program);
    }

    return program;
}

function createBuffer(value, gl) {

    const buffer = gl.createBuffer();

    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ARRAY_BUFFER, value, gl.STATIC_DRAW);
    gl.bindBuffer(gl.ARRAY_BUFFER, null);

    return buffer;
}

function createElementsBuffer(value, gl) {

    const buffer = gl.createBuffer();

    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, value, gl.STATIC_DRAW);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, null);

    return buffer;
}

function createTextureImage(image, opts, gl) {

    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, opts.flipY);

    const texture = gl.createTexture();

    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, texture);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    const format = opts.sRGB && extensions.SRGB ? extensions.SRGB.SRGB_ALPHA_EXT : gl.RGBA;

    gl.texImage2D(gl.TEXTURE_2D, 0, format, format, gl.UNSIGNED_BYTE, image);

    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl[opts.magFilter]);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl[opts.minFilter]);

    if (opts.generateMipMap) {
        gl.generateMipmap(gl.TEXTURE_2D);
    }

    if (opts.customMipMap) {
        gl.texImage2D(gl.TEXTURE_2D, 1, format, format, gl.UNSIGNED_BYTE, opts.customMipMap);
    }

    gl.bindTexture(gl.TEXTURE_2D, null);
    return texture;
}

function loadImage(src, callback) {
    const image = new Image();
    image.crossOrigin = true;
    image.onload = function () { callback(image) };
    image.src = src;
}

function loadImages(callback) {

    const imageLenaUrl_512 = '//gw.alicdn.com/tfs/TB1I3iQtHPpK1RjSZFFXXa5PpXa-512-512.png';
    const imageLenaUrl_300 = '//gw.alicdn.com/tfs/TB12hIvtHvpK1RjSZPiXXbmwXXa-300-300.png';
    const imageUVUrl_256 = '//gw.alicdn.com/tfs/TB1TfostQvoK1RjSZFNXXcxMVXa-256-256.png';

    loadImage(imageLenaUrl_512, function (imageLena_512) {
        loadImage(imageLenaUrl_300, function (imageLena_300) {
            loadImage(imageUVUrl_256, function (imageUV_256) {
                callback(imageLena_512, imageLena_300, imageUV_256);
            })
        })
    })
}