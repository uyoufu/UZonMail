#!/usr/bin/env node
/**
 * update_version_zh.js
 *
 * 根据输入的版本 markdown 内容，更新或新增 docs/downloads.md 中的版本记录：
 *   - 版本已存在 → 覆盖更新
 *   - 版本不存在 → 插入为最新版本（front matter 之后第一条）
 *
 * 输入方式（二选一）：
 *   1. stdin 管道（推荐，适合多行内容）
 *      printf '## 0.20.1\n> 更新时期：...\n' | node update_version_zh.js
 *   2. 第一个命令行参数（字面 \n 会被自动转义）
 *      node update_version_zh.js "## 0.20.1\n> 更新时期：..."
 *
 * 必要格式要求：
 *   - 版本号标题：## X.X.X
 *   - 更新时期：  > 更新时期：YYYY-MM-DD
 *   - 下载地址：  ### 下载地址
 */

"use strict";

const fs = require("fs");
const path = require("path");

// 目标文件：从脚本位置向上 4 级到工作区根，再进入 docs/
const TARGET_FILE = path.resolve(__dirname, "../../../../docs/downloads.md");

// ── 校验 ──────────────────────────────────────────────────────────────────────

function validateEnvironment() {
  if (!fs.existsSync(TARGET_FILE)) {
    console.error(`[错误] 目标文件不存在：${TARGET_FILE}`);
    console.error("[提示] 请确认脚本与工作区目录结构是否正确。");
    process.exit(1);
  }
}

function validateInput(content) {
  const errors = [];

  if (!/^## \d+\.\d+\.\d+/m.test(content)) {
    errors.push("缺少版本号标题（格式：## X.X.X）");
  }
  if (!/^> 更新时期：\d{4}-\d{2}-\d{2}/m.test(content)) {
    errors.push("缺少更新时期（格式：> 更新时期：YYYY-MM-DD）");
  }
  if (!/^### 下载地址/m.test(content)) {
    errors.push("缺少下载地址章节（格式：### 下载地址）");
  }

  if (errors.length > 0) {
    console.error("[错误] 输入格式验证失败：");
    errors.forEach((e) => console.error(`  - ${e}`));
    process.exit(1);
  }
}

// ── 工具函数 ──────────────────────────────────────────────────────────────────

function extractVersion(content) {
  const m = content.match(/^## (\d+\.\d+\.\d+)/m);
  return m ? m[1] : null;
}

/**
 * 检查文件中是否已存在精确匹配的版本标题行（避免 0.20.1 匹配 0.20.10）。
 */
function versionExists(fileContent, version) {
  const escaped = version.replace(/\./g, "\\.");
  return new RegExp(`(^|\n)## ${escaped}(\n|$)`).test(fileContent);
}

/**
 * 核心处理：将文件按版本块分割，替换或插入目标版本，再合并回字符串。
 *
 * 文件结构：
 *   [front matter]\n
 *   \n
 *   ## X.X.X\n    ← 版本块 1
 *   ...\n
 *   \n             ← 块间空行的第一个 \n 保留在块末
 *   ## X.X.X\n    ← 版本块 2（由 split 的 \n 分隔）
 *   ...
 *
 * 用 /\n(?=## )/ 分割：每个版本块以 "## " 开头，前导 \n 被消耗；
 * 用 "\n" 合并时，相邻块之间恰好补回一个 \n，与块末尾的 \n 合成空行。
 */
function processVersionUpdate(fileContent, version, entry) {
  // 按紧邻 ## 的换行符分割
  const parts = fileContent.split(/\n(?=## )/);
  // parts[0] = front matter（以 \n 结尾）
  // parts[1..N] = 各版本块（以 "## " 开头，末尾含一个 \n）

  const headerLine = `## ${version}\n`;
  let found = false;

  const newParts = parts.map((part) => {
    // 精确匹配版本块首行，避免子版本号误判（如 0.20.1 vs 0.20.10）
    if (part === `## ${version}` || part.startsWith(headerLine)) {
      found = true;
      return entry; // entry 末尾已含 \n
    }
    return part;
  });

  if (!found) {
    // 找到第一个以 ## 开头的块（index > 0），在其前插入
    const firstVersionIdx = newParts.findIndex(
      (p, i) => i > 0 && p.startsWith("## "),
    );
    if (firstVersionIdx === -1) {
      // 文件中无任何版本，追加到末尾
      newParts.push(entry);
    } else {
      newParts.splice(firstVersionIdx, 0, entry);
    }
  }

  return newParts.join("\n");
}

// ── stdin 读取 ────────────────────────────────────────────────────────────────

function readStdin() {
  return new Promise((resolve) => {
    if (process.stdin.isTTY) {
      // 非管道调用，不等待 stdin
      resolve("");
      return;
    }
    let data = "";
    process.stdin.setEncoding("utf8");
    process.stdin.on("data", (chunk) => {
      data += chunk;
    });
    process.stdin.on("end", () => resolve(data));
    process.stdin.on("error", () => resolve(""));
  });
}

// ── 主流程 ────────────────────────────────────────────────────────────────────

async function main() {
  validateEnvironment();

  let input = "";

  if (process.argv[2]) {
    // 命令行参数：将字面 \n 转为真实换行符
    input = process.argv[2].replace(/\\n/g, "\n");
  } else {
    input = await readStdin();
  }

  input = input.trim();

  if (!input) {
    console.error("[错误] 未接收到版本内容。");
    console.error("用法：");
    console.error(
      "  printf '## 0.20.1\\n> 更新时期：2025-12-10\\n...' | node update_version_zh.js",
    );
    console.error(
      '  node update_version_zh.js "## 0.20.1\\n> 更新时期：2025-12-10\\n..."',
    );
    process.exit(1);
  }

  validateInput(input);

  const version = extractVersion(input);
  if (!version) {
    console.error("[错误] 无法从输入中提取版本号。");
    process.exit(1);
  }

  const fileContent = fs.readFileSync(TARGET_FILE, "utf8");
  const isUpdate = versionExists(fileContent, version);

  // 确保 entry 末尾恰好一个换行（split/join 结构需要）
  const entry = input.trim() + "\n";

  const newContent = processVersionUpdate(fileContent, version, entry);
  fs.writeFileSync(TARGET_FILE, newContent, "utf8");

  if (isUpdate) {
    console.log(`[成功] 已更新版本 ${version} 的记录。`);
  } else {
    console.log(`[成功] 已新增版本 ${version} 的记录（插入为最新版本）。`);
  }
  console.log(`[文件] ${TARGET_FILE}`);
}

main().catch((err) => {
  console.error("[错误] 执行失败：" + err.message);
  process.exit(1);
});
