#!/usr/bin/env node
import { pathToFileURL } from "url";

// 输出当前日期，格式 YYYY-MM-DD
export default function getNowDate() {
  const d = new Date();
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

if (
  process.argv[1] &&
  import.meta.url === pathToFileURL(process.argv[1]).href
) {
  console.log("当前日期：");
  console.log(getNowDate());
}
