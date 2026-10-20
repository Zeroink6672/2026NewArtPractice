# 2026 美术练习课题 · Unity 接入说明

> 浙江工业大学 创游工坊 · 2026

---

## 0. 这个仓库怎么用

三个课题共用**同一个 Unity 工程**。你的全部工作是：**按规范把 PNG 放进指定文件夹**，然后打开测试场景按 Play 看效果。

不需要写代码，不需要挂组件，不需要改场景，不需要动 Animator。

> **当前状态**：工程、目录骨架、交付契约、预览场景、自动生成与自检工具均已就绪。
> `Examples/` 里的格式范例由助教补充。

---

## 1. 环境准备（只做一次）

| 需要 | 要求 |
| --- | --- |
| Unity | **2022.3.62f3c1（Unity 中国版）** |
| Unity Hub | 用 [unity.cn](https://unity.cn) 的 Hub 安装对应版本。不要装"最新版"顶替 |
| GitHub 账号 | 用来 Fork 仓库、开 Pull Request |
| GitHub Desktop | 有图形界面，不用记命令 |
| Git LFS | **必须安装**，否则图片会变成指针文件 |

> 版本号里的 **`c1` 表示 Unity 中国版**。国际版 Hub 里没有 `2022.3.62f3c1` 这个版本，
> 全班请统一从 unity.cn 安装，不要和国际版混用。
> 核对方法：打开工程前对照 `ProjectSettings/ProjectVersion.txt` 里的 `m_EditorVersion`，必须**完全一致**。

### 1.1 把仓库 Fork 到自己账号下

1. 打开助教发的仓库主页，点右上角 **Fork**
2. 下一页直接点 **Create fork**
3. 完成后你会停在 `你的用户名/2026NewArtPractice`，**之后所有操作都在这个副本上**
4. 副本默认停在 **`dev`** 分支——这是学员基线，**不要切到 `main`**

> 仓库有两条长期分支：
> - **`dev`**：学员基线。助教在这里更新契约（`task.json`）和范例，你的分支从这里拉
> - **`main`**：冻结快照，只在里程碑时更新，平时不用管
>
> 为什么要 Fork：这是公开仓库，只有仓库所有者能直接往里推。Fork 出来的副本属于你，
> 你随便提交；最后用 Pull Request 把改动交给助教。
>
> 公开仓库意味着**大家的提交互相可见**——别往仓库里放和课题无关的文件。

### 1.2 用 GitHub Desktop 克隆自己的副本

1. 安装 [GitHub Desktop](https://desktop.github.com/)，登录你的 GitHub 账号
2. **File → Clone repository → GitHub.com**，在列表里选 `你的用户名/2026NewArtPractice`
   —— 注意是**你自己账号下**的那个，不是助教的
3. 本地路径保持**纯英文、无空格**
4. **Repository → Open in Command Prompt**，执行一次：

```bash
git lfs install
```

这一步每台机器做一次。做完用 `git lfs ls-files` 验证——能正常输出（哪怕是空列表）就说明 LFS 生效了。

> 如果克隆下来发现 PNG 只有 100 多字节，就是 LFS 没生效，见 §6。

### 1.3 打开工程

1. Unity Hub → **Add project from disk** → 选择仓库根目录（就是有 `Assets/`、`ProjectSettings/` 的那一层，**不是** `Assets/` 里面）
2. 用 `ProjectVersion.txt` 里写的版本打开。提示"版本不一致"时，去装对应版本，不要点升级
3. 首次打开会导入资源，**等进度条走完再动**（几百张小图可能要好几分钟）
4. 打开 `Assets/Scenes/Preview.unity`，点 **Play**

> 工程路径请保持**纯英文、无空格**。`D:\作业\游戏 美术\` 这类路径在 Unity + Git LFS 组合下容易出各种怪问题。

---

## 2. 铁律（比规范更重要）

1. **只修改 `Assets/Tasks/你的课题/` 下面的文件。**
   不要动 `ProjectSettings/`、`Packages/`、`Assets/Scripts/`、`Assets/Scenes/`、`Assets/Settings/`。
2. **不要复制别人的 `.meta` 文件，也不要复制范例文件的 `.meta`。**
   会导致 GUID 冲突，整个工程报错，而且很难修。
3. **不要提交 PSD / PSB / AI 源文件**，只交导出的 PNG。`.gitignore` 已经挡掉了。
4. **不要给已提交的文件改名或挪位置。** Unity 用 GUID 认资源，改名等于换了一个新资源，引用会断。要改就在分支里删掉重提。
5. **一个分支只放一个人的作品。** 不要把两个人做的同一个课题合到一个分支里。
6. **不要用 Git 客户端"自动合并"场景和预制体。** 仓库已设为二进制合并，冲突就找助教，别硬合。
7. **提交前先看 GitHub Desktop 的 Changes：只应该有你任务目录下的文件。**
   出现别的目录就右键还原，别"顺手一起提交"。

---

## 3. 目录结构

```
Assets/
├── Tasks/
│   ├── Task1_Icons/               课题一：道具图标
│   │   ├── task.json              ← 交付契约（文件清单 / 尺寸），先读它
│   │   ├── DropHere/              ← 你只往这里放图
│   │   └── Examples/              ← 助教范例，不要改
│   ├── Task2_Character/           课题二：角色状态机
│   │   ├── task.json
│   │   ├── Idle/  Run/  Attack/  Hit/  Die/    ← 文件夹名 = 状态名
│   │   ├── Animations/            ← 生成物（本地生成，不入库）
│   │   ├── Character.controller    ← 生成物（本地生成，不入库）
│   │   ├── Character.prefab        ← 生成物（本地生成，不入库）
│   │   └── Examples/
│   └── Task3_VFX/                 课题三：元素特效
│       ├── task.json
│       ├── Thunder/ Fire/ Ice/ Poison/ Earth/ Life/
│       │   └── Bullet/  Explosion/  Spray/  Fall/
│       └── Examples/
├── Scenes/Preview.unity           ← 唯一需要打开的测试场景
├── Scripts/                       ← 助教维护，不要动
└── Settings/
```

**空文件夹不是丢了**：Git 不记录空目录，每个空目录里有一个 `.gitkeep` 占位文件，**不要删**。

**标着「生成物」的文件不入库**：每台机器第一次打开工程时由工具自动重建。
不要手改，也不用管它们有没有出现在 Git 里。

---

## 4. 三个课题的交付规范

### 课题一 · RPG 道具图标套组

- 放到 `Assets/Tasks/Task1_Icons/DropHere/`
- **64×64** PNG，透明底，一张一个文件
- 文件名必须是下面 16 个 ID 之一，大小写一致

| 文件名 | 道具 | | 文件名 | 道具 |
| --- | --- | --- | --- | --- |
| `icon_potion_hp.png` | 生命药水 | | `icon_gem.png` | 宝石 |
| `icon_potion_mp.png` | 法力药水 | | `icon_coin.png` | 金币 |
| `icon_sword.png` | 铁剑 | | `icon_key.png` | 钥匙 |
| `icon_shield.png` | 木盾 | | `icon_bomb.png` | 炸弹 |
| `icon_bow.png` | 长弓 | | `icon_herb.png` | 草药 |
| `icon_staff.png` | 法杖 | | `icon_map.png` | 地图 |
| `icon_ring.png` | 戒指 | | `icon_boots.png` | 靴子 |
| `icon_amulet.png` | 护符 | | `icon_scroll.png` | 卷轴 |

风格：1px 深色描边，统一左上 45° 光源，保证缩到 64px 还能看清轮廓。

### 课题二 · 角色全套状态机动画序列帧

- 放到 `Assets/Tasks/Task2_Character/<状态名>/`
- 每个状态一个文件夹，**文件夹名就是下面的状态名**
- **512×512** PNG，单帧一张，`<State>_0001.png` 起编号（**四位补零**，别用 `_1.png`）
- 角色脚底对齐画布底边中点，**同一状态内所有帧的画布尺寸和位置必须一模一样**

| 状态 | 文件夹 | 最少帧数 | 循环 | 说明 |
| --- | --- | --- | --- | --- |
| Idle | `Idle/` | 8 | 是 | 待机呼吸，首尾无缝 |
| Run | `Run/` | 8 | 是 | 跑步循环，首尾无缝 |
| Attack | `Attack/` | 6 | 否 | 起手 → 挥击 → 收招 |
| Hit | `Hit/` | 3 | 否 | 受击硬直，幅度要明显 |
| Die | `Die/` | 6 | 否 | 倒地，停在最后一帧 |

状态机参数（助教已配好，你不用管）：`Speed`(float)、`Attack`(trigger)、`Hit`(trigger)、`Dead`(bool)。
预览里用 `A`/`D` 跑、`J` 攻击、`H` 受击、`U` 倒地/起身。

### 课题三 · 元素特效套组

- 放到 `Assets/Tasks/Task3_VFX/<元素>/<类型>/`
- **256×256** PNG，`<Element>_<Type>_0001.png` 起编号（四位补零）
- 帧要对齐画面中心，**不要做位移**；位移和发射方向由播放端负责

| 元素 | 文件夹 | | 类型 | 文件夹 | 必需 | 最少帧 | 混合 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 雷 | `Thunder/` | | 子弹 | `Bullet/` | ✅ | 4 | 加法 |
| 火 | `Fire/` | | 爆炸 | `Explosion/` | ✅ | 8 | 加法 |
| 冰 | `Ice/` | | 喷射 | `Spray/` | 加分 | 6 | 透明 |
| 毒 | `Poison/` | | 坠落 | `Fall/` | 加分 | 6 | 透明 |
| 土 | `Earth/` | | | | | | |
| 生命 | `Life/` | | | | | | |

**必需 12 套**：六元素 × (子弹 + 爆炸)。喷射、坠落实装多少都算加分。

---

## 5. 预览与自检

打开 `Assets/Scenes/Preview.unity` → **Play**。场景里不用摆任何东西，预览是运行时自动生成的。

| 按键 | 作用 |
| --- | --- |
| `1` / `2` / `3` | 切换课题 |
| `A` / `D` | 课题二：向左 / 向右奔跑 |
| `J` | 课题二：攻击 |
| `H` | 课题二：受击 |
| `U` | 课题二：倒地 / 起身（倒地期间其他按键无效） |
| `Q` / `E` | 切换特效（课题三） |
| `R` | 重播当前特效 |

顶部菜单：

| 菜单 | 作用 |
| --- | --- |
| `Tools / 美术课题 / 自检` | 出报告：缺哪些文件、尺寸不对、帧数不够 |
| `Tools / 美术课题 / 重新生成预览数据` | 手动重建预览数据 |
| `Tools / 美术课题 / 重新导入任务资源` | 贴图导入设置被改坏时强制重导 |
| `Tools / 美术课题 / 打开预览场景` | 一键打开 `Preview.unity` |

**放完 PNG 不用点菜单**：编辑器检测到任务目录里的 PNG 变化会自动重新扫描，
Console 里会打一行总览（有错误时是黄色）：

```
[美术课题] 已扫描 · 图标 5/16 · 角色 0/5 个状态 · 特效 0/12 套 · 错误 33 · 警告 0 · 缺 icon_bow … 等 11 个 · 首个问题：尺寸 16×16，要求 64×64 · 详情 Tools / 美术课题 / 自检
```

这行只给总览；要看逐条清单点「自检」。自检结果是红色必须修、黄色是建议，点一条可以直接跳到对应文件。

如果预览没跟上，点一次「重新生成预览数据」再按 Play。

---

## 6. 出问题了

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 图片只有 100 多字节，Unity 导入失败 | 没装 / 没启用 Git LFS | `git lfs install` 然后 `git lfs pull` |
| 打开工程一片粉色 | 渲染管线或材质不匹配 | 确认用 `ProjectVersion.txt` 的版本打开；不要删 `Packages/` |
| 状态机里一堆 Missing | `.meta` 没同步，或文件被改名 | 删掉本地工程重新 clone；不要手动改任何 GUID |
| 找不到 `DropHere` 之类的文件夹 | 没拉最新的 `dev` | `git pull`；确认克隆的是仓库根 |
| 图糊 / 锯齿严重 | 交付尺寸不对，靠放大凑数 | 按规范尺寸重新导出 |
| 动画播放时角色抖动 | 同状态各帧画布尺寸或脚底位置不一致 | 统一画布，脚底对齐底边中点 |
| 按 Play 只有一个深灰背景 | 预览数据没生成 | 菜单 `Tools / 美术课题 / 重新生成预览数据` |
| 改了图但预览没变 | 自动扫描没触发 | 同上，然后重新 Play |
| 图标下面写着"缺 xxx" | 文件名不对，或没放进 `DropHere/` | 对照 §4 的 16 个 ID |
| 放图后 Console 没反应 | 正在 Play（自动扫描会暂停），或脚本有编译错误 | 退出 Play 再放；先看 Console 顶部的编译错误 |
| 帧顺序乱了 | 没补零（`_10` 排到了 `_2` 前面） | 改成四位补零 `_0010` |
| 首次打开卡很久 | 首次导入资源 | 等进度条，别强退 |
| push 被拒：文件过大 | 提交了 PSD 或超大图 | 只交 PNG；单文件上限 100MB |

还是不行，把 **Console 的完整报错**（不是截图的一角）和你的分支名发给助教。

---

## 7. 提交流程（Fork → 分支 → PR）

### 7.1 建分支

GitHub Desktop → **Current Branch → New Branch**，名字用 **`taskX-拼音`**，例如 `task1-zhangsan`。

> 别用中文：网页上会变成 `%E4%BD%A0...` 这种乱码，出问题不好查。也别用空格。

### 7.2 提交

把图放进对应文件夹后，回到 GitHub Desktop：

1. 左侧 **Changes** 里确认改动**只有**你自己任务目录下的文件：
   - 课题一：`Assets/Tasks/Task1_Icons/DropHere/` 下的 `.png` 和 `.png.meta`
   - 课题二：`Assets/Tasks/Task2_Character/<状态名>/` 下的序列帧
   - 课题三：`Assets/Tasks/Task3_VFX/<元素>/<类型>/` 下的序列帧
2. 如果出现 `ProjectSettings/`、`Packages/`、`Assets/Scripts/`、`Assets/Scenes/`——说明改错地方了，右键 **Discard changes** 还原
3. Summary 写清楚，例如 `task1: 16 枚道具图标`
4. 点 **Commit to task1-zhangsan**

> `.meta` 是 Unity 的资源身份证，必须跟着 PNG 一起提交。GitHub Desktop 会自动勾上，**别手动取消**。

### 7.3 推送

右上角 **Push origin**（第一次显示 **Publish branch**）。

### 7.4 开 Pull Request

1. 推送后 GitHub Desktop 会出现 **Create Pull Request**，点它
2. 浏览器打开比较页，**核对三行**：
   - `base repository` = 助教的仓库（**不是**你自己的 fork）
   - `base` = `dev`
   - `compare` = `你的账号:task1-zhangsan`
3. 标题写 `task1-张三`，描述里贴三样东西：
   - Console 里「美术课题自检」那段结果
   - 实际交付的文件数 / 帧数
   - 一张 Game 窗口截图（`Win + Shift + S` 框选即可）
4. **Create pull request**

### 7.5 等验收、改完再推

助教直接在 PR 里评论。你改完后：

1. 在**同一个分支**上继续 commit
2. Push origin
3. PR 会自动更新，**不用重新开一个**

> 不要点 Merge，合并由助教处理。

### 7.6 助教更新了 dev 怎么办

只有助教改了 `task.json` 或加了范例时才需要，平时不用管：

1. 网页上打开你的 fork，点 **Sync fork → Update branch**（同步的是 `dev`）
2. GitHub Desktop 点 **Fetch origin**
3. 切到 `dev` → **Pull origin**
4. 切回自己分支 → **Branch → Merge into Current Branch** → 选 `dev`

只往自己目录加文件的同学，冲突概率基本为零。

---

## 8. 验收标准

**技术**（工具自动判定）：自检无缺失、尺寸合规、帧数达标、命名合法。

**美术**（助教人工判定）：

- 三个课题内部风格统一（描边、光源、饱和度量级）
- 缩放后仍可读——图标在 64px、特效在 256px 下能一眼看出是什么
- 六个元素的色彩能互相区分，不能只靠"火焰颜色不一样"
- 循环动画首尾衔接自然，没有明显跳帧

---

## 9. 给助教

- **两条长期分支**：
  - `dev` 是学员基线，改 `task.json`、加范例都提交到这里，改完通知学员 Sync fork
  - `main` 是冻结快照，里程碑时把 `dev` 合进来并打 tag（`git tag v0.2`）
  - 分支不是备份：真正能还原的是 **tag**，`main` 漂了就只能靠 tag 找回来
- 新增/调整必需要素，改 `task.json` 即可，预览与自检会读它
- 编辑器工具都在 `Assets/Scripts/` 下：
  - `Runtime/PreviewCatalog.cs` 数据模型，`Runtime/PreviewBootstrap.cs` 运行时预览
  - `Editor/PreviewGenerator.cs` 扫描 + 生成 clip / controller / prefab / catalog
  - `Editor/TaskTexturePostprocessor.cs` 贴图导入规则（改规则时记得把 `ImportVersion` 加一）
  - `Editor/ArtPracticeSelfCheck.cs` 自检报告
- 生成物（`Resources/Preview/`、`Animations/`、`Character.controller`、`Character.prefab`）**不入库**，
  每台机器打开工程时自动重建——所以合并 PR 时不会有生成物冲突
- 收作业就看 PR 列表：`Files changed` 里应该只有 `Assets/Tasks/<任务>/` 下的图片；
  出现别的目录说明学生改错了地方，直接打回
- **学生 PR 默认只验收、不合并**：同一课题的文件名是固定的（Task1 就那 16 个），
  两份同课题的 PR 合进同一个分支会互相覆盖，Task2 更是只能留一份
- 真要归档，按人复制到 `Assets/Tasks/_Archive/<昵称>/`（在扫描目录之外，不影响预览）
- 要发版时再把 `dev` 合进 `main`。合并后 clone 一次 `main`，确认图片不是 100 多字节
  （fork + LFS 唯一需要盯的地方）
- 改了 `task.json` 要提醒学生 Sync fork，否则他们本地还是旧契约
- `Examples/` 目前为空，需要补 1~2 组格式范例
