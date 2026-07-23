import { defineStore } from 'pinia'

// options 方式定义
export const useCounterStore = defineStore('counter', {
  state: () => ({
    counter: 0
  }),
  getters: {
    doubleCount: (state) => state.counter * 2
  },
  actions: {
    increment () {
      this.counter++
    }
  }
})

// 若使用 setup 方式定义，需要所有的变量都要返回
