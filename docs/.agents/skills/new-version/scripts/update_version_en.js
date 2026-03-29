#!/usr/bin/env node
/**
 * update_version_en.js
 *
 * Updates or inserts a version entry in docs/en/downloads.md:
 *   - Version already exists → overwrite/update
 *   - Version does not exist → insert as the latest version (first entry after front matter)
 *
 * Input methods (choose one):
 *   1. stdin pipe (recommended for multi-line content)
 *      printf '## 0.20.1\n> Release Date: ...\n' | node update_version_en.js
 *   2. First command-line argument (literal \n will be unescaped automatically)
 *      node update_version_en.js "## 0.20.1\n> Release Date: ..."
 *
 * Required format:
 *   - Version title:  ## X.X.X
 *   - Release date:   > Release Date: YYYY-MM-DD
 *   - Downloads:      ### Downloads
 */

"use strict";

import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Target file: 4 levels up from script location to workspace root, then into docs/en/
const TARGET_FILE = path.resolve(__dirname, "../../../../docs/en/downloads.md");

// ── Validation ────────────────────────────────────────────────────────────────

function validateEnvironment() {
  if (!fs.existsSync(TARGET_FILE)) {
    console.error(`[Error] Target file not found: ${TARGET_FILE}`);
    console.error(
      "[Hint] Please verify the script and workspace directory structure.",
    );
    process.exit(1);
  }
}

function validateInput(content) {
  const errors = [];

  if (!/^## \d+\.\d+\.\d+/m.test(content)) {
    errors.push("Missing version title (format: ## X.X.X)");
  }
  if (!/^> Release Date:[ ]?\d{4}-\d{2}-\d{2}/m.test(content)) {
    errors.push("Missing release date (format: > Release Date: YYYY-MM-DD)");
  }
  if (!/^### Downloads/m.test(content)) {
    errors.push("Missing downloads section (format: ### Downloads)");
  }

  if (errors.length > 0) {
    console.error("[Error] Input validation failed:");
    errors.forEach((e) => console.error(`  - ${e}`));
    process.exit(1);
  }
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function extractVersion(content) {
  const m = content.match(/^## (\d+\.\d+\.\d+)/m);
  return m ? m[1] : null;
}

/**
 * Checks whether an exact version title line already exists in the file,
 * avoiding partial matches (e.g. 0.20.1 matching 0.20.10).
 */
function versionExists(fileContent, version) {
  const escaped = version.replace(/\./g, "\\.");
  return new RegExp(`(^|\n)## ${escaped}(\n|$)`).test(fileContent);
}

/**
 * Core logic: split the file into version blocks, replace or insert the target
 * version, then re-join into a single string.
 *
 * File structure:
 *   [front matter]\n
 *   \n
 *   ## X.X.X\n    ← version block 1
 *   ...\n
 *   \n             ← trailing \n kept inside the block
 *   ## X.X.X\n    ← version block 2
 *   ...
 *
 * Split on /\n(?=## )/: each version block starts with "## "; the leading \n
 * is consumed. Joining with "\n" restores the separator, giving a blank line
 * between adjacent blocks (block tail \n + join \n).
 */
function processVersionUpdate(fileContent, version, entry) {
  const parts = fileContent.split(/\n(?=## )/);
  // parts[0] = front matter (ends with \n)
  // parts[1..N] = version blocks (start with "## ", end with \n)

  const headerLine = `## ${version}\n`;
  let found = false;

  const newParts = parts.map((part) => {
    if (part === `## ${version}` || part.startsWith(headerLine)) {
      found = true;
      return entry; // entry already ends with \n
    }
    return part;
  });

  if (!found) {
    // Find the first block starting with ## (index > 0) and insert before it
    const firstVersionIdx = newParts.findIndex(
      (p, i) => i > 0 && p.startsWith("## "),
    );
    if (firstVersionIdx === -1) {
      // No version blocks in file yet — append at the end
      newParts.push(entry);
    } else {
      newParts.splice(firstVersionIdx, 0, entry);
    }
  }

  return newParts.join("\n");
}

// ── stdin reading ─────────────────────────────────────────────────────────────

function readStdin() {
  return new Promise((resolve) => {
    if (process.stdin.isTTY) {
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

// ── Main ──────────────────────────────────────────────────────────────────────

async function main() {
  validateEnvironment();

  let input = "";

  if (process.argv[2]) {
    // Command-line argument: unescape literal \n sequences
    input = process.argv[2].replace(/\\n/g, "\n");
  } else {
    input = await readStdin();
  }

  input = input.trim();

  if (!input) {
    console.error("[Error] No version content received.");
    console.error("Usage:");
    console.error(
      "  printf '## 0.20.1\\n> Release Date: 2025-12-10\\n...' | node update_version_en.js",
    );
    console.error(
      '  node update_version_en.js "## 0.20.1\\n> Release Date: 2025-12-10\\n..."',
    );
    process.exit(1);
  }

  validateInput(input);

  const version = extractVersion(input);
  if (!version) {
    console.error("[Error] Could not extract version number from input.");
    process.exit(1);
  }

  const fileContent = fs.readFileSync(TARGET_FILE, "utf8");
  const isUpdate = versionExists(fileContent, version);

  // Ensure entry ends with exactly one newline (required by split/join logic)
  const entry = input.trim() + "\n";

  const newContent = processVersionUpdate(fileContent, version, entry);
  fs.writeFileSync(TARGET_FILE, newContent, "utf8");

  if (isUpdate) {
    console.log(`[Success] Version ${version} has been updated.`);
  } else {
    console.log(
      `[Success] Version ${version} has been inserted as the latest entry.`,
    );
  }
  console.log(`[File] ${TARGET_FILE}`);
}

main().catch((err) => {
  console.error("[Error] Execution failed: " + err.message);
  process.exit(1);
});
