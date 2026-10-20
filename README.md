# 2026 美术练习课题 · 仓库入口

> 浙江工业大学 创游工坊 · 2026

## 0. 仓库里有什么

这个仓库放了**两个互相独立的 Unity 工程**：

| 目录 | 内容 | 渲染管线 | 色彩空间 |
| --- | --- | --- | --- |
| `2D/` | 道具图标 / 角色序列帧 / 元素特效 | Built-in | Gamma |
| `3D/` | 道具建模 / 材质 / 场景 / 骨骼动画 | URP 14.0.6 | Linear |

两个工程用同一个 Unity 版本，但**各自独立打开**，装不同的包、有不同的 Library，互不影响。

## 1. 先选课题，再决定打开哪个工程

| 你选的课题 | 打开这个目录 | 详细规范 |
| --- | --- | --- |
| 2D 图标 / 角色 / 特效 | `2D/` | [2D/README.md](2D/README.md) |
| 3D 道具 / 材质 / 场景 / 动画 | `3D/` | [3D/README.md](3D/README.md) |

**只打开你课题对应的那一个**，另一个不用管。

## 2. 环境准备（只做一次）

| 需要 | 要求 |
| --- | --- |
| Unity | **2022.3.62f3c1（Unity 中国版）** |
| Unity Hub | 用 [unity.cn](https://unity.cn) 的 Hub 安装对应版本 |
| GitHub 账号 | Fork 仓库、开 Pull Request |
| GitHub Desktop | 图形界面操作，不用记命令 |
| Git LFS | **必须安装**，否则图片和模型会变成指针文件 |

> 版本号里的 **`c1` 表示 Unity 中国版**。国际版 Hub 里没有这个版本，全班统一从 unity.cn 装。
> 核对方法：对照工程里的 `ProjectSettings/ProjectVersion.txt`，必须**完全一致**。

### 2.1 Fork 到自己账号下

1. 打开仓库主页，点右上角 **Fork** → **Create fork**
2. 之后你会停在 `你的用户名/2026NewArtPractice`，**所有操作都在这个副本上**
3. 副本默认停在 **`dev`** 分支——这是学员基线，**不要切到 `main`**

> 仓库有两条长期分支：
> - **`dev`**：学员基线，助教在这里更新契约和范例，你的分支从这里拉
> - **`main`**：冻结快照，只在里程碑时更新，平时不用管
>
> 公开仓库意味着**大家的提交互相可见**——别放和课题无关的文件。

### 2.2 用 GitHub Desktop 克隆

1. 安装 [GitHub Desktop](https://desktop.github.com/)，登录你的 GitHub 账号
2. **File → Clone repository → GitHub.com**，选 `你的用户名/2026NewArtPractice`
3. 本地路径保持**纯英文、无空格**
4. **Repository → Open in Command Prompt**，执行一次：

```bash
git lfs install
```

做完用 `git lfs ls-files` 验证——能正常输出（哪怕是空列表）就说明 LFS 生效了。

> 克隆下来如果图片只有 100 多字节，就是 LFS 没生效，见 §5。

### 2.3 打开工程

1. Unity Hub → **Add project from disk** → 选 `2D` 或 `3D` 目录
   （就是有 `Assets/`、`ProjectSettings/` 的那一层，**不是**仓库根，也不是 `Assets/` 里面）
2. 用 `ProjectVersion.txt` 里的版本打开。提示"版本不一致"时去装对应版本，不要点升级
3. 首次打开会导入资源，**等进度条走完再动**
4. 打开对应场景，按 **Play**：
   - 2D：`Assets/Scenes/Preview.unity`
   - 3D：`Assets/Scenes/Preview3D.unity`

## 3. 铁律（比规范更重要）

1. **只修改自己课题目录下的文件**：
   - 2D：`2D/Assets/Tasks/...`
   - 3D：`3D/Assets/Tasks/...`
2. **不要动** `ProjectSettings/`、`Packages/`、`Assets/Scripts/`、`Assets/Scenes/`、`Assets/Settings/`。
3. **不要复制别人的 `.meta` 文件**，会导致 GUID 冲突，整个工程报错。
4. **只提交导出的成品**（PNG / FBX / 贴图），不要交 PSD、`.blend`、`.max` 源文件。
5. **不要提交生成物**（预览数据、动画 clip、控制器、预制体）——已经 gitignore，本地会自动重建。
6. **一个分支只放一个人的作品**，不要把两个人的同一课题合到一个分支。
7. **提交前先看 GitHub Desktop 的 Changes**：只应该有你任务目录下的文件。

## 4. 提交流程（Fork → 分支 → PR）

### 4.1 建分支

GitHub Desktop → **Current Branch → New Branch**，名字用 **`taskX-拼音`**，例如 `task1-zhangsan`。

> 别用中文：网页上会变成 `%E4%BD%A0...`，出问题不好查。也别用空格。

### 4.2 提交

把作品放进对应目录后，回到 GitHub Desktop：

1. 左侧 **Changes** 里确认改动**只有**你自己任务目录下的文件
2. Summary 写清楚，例如 `task1: 16 枚道具图标`
3. 点 **Commit to task1-zhangsan**

> `.meta` 是 Unity 的资源身份证，必须跟着资源一起提交。GitHub Desktop 会自动勾上，**别手动取消**。

### 4.3 推送

右上角 **Push origin**（第一次显示 **Publish branch**）。

### 4.4 开 Pull Request

1. GitHub Desktop 出现 **Create Pull Request**，点它
2. 浏览器打开比较页，**核对三行**：
   - `base repository` = 助教的仓库（**不是**你的 fork）
   - `base` = `dev`
   - `compare` = `你的账号:task1-zhangsan`
3. 标题写 `task1-张三`，描述里贴：
   - Console 里「美术课题自检」那段结果
   - 交付的文件数 / 帧数
   - 一张 Game 窗口截图（`Win + Shift + S`）
4. **Create pull request**

### 4.5 等验收、改完再推

助教直接在 PR 里评论。改完后在**同一个分支**继续 commit + push，PR 会自动更新，不用重开。

> 不要点 Merge，合并由助教处理。

### 4.6 助教更新了 dev 怎么办

1. 网页上打开你的 fork，点 **Sync fork → Update branch**
2. GitHub Desktop 点 **Fetch origin**
3. 切到 `dev` → **Pull origin**
4. 切回自己分支 → **Branch → Merge into Current Branch** → 选 `dev`

## 5. 出问题了

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 图片只有 100 多字节 | Git LFS 没生效 | `git lfs install` 然后 `git lfs pull` |
| 打开工程一片粉色 | 渲染管线不匹配 | 用对应工程的版本打开；不要删 `Packages/` |
| 找不到 `DropHere` 之类的文件夹 | 没拉最新的 `dev` | `git pull` |
| 首次打开卡很久 | 首次导入资源 | 等进度条，别强退 |
| push 被拒：文件过大 | 提交了 PSD 或超大图 | 只交成品；单文件上限 100MB |

2D 的预览/自检问题见 [2D/README.md](2D/README.md)，3D 的见 [3D/README.md](3D/README.md)。

## 6. 给助教

- **两条长期分支**：`dev` 学员基线（改契约、加范例都提交到这里），`main` 冻结快照；
  里程碑时把 `dev` 合进 `main` 并打 tag（`git tag v0.2`）。**分支不是备份，tag 才是还原点**
- **学生 PR 默认只验收、不合并**：同一课题的文件名是固定的，两份同课题的 PR 合进同一分支会互相覆盖
- 生成物不入库，每台机器打开工程时自动重建，所以合并 PR 不会有生成物冲突
- 收作业看 PR 的 `Files changed`：应该只有 `2D/Assets/Tasks/...` 或 `3D/Assets/Tasks/...` 下的文件
- 改了契约要提醒学生 Sync fork，否则他们本地还是旧版本
