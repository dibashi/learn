/**
 * @author mrdoob / http://mrdoob.com/
 */

/**
	if (material instanceof THREE.MultiMaterial) {

	var groups = geometry.groups;
	var materials = material.materials;

	for (var i = 0, l = groups.length; i < l; i++) {

		var group = groups[i];
		var groupMaterial = materials[group.materialIndex];

		if (groupMaterial.visible === true) {

			pushRenderItem(object, geometry, groupMaterial, _vector3.z, group);

		}

	}

	} else {

	pushRenderItem(object, geometry, material, _vector3.z, null);

	}
 */
//从WebGLRenderer源码可以看到，似乎是按组来分配材质的，所以又引出了组的概念，稍后研究

THREE.MultiMaterial = function (materials) {

	this.uuid = THREE.Math.generateUUID();

	this.type = 'MultiMaterial';

	this.materials = materials instanceof Array ? materials : [];

	this.visible = true;

};

THREE.MultiMaterial.prototype = {

	constructor: THREE.MultiMaterial,

	toJSON: function (meta) {

		var output = {
			metadata: {
				version: 4.2,
				type: 'material',
				generator: 'MaterialExporter'
			},
			uuid: this.uuid,
			type: this.type,
			materials: []
		};

		var materials = this.materials;

		for (var i = 0, l = materials.length; i < l; i++) {

			var material = materials[i].toJSON(meta);
			delete material.metadata;

			output.materials.push(material);

		}

		output.visible = this.visible;

		return output;

	},

	clone: function () {

		var material = new this.constructor();

		for (var i = 0; i < this.materials.length; i++) {

			material.materials.push(this.materials[i].clone());

		}

		material.visible = this.visible;

		return material;

	}

};
