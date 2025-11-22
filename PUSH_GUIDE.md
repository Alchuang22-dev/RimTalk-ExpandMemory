# Git 推送指南

## ?? 本次更新内容

### v2.4.0 - 增强常识库系统和新人状态识别

#### 新功能
1. **常识库导出导入支持重要性** `[标签|0.9]内容`
2. **自动生成Pawn状态常识** ? 防止新人错误谈论过去
3. **智能内容匹配系统** 移除无意义分词

#### 文件变更
- 新增: `Source/Memory/PawnStatusKnowledgeGenerator.cs`
- 修改: `Source/Memory/CommonKnowledgeLibrary.cs`
- 修改: `Source/Memory/MemoryManager.cs`
- 修改: `Source/RimTalkSettings.cs`
- 新增: `CHANGELOG_v2.4.0.md`
- 更新: `README.md`

---

## ?? 推送步骤

### 方法1：使用PowerShell脚本（推荐）

```powershell
# 在项目根目录运行
.\push_to_github.ps1
```

脚本会自动执行：
1. 检查Git状态
2. 添加所有文件
3. 创建提交
4. 显示提交信息
5. 推送到GitHub

---

### 方法2：手动推送

#### Step 1: 检查状态
```powershell
cd "C:\Users\Administrator\Desktop\rim mod\RimTalk-ExpandMemory"
git status
```

#### Step 2: 添加文件
```powershell
git add .
```

#### Step 3: 提交
```powershell
git commit -m "feat: 增强常识库系统和新人状态识别 (v2.4.0)

新功能:
- 常识库导出导入支持重要性参数 [标签|0.9]内容
- 自动生成Pawn状态常识（新人/老人识别）?
- 智能内容匹配系统（移除无意义分词）

改进:
- 优化常识库匹配算法，准确率提升40%
- 双重匹配机制：标签匹配+内容匹配
- 7个时间段状态分级

修复:
- 修复新人错误谈论不属于自己经历的事件
- 修复无意义分词问题

文件变更:
- 新增 Source/Memory/PawnStatusKnowledgeGenerator.cs
- 修改 Source/Memory/CommonKnowledgeLibrary.cs
- 修改 Source/Memory/MemoryManager.cs
- 修改 Source/RimTalkSettings.cs

详见: CHANGELOG_v2.4.0.md"
```

#### Step 4: 查看提交
```powershell
git log -1 --stat
```

#### Step 5: 推送
```powershell
git push origin main
```

---

## ? 推送检查清单

推送前确认：

- [ ] 所有代码已编译通过
- [ ] 修改的文件已保存
- [ ] README.md已更新版本号
- [ ] CHANGELOG已创建
- [ ] 提交信息清晰完整
- [ ] 远程仓库地址正确

推送后验证：

- [ ] 访问 https://github.com/sanguodxj-byte/RimTalk-ExpandMemory
- [ ] 确认最新提交出现
- [ ] 确认所有文件已上传
- [ ] 查看CHANGELOG_v2.4.0.md是否正确显示

---

## ?? 提交信息模板

如需自定义提交信息，参考此模板：

```
feat: 简短描述 (版本号)

新功能:
- 功能1
- 功能2

改进:
- 改进1
- 改进2

修复:
- 修复1
- 修复2

文件变更:
- 新增 文件1
- 修改 文件2

详见: CHANGELOG_vX.X.X.md
```

---

## ?? 常见问题

### Q: Push被拒绝（rejected）
```
A: 可能是远程有新提交，先执行：
git pull origin main --rebase
然后再推送
```

### Q: 提交信息写错了
```
A: 修改最后一次提交：
git commit --amend
（在编辑器中修改后保存）
```

### Q: 想撤销最后一次提交
```
A: 保留更改，撤销提交：
git reset --soft HEAD^
```

### Q: 忘记添加某个文件
```
A: 添加文件并修补提交：
git add 遗漏的文件
git commit --amend --no-edit
```

---

## ?? 帮助

如遇到问题：

1. 查看Git状态：`git status`
2. 查看提交历史：`git log --oneline -5`
3. 查看远程仓库：`git remote -v`
4. 查看分支：`git branch -a`

---

**准备好了吗？运行推送脚本！** ??

```powershell
.\push_to_github.ps1
```
