
var fs = require('fs');
function IdiomNode(idiom) {
    this.idiom = idiom;
    this.childs = [];
    this.parent = null;
}

function main() {
    var result = [];
    for (var i = 0, j = idioms.length; i < j; i++) {
        var idiom_node = new IdiomNode(idioms[i]);
        generate_tree(idiom_node);


        var strings = traverse_idioms_tree_to_Strings(idiom_node);
        for (var k = 0, l = strings.length; k < l; k++) {
            result.push(strings[k]);
        }
    }

    //按长度排序
    result.sort(function(a,b) {
        if(a.length<b.length) {
            return -1;
        } else if(a.length === b.length) {
            return 0;
        } else {
            return 1;
        }
    });
    var result_json = JSON.stringify(result);
    fs.writeSync('./data.json',result_json);
}

function generate_tree(node) {
    for (var i = 0, j = idioms.length; i < j; i++) {

        if (!inChain(idioms[i], node)) {
            var word = hasSameWord(node.idiom, idioms[i]);

            if (word) {
                if (!node.parent || node.parent.idiom.indexOf(word) == -1) {
                    var newNode = new IdiomNode(idioms[i]);
                    newNode.parent = node;
                    node.childs.push(newNode);

                    generate_tree(newNode);
                }
            }
        }

    }
}

//这俩串有相同的字
function hasSameWord(strA, strB) {
    for (var i = 0, j = strA.length; i < j; i++) {
        var word = strA.charAt(i);
        var index = strB.indexOf(word);
        if (index != -1) {
            return word;
        }
    }
    return '';
}

function inChain(idiom, node) {
    var tempNode = node;
    while (tempNode) {
        if (tempNode.idiom === idiom) {
            return true;
        }
        tempNode = tempNode.parent;
    }

    return false;
}

function traverse_idioms_tree_to_Strings(idiom_node) {

    var results = [];
    var pre = idiom_node.idiom;
    var len = idiom_node.childs.length;
    if (len > 0) {
        for (var i = 0; i < len; i++) {
            var childStrs = traverse_idioms_tree_to_Strings(idiom_node.childs[i]);
            for (var k = 0, l = childStrs.length; k < l; k++) {
                var str = pre + "," + childStrs[k];
                results.push(str);
            }
        }
        return results;
    } else {
        results.push(pre);
        return results;
    }

}

