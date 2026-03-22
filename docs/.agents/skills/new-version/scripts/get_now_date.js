#!/usr/bin/env node
// 输出当前日期，格式 YYYY-MM-DD
const d = new Date();
const yyyy = d.getFullYear();
const mm = String(d.getMonth() + 1).padStart(2, "0");
const dd = String(d.getDate()).padStart(2, "0");
console.log("当前日期：");
console.log(`${yyyy}-${mm}-${dd}`);

module.exports = function getNowDate() {
  return `${yyyy}-${mm}-${dd}`;
};
