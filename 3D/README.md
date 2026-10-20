# 3D 课题规范

> 工程目录：`3D/` · Unity **2022.3.62f3c1** · **URP 14.0.6** · **Linear** 色彩空间
> 仓库流程（Fork / 克隆 / 分支 / PR）见[根目录 README](../README.md)
> 每个课题按 **7–10 天（含自学）** 设计，任选其一。

---

## 0. 先看这里

### 0.1 打开工程

1. Unity Hub → **Add project from disk** → 选 **`3D/`** 目录（不是仓库根）
2. 用 `ProjectSettings/ProjectVersion.txt` 里的版本打开
3. 打开 `Assets/Scenes/Preview3D.unity`，点 **Play**

### 0.2 四个课题的起点

| 按键 | 课题 | 空着的时候会看到什么 |
| --- | --- | --- |
| `1` | 道具建模 | 空场景（还没交道具） |
| `2` | 材质 | 空场景（还没交贴图） |
| `3` | 场景美化 | **白模**——12 个方块拼的房间，这就是起点 |
| `4` | 骨骼动画 | 基础角色站在转台上（还没交动画） |

按 `3` 看到的那间用方块拼的房间，就是课题三要美化的对象。它不是 bug，是你工作的起点。

### 0.3 交作业只用一个文件夹

所有文件都放进对应课题的 `DropHere/`，**不要建子文件夹**：

```
Assets/Tasks/Task1_Props/DropHere/
Assets/Tasks/Task2_Materials/DropHere/
Assets/Tasks/Task3_Diorama/DropHere/
Assets/Tasks/Task4_Character/DropHere/
```

放进去之后**不用点任何菜单**：编辑器会自动重新扫描，Console 里会打一行
`[美术课题] 3D 课题X · ...`，回到 Play 就能看到。

### 0.4 工程提供的东西在哪

| 路径 | 内容 | 课题 |
| --- | --- | --- |
| `Assets/Tasks/Task2_Materials/Base/Crate.fbx` | 木箱（指定模型） | 二 |
| `Assets/Tasks/Task2_Materials/Base/Pillar.fbx` | 石柱（指定模型） | 二 |
| `Assets/Tasks/Task3_Diorama/Base/Kit.fbx` | 28 件低模套件 | 三 |
| `Assets/Tasks/Task3_Diorama/Base/Blockout.fbx` | 立方体白模 | 三 |
| `Assets/Tasks/Task4_Character/Base/Dummy.fbx` | 已绑定的角色 | 四 |

**`Base/` 目录不要改**，那是全班统一的基础。

---

## 1. 目录结构

```
Assets/
├── Tasks/
│   ├── Task1_Props/               课题一：道具建模
│   │   ├── task.json              ← 交付契约（文件清单 / 面数 / 尺寸）
│   │   └── DropHere/              ← 你只往这里放
│   ├── Task2_Materials/           课题二：材质
│   │   ├── task.json
│   │   ├── Base/                  ← Crate.fbx / Pillar.fbx（指定模型）
│   │   └── DropHere/
│   ├── Task3_Diorama/             课题三：场景美化
│   │   ├── task.json
│   │   ├── Base/                  ← Kit.fbx / Blockout.fbx
│   │   └── DropHere/
│   └── Task4_Character/           课题四：骨骼动画
│       ├── task.json
│       ├── Base/                  ← Dummy.fbx / BaseCharacter.prefab
│       └── DropHere/
├── Scenes/Preview3D.unity         ← 唯一需要打开的测试场景
├── Scripts/                       ← 助教维护，不要动
└── Settings/                      ← URP 管线资产，不要动
```

**空文件夹不是丢了**：Git 不记录空目录，每个空目录里有 `.gitkeep`，不要删。

**生成物不入库**：`Assets/Resources/Preview/`、`Task4_Character/Animation.controller`
这些文件由工程自动生成，第一次打开时重建，不要手改、也不用手动提交。

---

## 2. 通用约定

| 项 | 约定 |
| --- | --- |
| 单位 | 1 Unity unit = 1 米，按真实尺度 |
| 轴向 | Y 轴向上，`+Z` 朝前 |
| 轴心 | 道具在底部中心，角色在脚底中心 |
| 导出 | FBX，应用全部变换（Apply All Transforms），不带缩放/旋转 |
| 贴图 | 默认 1024，主道具可 2048，必须是 2 的幂 |
| 法线 | Tangent Space，导入类型设为 Normal Map，sRGB 关闭 |
| 源文件 | `.blend` / `.max` 一律不提交 |

### 2.1 文件命名

- **全部小写**，只用字母、数字、下划线
- **不要空格、中文、括号**，例如不要 `长剑 (1).fbx`
- FBX 名 = 道具 id，贴图 = id + 固定后缀
- 大小写严格按表里写，`Sword.fbx` 和 `sword.fbx` 在 Linux 上不是一回事

### 2.2 贴图三件套

| 后缀 | 内容 | 导入设置（工程会自动设） |
| --- | --- | --- |
| `_Albedo` | 颜色 | sRGB 开 |
| `_Normal` | 法线 | Normal Map 类型，sRGB 关 |
| `_MaskMap` | R 金属度、G AO、A 光滑度 | sRGB 关 |

MaskMap 的通道含义：

| 通道 | 内容 | 取值 |
| --- | --- | --- |
| R | Metallic | 0 = 非金属，255 = 金属 |
| G | AO | 255 = 无遮挡，0 = 全遮挡（平铺材质填 255） |
| B | 预留 | 填 0 |
| A | Smoothness | 0 = 粗糙，255 = 光滑 |

在 Photoshop 里用「通道」面板拼，在 Substance 里用 Pack 节点拼。

---

## 3. 课题一 · 道具建模

**目标**：从零完成 5 件可进引擎的低模道具，并体验一次高模→低模烘焙。

### 3.1 放到哪

`Assets/Tasks/Task1_Props/DropHere/`

### 3.2 文件清单（20 个）

每件道具 = 1 个 FBX + 3 张贴图。

| id | FBX | 贴图 | 道具 | 参考最大边 | 面数上限 |
| --- | --- | --- | --- | --- | --- |
| `sword` | `sword.fbx` | `sword_Albedo.png` `sword_Normal.png` `sword_MaskMap.png` | 长剑 | 1.1 m | 3,000 |
| `shield` | `shield.fbx` | `shield_*.png` | 圆盾 | 0.7 m | 3,000 |
| `potion` | `potion.fbx` | `potion_*.png` | 药瓶 | 0.25 m | 2,000 |
| `torch` | `torch.fbx` | `torch_*.png` | 火把 | 0.6 m | 3,000 |
| `chest` | `chest.fbx` | `chest_*.png`（可用 2048） | 宝箱 | 0.9 m | 8,000 |

**命名规则**：贴图 = `<id>_Albedo.png` / `<id>_Normal.png` / `<id>_MaskMap.png`。
把表里的 id 代进去就是文件名，例如 `shield` → `shield_Albedo.png`、`shield_Normal.png`、`shield_MaskMap.png`。
不要写成 `Shield.Albedo.png`、`盾牌_颜色.png` 这种。

### 3.3 每件怎么做

| id | 造型重点 |
| --- | --- |
| `sword` | 剑身要直、有厚度；护手和握把分开建；刃口可以加一道高光面 |
| `shield` | 曲面盾面 + 正面纹样；背面要有把手和绑带 |
| `potion` | 圆润瓶身 + 瓶颈 + 瓶塞；液体单独一个面，方便以后做透明 |
| `torch` | 木柄 + 布头 + 金属箍，天然三段；布头可以破一点 |
| `chest` | 箱体 + 弧形盖 + 锁扣 + 铆钉；这件要做高模烘焙 |

### 3.4 步骤

1. **定比例**：在场景里放一个 1 m 立方体当参照，先把最大边调到表里的值
2. **白模搭建**：大块面切出来（箱体、柄、刃），确认剪影再细化
3. **展 UV**：不重叠、边缘留 4 px 边距；硬表面按面分块，曲面尽量少接缝
4. **宝箱烘焙**：
   - 高模做倒角、铆钉、木纹起伏
   - 拓扑低模（不要直接减面）
   - 烘焙 Normal 和 AO，AO 进 MaskMap 的 G 通道
5. **画贴图**：三张 1024（宝箱可 2048），Albedo 不要带光照，Normal 只放凹凸
6. **对轴心**：把轴心移到模型**底部中心**，然后 Apply All Transforms
7. **导出**：只选模型，单位 m，不导出材质（材质由工程按贴图生成）
8. **放进** `DropHere/`，回 Unity 按 `1`

### 3.5 验收

- 面数不超上限（超 20% 直接判错）
- 最大边不超过参考尺寸的 1.3 倍
- 有 UV、轴心在底部、Albedo 必须有（缺 Normal/Mask 会警告）
- 美术：剪影可读、比例可信、五件风格统一

### 3.6 常见错误

| 现象 | 原因 |
| --- | --- |
| 五件比例忽大忽小 | 没按米制建，或参照立方体没放对 |
| 宝箱法线全是脏边 | 高模直接减面，没有重新拓扑 |
| 贴图糊 | 用 512 的图硬撑，或者 UV 拉伸严重 |
| 模型歪 | 导出前没 Apply Transform |

---

## 4. 课题二 · 材质

**目标**：掌握 PBR 贴图绘制、通道打包、UV 展开与铺设。

### 4.1 放到哪

`Assets/Tasks/Task2_Materials/DropHere/`

### 4.2 文件清单（15 个）

| id | 贴图 | 类型 | 说明 |
| --- | --- | --- | --- |
| `rock` | `rock_Albedo.png` `rock_Normal.png` `rock_MaskMap.png` | 可平铺 | 岩石 |
| `metal` | `metal_*.png` | 可平铺 | 金属 |
| `wood` | `wood_*.png` | 可平铺 | 木材 |
| `crate` | `crate_*.png` | 贴模型 | 用工程给的 `Base/Crate.fbx` |
| `pillar` | `pillar_*.png` | 贴模型 | 用工程给的 `Base/Pillar.fbx` |

**命名规则**：和课题一一样，`<id>_Albedo.png` / `<id>_Normal.png` / `<id>_MaskMap.png`。
例如 `rock` → `rock_Albedo.png`、`rock_Normal.png`、`rock_MaskMap.png`。

**只交贴图，不交模型。** 1024 × 1024，必须 2 的幂。

### 4.3 三套平铺怎么做

1. 用 Substance Designer / Sampler，或者在 PS 里画
2. **必须四边无缝**：PS 里用「滤镜 → 其它 → 位移」把图偏移半张，接缝会跑到中间，补好再偏移回去
3. 在预览里看：三个球体按 2×2 平铺贴，接缝一眼能看出来
4. MaskMap 的 G 通道填 255（平铺材质没有 AO）

### 4.4 两套模型怎么做

1. 导入 `Base/Crate.fbx` 和 `Base/Pillar.fbx`
2. **删掉原来的 UV**（这两个模型自带一套 UV，是给你练手的，别直接用）
3. 重新展 UV：
   - 木箱：按木板分块，接缝藏在木板缝里
   - 石柱：柱身环形展开，接缝转到背面
4. 控制纹素密度：**1 米约 512–1024 像素**，整个模型要均匀
5. 画三张贴图，Albedo 里可以带木纹和磨损
6. 只导出贴图，不导出模型

### 4.5 验收

- 五套的 Albedo 齐全；法线/Mask 缺失会警告
- 法线图类型是 Normal Map，法线和 Mask 的 sRGB 关闭
- 美术：平铺无缝、纹素密度均匀、五套材质风格协调

### 4.6 常见错误

| 现象 | 原因 |
| --- | --- |
| 材质发灰、细节糊 | 法线图没关 sRGB |
| 平铺有明显接缝 | 四边像素不连续 |
| 木箱有的地方糊有的地方锐 | 纹素密度不均 |
| 金属像塑料 | Metallic 没填或 Smoothness 全 0 |

---

## 5. 课题三 · 场景美化

**题目**：废弃的炼金工坊一角

### 5.1 工程提供

| 文件 | 说明 |
| --- | --- |
| `Base/Kit.fbx` | **28 件低模套件**：墙、半墙、墙洞、柱子、楼梯、门、桌、椅、桶、宝箱、药瓶、锅、石头、火把架、武器、旗帜等 |
| `Base/Blockout.fbx` | **12 个方块**拼的 4×4 m 白模 |

白模的 12 个方块分别是：

| 方块 | 代表什么 |
| --- | --- |
| Floor | 4×4 地面 |
| Wall_Back / Wall_Left_* / Wall_Right | 三面墙，左墙留了 **0.8×0.6 的窗洞** |
| Partition | 右前方的 **L 形隔断**，把入口做成拐角 |
| Furnace | 右后角的 **炼金炉体**（1×2.6×1） |
| Bench | 靠后墙的 **工作台**，视觉中心 |
| Shelf | 左后角的货架 |
| Platform | 右前方的 0.2 m 抬高平台 |

素材是 **CC0**（Kenney Mini Dungeon），可以随便改。

### 5.2 你要做什么

**第 0 步 · 先看清起点**

打开 `Preview3D.unity` 按 Play，按 `3`。你会看到一间灰色方块房间——
这就是本课题的起点，所有人都是这间屋子。

**第 1 步 · 导入素材**

在 Blender / Maya 里新建场景，导入两个文件：

- `Base/Blockout.fbx` —— 对位用，别改它
- `Base/Kit.fbx` —— 28 件散开摆在 3 m 网格上，**按需要挑，不要整个拖进来**

**第 2 步 · 用 Kit 替换方块**

| 方块 | 换成什么 |
| --- | --- |
| 地面 | `floor` + `floor-detail`，拼满 4×4 |
| 墙 | `wall` / `wall-half`；窗洞位置用 `wall-opening` 或直接留空 |
| L 形隔断 | `wall-narrow` 或 `wood-support` 搭一个木结构 |
| 炼金炉 | `column` + `rocks` 堆叠，或保留体块外面包一层 |
| 工作台 | `table`，上面摆 `potion` / `pot` 之类 |
| 货架 | `wood-support` + `banner` |
| 平台 | `stairs` 或地砖抬高 |

方块可以留在里面被包住，**但画面里不能直接看到灰色方块**。

**第 3 步 · 加道具和细节**

`barrel`、`chest`、`pot`、`potion`、`key`、`weapon-sword`、`shield-round` 都可以用。
注意别堆满——留出地面和墙面的呼吸，也让视觉中心（工作台）突出。

**第 4 步 · 做材质（≤ 4 套）**

按用途分四套就够了，例如：

1. 地面（石板 / 泥土）
2. 墙面（旧砖 / 抹灰）
3. 木头（桌、桶、架子）
4. 金属或炉体（铁、铜、发光的炉口）

每套三张 1024 贴图，命名和课题二一样。局部想要变化，可以在 DCC 里拆 UV 或用顶点色，不要靠加材质套数解决。

**第 5 步 · 打光**

- 主光从**窗洞**方向进来——白模留这个洞就是为了给你一个光源理由
- 炉体做**自发光**材质，作为画面里的暖色点
- 补一盏冷色环境光，拉开冷暖对比
- 别开全亮的环境光，否则画面会平

**第 6 步 · 导出**

1. 选中场景里所有要交的物体（不要带 Blockout 的灰色方块）
2. 导出 `Diorama.fbx`
3. **勾选 Embed Textures**（贴图嵌进 FBX），否则进 Unity 是白模
4. 应用全部变换，单位 1 m

**第 7 步 · 验收**

把 `Diorama.fbx` 和一张打好光的 `final.png`（1920×1080）放进
`Task3_Diorama/DropHere/`，回 Unity 按 `3`：

- 底部会显示面数、材质数、包围盒
- 超限会在自检里标出来：面数超过 36,000 或材质超过 6 套直接判错，包围盒超过 4.5 m 报警告

### 5.3 交付

```
DropHere/
├── Diorama.fbx     ← 贴图嵌入 FBX
└── final.png       ← 打好光的 1920×1080 渲染图
```

| 项 | 上限 |
| --- | --- |
| 面数 | 30,000 tris |
| 材质 | 4 套 |
| 包围盒 | 4.5 × 4.5 × 4.5 |

### 5.4 验收

- 技术：面数、材质数、包围盒不超限
- 美术：构图有视觉中心、前中后景层次清楚、材质统一、打光能看出主光方向

---

## 6. 课题四 · 骨骼动画

### 6.1 工程提供

| 文件 | 说明 |
| --- | --- |
| `Base/Dummy.fbx` | 白色低模假人，Humanoid 骨架：22 根人体骨骼 / 43 根骨架节点 |
| `Base/BaseCharacter.prefab` | 已挂 Animator 与材质 |

- 素材是 **CC0**，可以放心使用与修改
- 你**不需要**建模、绑定、刷权重，只负责动画
- 角色约 2.4 单位高，脚底在原点下方约 0.2，预览会自动对齐地面

### 6.2 放到哪 / 文件清单（3 个）

`Assets/Tasks/Task4_Character/DropHere/`

| 文件名 | 时长 | 24 fps 帧数 | 循环 |
| --- | --- | --- | --- |
| `Idle.fbx` | 1.0–1.2 s | 24–29 帧 | 是 |
| `Run.fbx` | 0.6–0.8 s | 15–19 帧 | 是 |
| `StartRun.fbx` | 0.2–0.3 s | 5–7 帧 | 否 |

文件名大小写严格按上面写。也可以交一个 `Animation.fbx` 里带三个同名 take（进阶用法）。

### 6.3 步骤

1. **导入** `Base/Dummy.fbx`，不要改骨骼数量、名字和层级
2. **场景帧率设 24**（Blender：Output Properties → Frame Rate）
3. **做 Idle**：1.0–1.2 s，呼吸起伏，首尾姿势对齐
4. **做 Run**：0.6–0.8 s，至少一个完整步幅，首尾对齐
5. **做 StartRun**：0.2–0.3 s，从站姿到跑姿，**最后一帧要和 Run 的第一帧一致**
6. **原地**：根骨骼不要有位移，不要开 Root Motion
7. **导出**：三个动作分别导出三个 FBX。Blender 里在 FBX 导出的
   `Animation → Animation Mode` 选 **Active Action**，只导当前动作
8. **放进** `DropHere/`，回 Unity 按 `4`，空格切换 Idle/Run

### 6.4 循环动画怎么接

- 第 1 帧和第 N 帧的姿势要一样，但**不要**把第 1 帧原样复制到第 N 帧
- 正确做法：关键帧范围是 1..N，播放器从 N 回到 1 时靠插值衔接
- 检查方法：在 DCC 里循环播放，盯着手脚有没有跳一下

### 6.5 状态机（助教维护，你不用管）

```
Idle --(Speed>0.1)--> StartRun --(exitTime 0.9)--> Run --(Speed<0.1)--> Idle
```

没交 `StartRun` 时会直接 `Idle -> Run`，不会卡死。

### 6.6 验收

- 技术：三个 clip 名字正确、时长在区间内、Idle/Run 循环、StartRun 不循环
- 美术：重心转移可信、跑步循环首尾自然、起步有加速感

### 6.7 常见错误

| 现象 | 原因 |
| --- | --- |
| 导入后找不到动画 | 文件名不是 Idle/Run/StartRun |
| 每跑一圈抖一下 | 首尾姿势不一致 |
| 起步瞬间弹一下 | StartRun 第一帧和 Idle 差太远 |
| 角色滑步 | 开了 Root Motion 或位移烘进了动画 |
| 动作变形 | 改了骨骼名字或增删了骨骼 |

---

## 7. 预览与自检

打开 `Assets/Scenes/Preview3D.unity` → **Play**：

| 按键 | 作用 |
| --- | --- |
| `1` | 道具（课题一） |
| `2` | 材质（课题二） |
| `3` | 场景（课题三）；没交时显示白模 |
| `4` | 骨骼动画（课题四） |
| `空格` | 课题四：切换 Idle / Run |
| `R` | 重置 |

底部一行显示当前课题的统计，超限会变红。

菜单：

| 菜单 | 作用 |
| --- | --- |
| `Tools / 美术课题 / 自检（3D）` | 出完整报告：缺件、面数、尺寸、贴图规范 |
| `Tools / 美术课题 / 重新生成 3D 预览数据` | 手动重建 |
| `Tools / 美术课题 / 重新导入 3D 任务贴图` | 贴图导入设置被改坏时强制重导 |
| `Tools / 美术课题 / 打开 3D 预览场景` | 一键打开 `Preview3D.unity` |

---

## 8. 评分方式

技术自检是**门槛**：不过就打回，不计分；过了之后只看美术。

---

## 9. 常见问题

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 按 `3` 只有白模 | 还没交 `Diorama.fbx`，这是起点 | 按第 5 节的步骤做 |
| 按 `1`/`2` 是空的 | 还没交文件 | 按文件清单检查命名和路径 |
| 模型全黑 | 材质 Shader 不兼容，或法线图 sRGB 没关 | 用 URP/Lit；法线图设为 Normal Map |
| 模型是粉的 | 材质 Shader 不是 URP 版本 | 换 `Universal Render Pipeline/Lit` |
| 交的 FBX 在 Unity 里是白模 | 导出时没嵌入贴图 | 重新导出，勾 Embed Textures |
| 模型悬空 / 陷地 | 轴心不在底部 | 在 DCC 里把轴心移到脚底/底部中心 |
| 比例不对 | 导出时没按米制 | 统一 1 unit = 1m，重新导出 |
| 面是黑的 | 负缩放导致法线翻转 | 应用变换，别用镜像缩放 |
| 文件名识别不到 | 大小写不对，或用了中文/空格 | 按表里的 id 重新命名 |
| 识别不出动画 | 文件名和 take 名都不是 Idle/Run/StartRun | 按 `Idle.fbx` / `Run.fbx` / `StartRun.fbx` 命名 |
| 每跑一圈抖一下 | 首尾姿势不一致 | 让第一帧和最后一帧姿势对齐 |
| 贴图发灰 | 法线图没关 sRGB | 导入时关掉 sRGB |
| 平铺有接缝 | 四边像素不连续 | 用位移滤镜检查并修补 |
