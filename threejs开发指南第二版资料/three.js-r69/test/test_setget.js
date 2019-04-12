

function  test_setget(x) {
    this._x = x;
}

test_setget.prototype = {

    constructor:test_setget,

    get x() {
        console.log("get方法被调用了");
        return this._x;
    },

    set x(value) {
        console.log("set方法被调用了");
        this._x = value;
    }

}