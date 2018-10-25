# Copyright 2012, Gregg Tavares.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are
# met:
#
#     * Redistributions of source code must retain the above copyright
# notice, this list of conditions and the following disclaimer.
#     * Redistributions in binary form must reproduce the above
# copyright notice, this list of conditions and the following disclaimer
# in the documentation and/or other materials provided with the
# distribution.
#     * Neither the name of Gregg Tavares. nor the names of his
# contributors may be used to endorse or promote products derived from
# this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
# "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
# LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
# A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
# OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
# SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
# LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
# DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
# THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
# (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
# OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

import markdown
import glob
import os
import re
import sys


class ObjParser(object):
  def __init__(self, filename):
    self.positions = []
    self.normals = []
    self.texcoords = []

    self.out_positions = []
    self.out_normals = []
    self.out_texcoords = []

    file = open(filename, "r")
    lines = file.readlines()
    file.close()

    for line in lines:
      parts = line.split()
      if parts[0] == "v":
        self.positions.append([parts[1], parts[2], parts[3]])
      elif parts[0] == 'vn':
        self.normals.append([parts[1], parts[2], parts[3]])
      elif parts[0] == 'vt':
        self.texcoords.append([parts[1], parts[2]])
      elif parts[0] == 'f':
        for v in parts[1:4]:
          f = v.split("/")
          self.out_positions.append(self.positions[int(f[0]) - 1])
          self.out_texcoords.append(self.texcoords[int(f[1]) - 1])
          self.out_normals.append(self.normals[int(f[2]) - 1])

    print "// positions"
    self.dump(self.out_positions)
    print "// texcoords"
    self.dump(self.out_texcoords)
    print "// normals"
    self.dump(self.out_normals)

  def dump(self, array):
    for e in array:
      print ", ".join(e) + ","

def main (argv):
  o = ObjParser(argv[0])


if __name__ == '__main__':
  main(sys.argv[1:])
