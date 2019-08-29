//This class contains the parameters that define linear fog, i.e., that grows linearly denser with the distance.
THREE.Fog = function ( color, near, far ) {
	/**
	 * @desc 雾的名字
	 * @default ''
	 * @type {string}
	 */
	this.name = '';
	/**
	 * @desc 雾效颜色
	 * @type {THREE.Color}
	 */
	this.color = new THREE.Color( color );
//The minimum distance to start applying fog. Objects that are less than 'near' units from the active camera won't be affected by fog.
	this.near = ( near !== undefined ) ? near : 1;
//The maximum distance at which fog stops being calculated and applied. 
//Objects that are more than 'far' units away from the active camera won't be affected by fog.
	this.far = ( far !== undefined ) ? far : 1000;

};
/**
 * @desc 雾对象的克隆
 * @returns {THREE.Fog}
 */
THREE.Fog.prototype.clone = function () {

	return new THREE.Fog( this.color.getHex(), this.near, this.far );

};
