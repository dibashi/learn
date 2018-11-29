"""Convert Wavefront OBJ / MTL files into Three.js (slim models version, to be used with web worker based ascii / binary loader)

-------------------------
How to use this converter
-------------------------

python convert_obj_threejs_slim.py -i infile.obj -o outfile.js [-a center|top|bottom] [-s smooth|flat] [-t ascii|binary] [-d invert|normal]

Notes: 

    - by default:
        converted model will be centered (middle of bounding box goes to 0,0,0)
        use smooth shading (if there were vertex normals in the original model)
        will be in ASCII format
        original model is assumed to use non-inverted transparency / dissolve (0.0 fully transparent, 1.0 fully opaque)
 
    - binary conversion will create two files: 
        outfile.js  (materials)
        outfile.bin (binary buffers)
    
--------------------------------------------------
How to use generated JS file in your HTML document
--------------------------------------------------

    <script type="text/javascript" src="Three.js"></script>
    
    ...
    
    <script type="text/javascript">
        ...
        
        var loader = new THREE.Loader();
        
        // load ascii model
        loader.loadAscii( "Model_slim.js", function( geometry ) { createScene( geometry) }, path_to_textures );

        // load binary model
        loader.loadBinary( "Model_bin.js", function( geometry ) { createScene( geometry) }, path_to_textures );

        function createScene( geometry ) {
            
            var normalizeUVsFlag = 1; // set to 1 if canvas render has missing materials
            var mesh = new THREE.Mesh( geometry, new THREE.MeshFaceMaterial(), normalizeUVsFlag );
            
        }
        
        ...
    </script>
    
-------------------------------------
Parsers based on formats descriptions
-------------------------------------

    http://en.wikipedia.org/wiki/Obj
    http://en.wikipedia.org/wiki/Material_Template_Library
    
-------------------
Current limitations
-------------------

    - for the moment, only diffuse color and texture are used 
      (will need to extend shaders / renderers / materials in Three)
     
    - models can have more than 65,536 vertices,
      but in most cases it will not work well with browsers,
      which currently seem to have troubles with handling
      large JS files
       
    - texture coordinates can be wrong in canvas renderer
      (there is crude normalization, but it doesn't
       work for all cases)
       
    - smoothing can be turned on/off only for the whole mesh

---------------------------------------------- 
How to get proper OBJ + MTL files with Blender
----------------------------------------------

    0. Remove default cube (press DEL and ENTER)
    
    1. Import / create model
    
    2. Select all meshes (Select -> Select All by Type -> Mesh)
    
    3. Export to OBJ (File -> Export -> Wavefront .obj) [*]
        - enable following options in exporter
            Material Groups
            Rotate X90
            Apply Modifiers
            High Quality Normals
            Copy Images
            Selection Only
            Objects as OBJ Objects
            UVs
            Normals
            Materials
            Edges
            
        - select empty folder
        - give your exported file name with "obj" extension
        - click on "Export OBJ" button
        
    4. Your model is now all files in this folder (OBJ, MTL, number of images)
        - this converter assumes all files staying in the same folder,
          (OBJ / MTL files use relative paths)
          
        - for WebGL, textures must be power of 2 sized

    [*] If OBJ export fails (Blender 2.54 beta), patch your Blender installation 
        following instructions here:
            
            http://www.blendernation.com/2010/09/12/blender-2-54-beta-released/
            
------
Author
------
AlteredQualia http://alteredqualia.com

"""

import fileinput
import operator
import random
import os.path
import getopt
import sys
import struct
import math

# #####################################################
# Configuration
# #####################################################
ALIGN = "center"        # center bottom top none
SHADING = "smooth"      # smooth flat 
TYPE = "ascii"          # ascii binary
TRANSPARENCY = "normal" # normal invert

# default colors for debugging (each material gets one distinct color): 
# white, red, green, blue, yellow, cyan, magenta
COLORS = [0xeeeeee, 0xee0000, 0x00ee00, 0x0000ee, 0xeeee00, 0x00eeee, 0xee00ee]

# #####################################################
# Templates
# #####################################################
TEMPLATE_FILE_ASCII = u"""\
// Converted from: %(fname)s
//  vertices: %(nvertex)d
//  faces: %(nface)d 
//  materials: %(nmaterial)d
//
//  Generated with OBJ -> Three.js converter
//  http://github.com/alteredq/three.js/blob/master/utils/exporters/convert_obj_threejs_slim.py


var model = {
    'materials': [%(materials)s],

    'normals': [%(normals)s],

    'vertices': [%(vertices)s],

    'uvs': [%(uvs)s],

    'triangles': [%(triangles)s],
    'triangles_n': [%(triangles_n)s],
    'triangles_uv': [%(triangles_uv)s],
    'triangles_n_uv': [%(triangles_n_uv)s],

    'quads': [%(quads)s],
    'quads_n': [%(quads_n)s],
    'quads_uv': [%(quads_uv)s],
    'quads_n_uv': [%(quads_n_uv)s],

    'end': (new Date).getTime()
    }
    
postMessage( model );
"""

TEMPLATE_FILE_BIN = u"""\
// Converted from: %(fname)s
//  vertices: %(nvertex)d
//  faces: %(nface)d 
//  materials: %(nmaterial)d
//
//  Generated with OBJ -> Three.js converter
//  http://github.com/alteredq/three.js/blob/master/utils/exporters/convert_obj_threejs_slim.py


var model = {
    'materials': [%(materials)s],

    'buffers': '%(buffers)s',

    'end': (new Date).getTime()
    }
    
postMessage( model );
"""

TEMPLATE_VERTEX = "%f,%f,%f"

TEMPLATE_UV_TRI = "%f,%f,%f,%f,%f,%f"
TEMPLATE_UV_QUAD = "%f,%f,%f,%f,%f,%f,%f,%f"

TEMPLATE_TRI = "%d,%d,%d,%d"
TEMPLATE_QUAD = "%d,%d,%d,%d,%d"

TEMPLATE_TRI_UV = "%d,%d,%d,%d,%d,%d,%d"
TEMPLATE_QUAD_UV = "%d,%d,%d,%d,%d,%d,%d,%d,%d"

TEMPLATE_TRI_N = "%d,%d,%d,%d,%d,%d,%d"
TEMPLATE_QUAD_N = "%d,%d,%d,%d,%d,%d,%d,%d,%d"

TEMPLATE_TRI_N_UV = "%d,%d,%d,%d,%d,%d,%d,%d,%d,%d"
TEMPLATE_QUAD_N_UV = "%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d"

TEMPLATE_N = "%f,%f,%f"
TEMPLATE_UV = "%f,%f"

# #####################################################
# Utils
# #####################################################
def file_exists(filename):
    """Return true if file exists and is accessible for reading.
    
    Should be safer than just testing for existence due to links and 
    permissions magic on Unix filesystems.
    
    @rtype: boolean
    """
    
    try:
        f = open(filename, 'r')
        f.close()
        return True
    except IOError:
        return False

    
def get_name(fname):
    """Create model name based of filename ("path/fname.js" -> "fname").
    """
    
    return os.path.basename(fname).split(".")[0]

def bbox(vertices):
    """Compute bounding box of vertex array.
    """
    
    if len(vertices)>0:
        minx = maxx = vertices[0][0]
        miny = maxy = vertices[0][1]
        minz = maxz = vertices[0][2]
        
        for v in vertices[1:]:
            if v[0]<minx:
                minx = v[0]
            elif v[0]>maxx:
                maxx = v[0]
            
            if v[1]<miny:
                miny = v[1]
            elif v[1]>maxy:
                maxy = v[1]

            if v[2]<minz:
                minz = v[2]
            elif v[2]>maxz:
                maxz = v[2]

        return { 'x':[minx,maxx], 'y':[miny,maxy], 'z':[minz,maxz] }
    
    else:
        return { 'x':[0,0], 'y':[0,0], 'z':[0,0] }

def translate(vertices, t):
    """Translate array of vertices by vector t.
    """
    
    for i in xrange(len(vertices)):
        vertices[i][0] += t[0]
        vertices[i][1] += t[1]
        vertices[i][2] += t[2]
        
def center(vertices):
    """Center model (middle of bounding box).
    """
    
    bb = bbox(vertices)
    
    cx = bb['x'][0] + (bb['x'][1] - bb['x'][0])/2.0
    cy = bb['y'][0] + (bb['y'][1] - bb['y'][0])/2.0
    cz = bb['z'][0] + (bb['z'][1] - bb['z'][0])/2.0
    
    translate(vertices, [-cx,-cy,-cz])

def top(vertices):
    """Align top of the model with the floor (Y-axis) and center it around X and Z.
    """
    
    bb = bbox(vertices)
    
    cx = bb['x'][0] + (bb['x'][1] - bb['x'][0])/2.0
    cy = bb['y'][1]
    cz = bb['z'][0] + (bb['z'][1] - bb['z'][0])/2.0
    
    translate(vertices, [-cx,-cy,-cz])
    
def bottom(vertices):
    """Align bottom of the model with the floor (Y-axis) and center it around X and Z.
    """
    
    bb = bbox(vertices)
    
    cx = bb['x'][0] + (bb['x'][1] - bb['x'][0])/2.0
    cy = bb['y'][0] 
    cz = bb['z'][0] + (bb['z'][1] - bb['z'][0])/2.0
    
    translate(vertices, [-cx,-cy,-cz])

def normalize(v):
    """Normalize 3d vector"""
    
    l = math.sqrt(v[0]*v[0] + v[1]*v[1] + v[2]*v[2])
    v[0] /= l
    v[1] /= l
    v[2] /= l

# #####################################################
# MTL parser
# #####################################################
def parse_mtl(fname):
    """Parse MTL file.
    """
    
    materials = {}
    
    for line in fileinput.input(fname):
        chunks = line.split()
        if len(chunks) > 0:
            
            # Material start
            # newmtl identifier
            if chunks[0] == "newmtl" and len(chunks) == 2:
                identifier = chunks[1]
                if not identifier in materials:
                    materials[identifier] = {}

            # Diffuse color
            # Kd 1.000 1.000 1.000
            if chunks[0] == "Kd" and len(chunks) == 4:
                materials[identifier]["col_diffuse"] = [float(chunks[1]), float(chunks[2]), float(chunks[3])]

            # Ambient color
            # Ka 1.000 1.000 1.000
            if chunks[0] == "Ka" and len(chunks) == 4:
                materials[identifier]["col_ambient"] = [float(chunks[1]), float(chunks[2]), float(chunks[3])]

            # Specular color
            # Ks 1.000 1.000 1.000
            if chunks[0] == "Ks" and len(chunks) == 4:
                materials[identifier]["col_specular"] = [float(chunks[1]), float(chunks[2]), float(chunks[3])]

            # Specular coefficient
            # Ns 154.000
            if chunks[0] == "Ns" and len(chunks) == 2:
                materials[identifier]["specular_coef"] = float(chunks[1])

            # Transparency
            # Tr 0.9 or d 0.9
            if (chunks[0] == "Tr" or chunks[0] == "d") and len(chunks) == 2:
                if TRANSPARENCY == "invert":
                    materials[identifier]["transparency"] = 1.0 - float(chunks[1])
                else:
                    materials[identifier]["transparency"] = float(chunks[1])

            # Optical density
            # Ni 1.0
            if chunks[0] == "Ni" and len(chunks) == 2:
                materials[identifier]["optical_density"] = float(chunks[1])

            # Diffuse texture
            # map_Kd texture_diffuse.jpg
            if chunks[0] == "map_Kd" and len(chunks) == 2:
                materials[identifier]["map_diffuse"] = chunks[1]

            # Ambient texture
            # map_Ka texture_ambient.jpg
            if chunks[0] == "map_Ka" and len(chunks) == 2:
                materials[identifier]["map_ambient"] = chunks[1]

            # Specular texture
            # map_Ks texture_specular.jpg
            if chunks[0] == "map_Ks" and len(chunks) == 2:
                materials[identifier]["map_specular"] = chunks[1]

            # Alpha texture
            # map_d texture_alpha.png
            if chunks[0] == "map_d" and len(chunks) == 2:
                materials[identifier]["map_alpha"] = chunks[1]

            # Bump texture
            # map_bump texture_bump.jpg or bump texture_bump.jpg
            if (chunks[0] == "map_bump" or chunks[0] == "bump") and len(chunks) == 2:
                materials[identifier]["map_bump"] = chunks[1]

            # Illumination
            # illum 2
            #
            # 0. Color on and Ambient off
            # 1. Color on and Ambient on
            # 2. Highlight on
            # 3. Reflection on and Ray trace on
            # 4. Transparency: Glass on, Reflection: Ray trace on
            # 5. Reflection: Fresnel on and Ray trace on
            # 6. Transparency: Refraction on, Reflection: Fresnel off and Ray trace on
            # 7. Transparency: Refraction on, Reflection: Fresnel on and Ray trace on
            # 8. Reflection on and Ray trace off
            # 9. Transparency: Glass on, Reflection: Ray trace off
            # 10. Casts shadows onto invisible surfaces
            if chunks[0] == "illum" and len(chunks) == 2:
                materials[identifier]["illumination"] = int(chunks[1])

    return materials
    
# #####################################################
# OBJ parser
# #####################################################
def parse_vertex(text):
    """Parse text chunk specifying single vertex.
    
    Possible formats:
        vertex index
        vertex index / texture index
        vertex index / texture index / normal index
        vertex index / / normal index
    """
    
    v = 0
    t = 0
    n = 0
    
    chunks = text.split("/")
    
    v = int(chunks[0])
    if len(chunks) > 1:
        if chunks[1]:
            t = int(chunks[1])
    if len(chunks) > 2:
        if chunks[2]:
            n = int(chunks[2])
            
    return { 'v':v, 't':t, 'n':n }
    
def parse_obj(fname):
    """Parse OBJ file.
    """
    
    vertices = []
    normals = []
    uvs = []
    
    faces = []
    
    materials = {}
    mcounter = 0
    mcurrent = 0
    
    mtllib = ""
    
    # current face state
    group = 0
    object = 0
    smooth = 0
    
    for line in fileinput.input(fname):
        chunks = line.split()
        if len(chunks) > 0:
            
            # Vertices as (x,y,z) coordinates
            # v 0.123 0.234 0.345
            if chunks[0] == "v" and len(chunks) == 4:
                x = float(chunks[1])
                y = float(chunks[2])
                z = float(chunks[3])
                vertices.append([x,y,z])

            # Normals in (x,y,z) form; normals might not be unit
            # vn 0.707 0.000 0.707
            if chunks[0] == "vn" and len(chunks) == 4:
                x = float(chunks[1])
                y = float(chunks[2])
                z = float(chunks[3])
                normals.append([x,y,z])

            # Texture coordinates in (u,v[,w]) coordinates, w is optional
            # vt 0.500 -1.352 [0.234]
            if chunks[0] == "vt" and len(chunks) >= 3:
                u = float(chunks[1])
                v = float(chunks[2])
                w = 0
                if len(chunks)>3:
                    w = float(chunks[3])
                uvs.append([u,v,w])

            # Face
            if chunks[0] == "f" and len(chunks) >= 4:
                vertex_index = []
                uv_index = []
                normal_index = []
                
                for v in chunks[1:]:
                    vertex = parse_vertex(v)
                    if vertex['v']:
                        vertex_index.append(vertex['v'])
                    if vertex['t']:
                        uv_index.append(vertex['t'])
                    if vertex['n']:
                        normal_index.append(vertex['n'])
                
                faces.append({
                    'vertex':vertex_index, 
                    'uv':uv_index,
                    'normal':normal_index,
                    
                    'material':mcurrent,
                    'group':group, 
                    'object':object, 
                    'smooth':smooth,
                    })
    
            # Group
            if chunks[0] == "g" and len(chunks) == 2:
                group = chunks[1]

            # Object
            if chunks[0] == "o" and len(chunks) == 2:
                object = chunks[1]

            # Materials definition
            if chunks[0] == "mtllib" and len(chunks) == 2:
                mtllib = chunks[1]
                
            # Material
            if chunks[0] == "usemtl" and len(chunks) == 2:
                material = chunks[1]
                if not material in materials:
                    mcurrent = mcounter
                    materials[material] = mcounter
                    mcounter += 1
                else:
                    mcurrent = materials[material]

            # Smooth shading
            if chunks[0] == "s" and len(chunks) == 2:
                smooth = chunks[1]

    return faces, vertices, uvs, normals, materials, mtllib
    
# #####################################################
# Generator
# #####################################################
def generate_vertex(v):
    return TEMPLATE_VERTEX % (v[0], v[1], v[2])
    
def generate_triangle(f):
    v = f['vertex']
    return TEMPLATE_TRI % (v[0]-1, v[1]-1, v[2]-1, 
                           f['material'])

def generate_triangle_uv(f):
    v = f['vertex']
    uv = f['uv']
    return TEMPLATE_TRI_UV % (v[0]-1, v[1]-1, v[2]-1, 
                              f['material'], 
                              uv[0]-1, uv[1]-1, uv[2]-1)

def generate_triangle_n(f):
    v = f['vertex']
    n = f['normal']
    return TEMPLATE_TRI_N % (v[0]-1, v[1]-1, v[2]-1, 
                             f['material'], 
                             n[0]-1, n[1]-1, n[2]-1)

def generate_triangle_n_uv(f):
    v = f['vertex']
    n = f['normal']
    uv = f['uv']
    return TEMPLATE_TRI_N_UV % (v[0]-1, v[1]-1, v[2]-1, 
                                f['material'], 
                                n[0]-1, n[1]-1, n[2]-1, 
                                uv[0]-1, uv[1]-1, uv[2]-1)

def generate_quad(f):
    vi = f['vertex']
    return TEMPLATE_QUAD % (vi[0]-1, vi[1]-1, vi[2]-1, vi[3]-1, 
                            f['material'])

def generate_quad_uv(f):
    v = f['vertex']
    uv = f['uv']
    return TEMPLATE_QUAD_UV % (v[0]-1, v[1]-1, v[2]-1, v[3]-1, 
                               f['material'], 
                               uv[0]-1, uv[1]-1, uv[2]-1, uv[3]-1)

def generate_quad_n(f):
    v = f['vertex']
    n = f['normal']
    return TEMPLATE_QUAD_N % (v[0]-1, v[1]-1, v[2]-1, v[3]-1, 
                              f['material'],
                              n[0]-1, n[1]-1, n[2]-1, n[3]-1)

def generate_quad_n_uv(f):
    v = f['vertex']
    n = f['normal']
    uv = f['uv']
    return TEMPLATE_QUAD_N_UV % (v[0]-1, v[1]-1, v[2]-1, v[3]-1, 
                                 f['material'],
                                 n[0]-1, n[1]-1, n[2]-1, n[3]-1,
                                 uv[0]-1, uv[1]-1, uv[2]-1, uv[3]-1)

def generate_normal(n):
    return TEMPLATE_N % (n[0], n[1], n[2])

def generate_uv(uv):
    return TEMPLATE_UV % (uv[0], 1.0 - uv[1])

# #####################################################
# Materials
# #####################################################
def generate_color(i):
    """Generate hex color corresponding to integer.
    
    Colors should have well defined ordering.
    First N colors are hardcoded, then colors are random 
    (must seed random number  generator with deterministic value 
    before getting colors).
    """
    
    if i < len(COLORS):
        return "0x%06x" % COLORS[i]
    else:
        return "0x%06x" % int(0xffffff * random.random())
        
def value2string(v):
    if type(v)==str and v[0:2] != "0x":
        return '"%s"' % v
    return str(v)
    
def generate_materials(mtl, materials):
    """Generate JS array of materials objects
    
    JS material objects are basically prettified one-to-one 
    mappings of MTL properties in JSON format.
    """
    
    mtl_array = []
    for m in mtl:
        index = materials[m]
        
        # add debug information
        #  materials should be sorted according to how
        #  they appeared in OBJ file (for the first time)
        #  this index is identifier used in face definitions
        mtl[m]['a_dbg_name'] = m
        mtl[m]['a_dbg_index'] = index
        mtl[m]['a_dbg_color'] = generate_color(index)
        
        mtl_raw = ",\n".join(['\t"%s" : %s' % (n, value2string(v)) for n,v in sorted(mtl[m].items())])
        mtl_string = "\t{\n%s\n\t}" % mtl_raw
        mtl_array.append([index, mtl_string])
        
    return ",\n\n".join([m for i,m in sorted(mtl_array)])

def generate_mtl(materials):
    """Generate dummy materials (if there is no MTL file).
    """
    
    mtl = {}
    for m in materials:
        index = materials[m]
        mtl[m] = {
            'a_dbg_name': m,
            'a_dbg_index': index,
            'a_dbg_color': generate_color(index)
        }
    return mtl
    
def generate_materials_string(materials, mtllib):
    """Generate final materials string.
    """

    random.seed(42) # to get well defined color order for materials
    
    # default materials with debug colors for when
    # there is no specified MTL / MTL loading failed,
    # or if there were no materials / null materials
    if not materials:
        materials = { 'default':0 }
    mtl = generate_mtl(materials)
    
    if mtllib:
        # create full pathname for MTL (included from OBJ)
        path = os.path.dirname(infile)
        fname = os.path.join(path, mtllib)
        
        if file_exists(fname):
            # override default materials with real ones from MTL
            # (where they exist, otherwise keep defaults)
            mtl.update(parse_mtl(fname))
        
        else:
            print "Couldn't find [%s]" % fname
    
    return generate_materials(mtl, materials)
    
# #####################################################
# Faces
# #####################################################
def is_triangle_flat(f):
    return len(f['vertex'])==3 and not (f["normal"] and SHADING == "smooth") and not f['uv']
    
def is_triangle_flat_uv(f):
    return len(f['vertex'])==3 and not (f["normal"] and SHADING == "smooth") and len(f['uv'])==3

def is_triangle_smooth(f):
    return len(f['vertex'])==3 and f["normal"] and SHADING == "smooth" and not f['uv']
    
def is_triangle_smooth_uv(f):
    return len(f['vertex'])==3 and f["normal"] and SHADING == "smooth" and len(f['uv'])==3

def is_quad_flat(f):
    return len(f['vertex'])==4 and not (f["normal"] and SHADING == "smooth") and not f['uv']
    
def is_quad_flat_uv(f):
    return len(f['vertex'])==4 and not (f["normal"] and SHADING == "smooth") and len(f['uv'])==4

def is_quad_smooth(f):
    return len(f['vertex'])==4 and f["normal"] and SHADING == "smooth" and not f['uv']

def is_quad_smooth_uv(f):
    return len(f['vertex'])==4 and f["normal"] and SHADING == "smooth" and len(f['uv'])==4

def sort_faces(faces):
    data = {
    'triangles_flat': [],
    'triangles_flat_uv': [],
    'triangles_smooth': [],
    'triangles_smooth_uv': [],
    
    'quads_flat': [],
    'quads_flat_uv': [],
    'quads_smooth': [],
    'quads_smooth_uv': []
    }
    
    for f in faces:
        if is_triangle_flat(f):
            data['triangles_flat'].append(f)
        elif is_triangle_flat_uv(f):
            data['triangles_flat_uv'].append(f)
        elif is_triangle_smooth(f):
            data['triangles_smooth'].append(f)
        elif is_triangle_smooth_uv(f):
            data['triangles_smooth_uv'].append(f)
        
        elif is_quad_flat(f):
            data['quads_flat'].append(f)
        elif is_quad_flat_uv(f):
            data['quads_flat_uv'].append(f)
        elif is_quad_smooth(f):
            data['quads_smooth'].append(f)
        elif is_quad_smooth_uv(f):
            data['quads_smooth_uv'].append(f)

    return data

# #####################################################
# API - ASCII converter
# #####################################################
def convert_ascii(infile, outfile):
    """Convert infile.obj to outfile.js
    
    Here is where everything happens. If you need to automate conversions,
    just import this file as Python module and call this method.
    """
    
    if not file_exists(infile):
        print "Couldn't find [%s]" % infile
        return
        
    faces, vertices, uvs, normals, materials, mtllib = parse_obj(infile)
    
    if ALIGN == "center":
        center(vertices)
    elif ALIGN == "bottom":
        bottom(vertices)
    elif ALIGN == "top":
        top(vertices)
    
    normals_string = ""
    if SHADING == "smooth":
        normals_string = ",".join(generate_normal(n) for n in normals)
        
    sfaces = sort_faces(faces)
    
    text = TEMPLATE_FILE_ASCII % {
    "name"          : get_name(outfile),
    "vertices"      : ",".join(generate_vertex(v) for v in vertices),
    "triangles"     : ",".join(generate_triangle(f) for f in sfaces['triangles_flat']),
    "triangles_n"   : ",".join(generate_triangle_n(f) for f in sfaces['triangles_smooth']),
    "triangles_uv"  : ",".join(generate_triangle_uv(f) for f in sfaces['triangles_flat_uv']),
    "triangles_n_uv": ",".join(generate_triangle_n_uv(f) for f in sfaces['triangles_smooth_uv']),
    "quads"         : ",".join(generate_quad(f) for f in sfaces['quads_flat']),
    "quads_n"       : ",".join(generate_quad_n(f) for f in sfaces['quads_smooth']),
    "quads_uv"      : ",".join(generate_quad_uv(f) for f in sfaces['quads_flat_uv']),
    "quads_n_uv"    : ",".join(generate_quad_n_uv(f) for f in sfaces['quads_smooth_uv']),
    "uvs"           : ",".join(generate_uv(uv) for uv in uvs),
    "normals"       : normals_string,
    
    "materials" : generate_materials_string(materials, mtllib),
    
    "fname"     : infile,
    "nvertex"   : len(vertices),
    "nface"     : len(faces),
    "nmaterial" : len(materials)
    }
    
    out = open(outfile, "w")
    out.write(text)
    out.close()
    
    print "%d vertices, %d faces, %d materials" % (len(vertices), len(faces), len(materials))
        
    
# #############################################################################
# API - Binary converter
# #############################################################################
def convert_binary(infile, outfile):
    """Convert infile.obj to outfile.js + outfile.bin    
    """
    
    if not file_exists(infile):
        print "Couldn't find [%s]" % infile
        return
    
    binfile = get_name(outfile) + ".bin"
    
    faces, vertices, uvs, normals, materials, mtllib = parse_obj(infile)
    
    if ALIGN == "center":
        center(vertices)
    elif ALIGN == "bottom":
        bottom(vertices)
    elif ALIGN == "top":
        top(vertices)    
    
    sfaces = sort_faces(faces)
    
    # ###################
    # generate JS file
    # ###################
    
    text = TEMPLATE_FILE_BIN % {
    "name"       : get_name(outfile),
    
    "materials" : generate_materials_string(materials, mtllib),
    "buffers"   : binfile,
    
    "fname"     : infile,
    "nvertex"   : len(vertices),
    "nface"     : len(faces),
    "nmaterial" : len(materials)
    }
    
    out = open(outfile, "w")
    out.write(text)
    out.close()
    
    # ###################
    # generate BIN file
    # ###################
    
    if SHADING == "smooth":
        nnormals = len(normals)
    else:
        nnormals = 0
        
    buffer = []

    # header
    # ------
    header_bytes  = struct.calcsize('<8s')
    header_bytes += struct.calcsize('<BBBBBBBB')
    header_bytes += struct.calcsize('<IIIIIIIIIII')
    
    # signature
    signature = struct.pack('<8s', 'Three.js')
    
    # metadata (all data is little-endian)
    vertex_coordinate_bytes = 4
    normal_coordinate_bytes = 1
    uv_coordinate_bytes = 4
    
    vertex_index_bytes = 4
    normal_index_bytes = 4
    uv_index_bytes = 4
    material_index_bytes = 2
    
    # header_bytes            unsigned char   1
    
    # vertex_coordinate_bytes unsigned char   1
    # normal_coordinate_bytes unsigned char   1
    # uv_coordinate_bytes     unsigned char   1
    
    # vertex_index_bytes      unsigned char   1
    # normal_index_bytes      unsigned char   1
    # uv_index_bytes          unsigned char   1
    # material_index_bytes    unsigned char   1
    bdata = struct.pack('<BBBBBBBB', header_bytes,
                               vertex_coordinate_bytes, 
                               normal_coordinate_bytes,
                               uv_coordinate_bytes,
                               vertex_index_bytes, 
                               normal_index_bytes,
                               uv_index_bytes,
                               material_index_bytes)

    # nvertices       unsigned int    4
    # nnormals        unsigned int    4
    # nuvs            unsigned int    4
    
    # ntri_flat       unsigned int    4
    # ntri_smooth     unsigned int    4
    # ntri_flat_uv    unsigned int    4
    # ntri_smooth_uv  unsigned int    4
    
    # nquad_flat      unsigned int    4
    # nquad_smooth    unsigned int    4
    # nquad_flat_uv   unsigned int    4
    # nquad_smooth_uv unsigned int    4    
    ndata = struct.pack('<IIIIIIIIIII', len(vertices), 
                               nnormals,
                               len(uvs),
                               len(sfaces['triangles_flat']), 
                               len(sfaces['triangles_smooth']),
                               len(sfaces['triangles_flat_uv']), 
                               len(sfaces['triangles_smooth_uv']),
                               len(sfaces['quads_flat']),
                               len(sfaces['quads_smooth']),
                               len(sfaces['quads_flat_uv']),
                               len(sfaces['quads_smooth_uv']))
    buffer.append(signature)
    buffer.append(bdata)
    buffer.append(ndata)
        
    # 1. vertices
    # ------------
    # x float   4
    # y float   4
    # z float   4
    for v in vertices:
        data = struct.pack('<fff', v[0], v[1], v[2]) 
        buffer.append(data)

    # 2. normals
    # ---------------
    # x signed char 1
    # y signed char 1
    # z signed char 1
    if SHADING == "smooth":
        for n in normals:
            normalize(n)
            data = struct.pack('<bbb', math.floor(n[0]*127+0.5),
                                       math.floor(n[1]*127+0.5),
                                       math.floor(n[2]*127+0.5))
            buffer.append(data)
    
    # 3. uvs
    # -----------
    # u float   4
    # v float   4
    for uv in uvs:
        data = struct.pack('<ff', uv[0], 1.0-uv[1])
        buffer.append(data)
    
    # 4. flat triangles
    # ------------------
    # a unsigned int   4
    # b unsigned int   4
    # c unsigned int   4
    # m unsigned short 2
    for f in sfaces['triangles_flat']:
        vi = f['vertex']
        data = struct.pack('<IIIH', 
                            vi[0]-1, vi[1]-1, vi[2]-1, 
                            f['material'])
        buffer.append(data)

    # 5. smooth triangles
    # -------------------
    # a  unsigned int   4
    # b  unsigned int   4
    # c  unsigned int   4
    # m  unsigned short 2
    # na unsigned int   4
    # nb unsigned int   4
    # nc unsigned int   4
    for f in sfaces['triangles_smooth']:
        vi = f['vertex']
        ni = f['normal']
        data = struct.pack('<IIIHIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, 
                            f['material'], 
                            ni[0]-1, ni[1]-1, ni[2]-1)
        buffer.append(data)

    # 6. flat triangles uv
    # --------------------
    # a  unsigned int    4
    # b  unsigned int    4
    # c  unsigned int    4
    # m  unsigned short  2
    # ua unsigned int    4
    # ub unsigned int    4
    # uc unsigned int    4
    for f in sfaces['triangles_flat_uv']:
        vi = f['vertex']
        ui = f['uv']
        data = struct.pack('<IIIHIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, 
                            f['material'], 
                            ui[0]-1, ui[1]-1, ui[2]-1)
        buffer.append(data)

    # 7. smooth triangles uv
    # ----------------------
    # a  unsigned int    4
    # b  unsigned int    4
    # c  unsigned int    4
    # m  unsigned short  2
    # na unsigned int    4
    # nb unsigned int    4
    # nc unsigned int    4
    # ua unsigned int    4
    # ub unsigned int    4
    # uc unsigned int    4
    for f in sfaces['triangles_smooth_uv']:
        vi = f['vertex']
        ni = f['normal']
        ui = f['uv']
        data = struct.pack('<IIIHIIIIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, 
                            f['material'], 
                            ni[0]-1, ni[1]-1, ni[2]-1,
                            ui[0]-1, ui[1]-1, ui[2]-1)
        buffer.append(data)

    # 8. flat quads
    # ------------------
    # a unsigned int   4
    # b unsigned int   4
    # c unsigned int   4
    # d unsigned int   4
    # m unsigned short 2
    for f in sfaces['quads_flat']:
        vi = f['vertex']
        data = struct.pack('<IIIIH', 
                            vi[0]-1, vi[1]-1, vi[2]-1, vi[3]-1, 
                            f['material'])
        buffer.append(data)
            
    # 9. smooth quads
    # -------------------
    # a  unsigned int   4
    # b  unsigned int   4
    # c  unsigned int   4
    # d  unsigned int   4
    # m  unsigned short 2
    # na unsigned int   4
    # nb unsigned int   4
    # nc unsigned int   4
    # nd unsigned int   4
    for f in sfaces['quads_smooth']:
        vi = f['vertex']
        ni = f['normal']
        data = struct.pack('<IIIIHIIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, vi[3]-1, 
                            f['material'], 
                            ni[0]-1, ni[1]-1, ni[2]-1, ni[3]-1)
        buffer.append(data)
    
    # 10. flat quads uv
    # ------------------
    # a unsigned int   4
    # b unsigned int   4
    # c unsigned int   4
    # d unsigned int   4
    # m unsigned short 2
    # ua unsigned int  4
    # ub unsigned int  4
    # uc unsigned int  4
    # ud unsigned int  4
    for f in sfaces['quads_flat_uv']:
        vi = f['vertex']
        ui = f['uv']
        data = struct.pack('<IIIIHIIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, vi[3]-1, 
                            f['material'],
                            ui[0]-1, ui[1]-1, ui[2]-1, ui[3]-1)
        buffer.append(data)

    # 11. smooth quads uv
    # -------------------
    # a  unsigned int   4
    # b  unsigned int   4
    # c  unsigned int   4
    # d  unsigned int   4
    # m  unsigned short 2
    # na unsigned int   4
    # nb unsigned int   4
    # nc unsigned int   4
    # nd unsigned int   4
    # ua unsigned int   4
    # ub unsigned int   4
    # uc unsigned int   4
    # ud unsigned int   4
    for f in sfaces['quads_smooth_uv']:
        vi = f['vertex']
        ni = f['normal']
        ui = f['uv']
        data = struct.pack('<IIIIHIIIIIIII', 
                            vi[0]-1, vi[1]-1, vi[2]-1, vi[3]-1, 
                            f['material'], 
                            ni[0]-1, ni[1]-1, ni[2]-1, ni[3]-1,
                            ui[0]-1, ui[1]-1, ui[2]-1, ui[3]-1)
        buffer.append(data)

    path = os.path.dirname(outfile)
    fname = os.path.join(path, binfile)

    out = open(fname, "wb")
    out.write("".join(buffer))
    out.close()

# #############################################################################
# Helpers
# #############################################################################
def usage():
    print "Usage: %s -i filename.obj -o filename.js [-a center|top|bottom] [-s flat|smooth] [-t binary|ascii] [-d invert|normal]" % os.path.basename(sys.argv[0])
        
# #####################################################
# Main
# #####################################################
if __name__ == "__main__":
    
    # get parameters from the command line
    try:
        opts, args = getopt.getopt(sys.argv[1:], "hi:o:a:s:t:d:", ["help", "input=", "output=", "align=", "shading=", "type=", "dissolve="])
    
    except getopt.GetoptError:
        usage()
        sys.exit(2)
        
    infile = outfile = ""
    
    for o, a in opts:
        if o in ("-h", "--help"):
            usage()
            sys.exit()
        
        elif o in ("-i", "--input"):
            infile = a

        elif o in ("-o", "--output"):
            outfile = a

        elif o in ("-a", "--align"):
            if a in ("top", "bottom", "center"):
                ALIGN = a

        elif o in ("-s", "--shading"):
            if a in ("flat", "smooth"):
                SHADING = a
                
        elif o in ("-t", "--type"):
            if a in ("binary", "ascii"):
                TYPE = a

        elif o in ("-d", "--dissolve"):
            if a in ("normal", "invert"):
                TRANSPARENCY = a

    if infile == "" or outfile == "":
        usage()
        sys.exit(2)
    
    print "Converting [%s] into [%s] ..." % (infile, outfile)
    
    if TYPE == "ascii":
        convert_ascii(infile, outfile)
    elif TYPE == "binary":
        convert_binary(infile, outfile)
    