const IronTree = require('@denq/iron-tree')

const defaultOptions = {
  key_id: 'id',
  key_parent: 'parentId',
  key_children: 'children',
  empty_children: false
}

module.exports = class LTT {
  constructor(list, options = {}) {
    this.options = Object.assign({}, defaultOptions, options)

    const { key_id, key_parent } = options

    // 生成根节点
    const tree = new IronTree({ [key_id]: 'root' })
    // 添加下一级：没有 parentId 或者 找不到上一级都放到 root 下面
    const others = []
    for (const item of list) {
      const parentItem = list.find(ls => ls[key_id] === item[key_parent])
      if (!parentItem) {
        // 添加到根
        tree.add(() => {
          return true
        }, item)
      } else {
        others.push(item)
      }
    }

    others.forEach((item, index) => {
      tree.add(parentNode => {
        return parentNode.get(key_id) === item[key_parent]
      }, item)
    })

    this.tree = tree
  }

  sort(criteria) {
    this.tree.sort(criteria)
  }

  // 获取树列表
  GetTree() {
    const { key_child, empty_children } = this.options
    return this.tree.toJson({
      key_children: key_child,
      empty_children
    })[key_child]
  }

  GetCurentAndSub(id) {
    const targetNode = this.tree.contains(currentNode => {
      return currentNode.get('_id') === id
    })

    console.log('GetCurentAndSub:', targetNode)

    if (!targetNode) return []

    // 遍历所有的 content
    function traversal(node) {
      const results = [node.content]
      if (!node.children || node.children.length === 0) {
        return results
      }

      for (const child of node.children) {
        results.push(...traversal(child))
      }

      return results
    }

    return traversal(targetNode)
  }
}
