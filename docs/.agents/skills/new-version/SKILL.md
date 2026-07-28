---
name: new-version
description: 生成并更新 UzonMail 的版本发布文档
---

# Instructions

根据用户对新版本的描述，生成中文和英文两段 Markdown 更新正文，并调用仓库根目录的 `scripts/update-version-doc.ps1` 更新文档。

## Markdown 内容要求

1. 中文正文仅使用有内容的分类：`### 功能新增`、`### 功能优化`、`### Bug 修复`
2. 英文正文仅使用有内容的分类：`### New Features`、`### Improvements`、`### Bug Fixes`
3. 每个分类下使用编号列表
4. 不要输出版本标题、发布日期、下载地址或 docker 链接
5. 用户输入不是目标语言时，翻译后生成对应语言的正文

## 更新方式

不得直接编辑 `docs/downloads.md`、`docs/en/downloads.md` 或 `.vuepress/public/updates` 下的文件。将生成的正文分别通过 PowerShell here-string 管道传给脚本：

```powershell
$chineseMarkdown = @'
### 功能优化

1. 示例更新内容
'@
$chineseMarkdown | & pwsh -NoProfile -File ..\scripts\update-version-doc.ps1 -Version '0.23.0' -UpdatePath 'docs/docs/downloads.md'

$englishMarkdown = @'
### Improvements

1. Example release note
'@
$englishMarkdown | & pwsh -NoProfile -File ..\scripts\update-version-doc.ps1 -Version '0.23.0' -UpdatePath 'docs/docs/en/downloads.md'
```

脚本会根据更新路径确定语言，并自动生成版本号、日期、下载地址和更新清单。若任一次脚本调用失败，停止执行并报告错误。
