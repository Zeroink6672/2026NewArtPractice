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
| Git | 2.30 以上 |
| Git LFS | **必须安装**，否则所有图片都会拉取失败 |

> 版本号里的 **`c1` 表示 Unity 中国版**。国际版 Hub 里没有 `2022.3.62f3c1` 这个版本，
> 全班请统一从 unity.cn 安装，不要和国际版混用。
> 核对方法：打开工程前对照 `ProjectSettings/ProjectVersion.txt` 里的 `m_EditorVersion`，必须**完全一致**。

### 1.1 安装 Git LFS 并克隆

```bash
git lfs install          # 每台机器执行一次，装完 Git LFS 后运行
git clone <仓库地址>
cd 2026NewArtPractice
```

如果跳过 `git lfs install`，克隆下来的 PNG 会变成 100 多字节的文本文件（LFS 指针），Unity 里导入报错。排查见 §6。

> 仓库地址以助教发的链接为准；用 `https://` 克隆时记得登录 GitHub 账号。

### 1.2 打开工程

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
│   │   ├── Animations/            ← 生成物：每个状态一个 clip
│   │   ├── Character.controller    ← 生成物：状态机
│   │   ├── Character.prefab        ← 生成物：预览用角色
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

**标着「生成物」的文件由工具维护**，每次重新生成都会覆盖，不要手改。

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

**放完 PNG 不用点菜单**：编辑器检测到任务目录里的 PNG 变化会自动重新扫描。
如果预览没跟上，点一次「重新生成预览数据」再按 Play。

自检结果输出在 Console：红色必须修，黄色是建议。点一条错误可以直接跳到对应文件。

---

## 6. 出问题了

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 图片只有 100 多字节，Unity 导入失败 | 没装 / 没启用 Git LFS | `git lfs install` 然后 `git lfs pull` |
| 打开工程一片粉色 | 渲染管线或材质不匹配 | 确认用 `ProjectVersion.txt` 的版本打开；不要删 `Packages/` |
| 状态机里一堆 Missing | `.meta` 没同步，或文件被改名 | 删掉本地工程重新 clone；不要手动改任何 GUID |
| 找不到 `DropHere` 之类的文件夹 | 没拉最新的 `main` | `git pull`；确认克隆的是仓库根 |
| 图糊 / 锯齿严重 | 交付尺寸不对，靠放大凑数 | 按规范尺寸重新导出 |
| 动画播放时角色抖动 | 同状态各帧画布尺寸或脚底位置不一致 | 统一画布，脚底对齐底边中点 |
| 按 Play 只有一个深灰背景 | 预览数据没生成 | 菜单 `Tools / 美术课题 / 重新生成预览数据` |
| 改了图但预览没变 | 自动扫描没触发 | 同上，然后重新 Play |
| 图标下面写着"缺 xxx" | 文件名不对，或没放进 `DropHere/` | 对照 §4 的 16 个 ID |
| 帧顺序乱了 | 没补零（`_10` 排到了 `_2` 前面） | 改成四位补零 `_0010` |
| 首次打开卡很久 | 首次导入资源 | 等进度条，别强退 |
| push 被拒：文件过大 | 提交了 PSD 或超大图 | 只交 PNG；单文件上限 100MB |

还是不行，把 **Console 的完整报错**（不是截图的一角）和你的分支名发给助教。

---

## 7. 提交流程

```bash
# 1. 从最新 main 开自己的分支（命名：课题号-昵称）
git checkout main
git pull
git checkout -b task2-zhangsan

# 2. 把图放进对应文件夹后
git add Assets/Tasks/Task2_Character
git commit -m "task2: 角色五状态序列帧"
git push -u origin task2-zhangsan
```

然后在 GitHub 发起 Pull Request（目标分支 `main`），PR 描述里写：

- 课题编号与昵称
- 实际帧数 / 文件数
- 自检报告截图（Console 里「美术课题自检」那一段）
- 一张 1920×1080 的预览截图

**不要自己合并 PR**，助教验收后统一处理。

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

- 骨架与契约改动请直接改 `main`，改完通知所有人重新拉取
- 新增/调整必需要素，改 `task.json` 即可，预览与自检会读它
- 编辑器工具都在 `Assets/Scripts/` 下：
  - `Runtime/PreviewCatalog.cs` 数据模型，`Runtime/PreviewBootstrap.cs` 运行时预览
  - `Editor/PreviewGenerator.cs` 扫描 + 生成 clip / controller / prefab / catalog
  - `Editor/TaskTexturePostprocessor.cs` 贴图导入规则（改规则时记得把 `ImportVersion` 加一）
  - `Editor/ArtPracticeSelfCheck.cs` 自检报告
- 生成物已入库；输入不变时重复生成是逐字节一致的，不会产生无意义的 diff
- `Examples/` 目前为空，需要补 1~2 组格式范例
