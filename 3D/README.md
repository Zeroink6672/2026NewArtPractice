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

所有文件都放进对应课题的 `DropHere/`：

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

## 1. 通用约定

| 项 | 约定 |
| --- | --- |
| 单位 | 1 Unity unit = 1 米，按真实尺度 |
| 轴向 | Y 轴向上，`+Z` 朝前 |
| 轴心 | 道具在底部中心，角色在脚底中心 |
| 导出 | FBX，应用全部变换（Apply All Transforms），不带缩放/旋转 |
| 贴图 | 默认 1024，主道具可 2048，必须是 2 的幂 |
| 法线 | Tangent Space，导入类型设为 Normal Map，sRGB 关闭 |
| 源文件 | `.blend` / `.max` 一律不提交 |

贴图统一三张，按后缀命名：

| 后缀 | 内容 | 导入设置（工程会自动设） |
| --- | --- | --- |
| `_Albedo` | 颜色 | sRGB 开 |
| `_Normal` | 法线 | Normal Map 类型，sRGB 关 |
| `_MaskMap` | R 金属度、G AO、A 光滑度 | sRGB 关 |

---

## 2. 课题一 · 道具建模

**目标**：从零完成 5 件可进引擎的低模道具，并体验一次高模→低模烘焙。

| 文件 | 道具 | 参考最大边 | 面数上限 |
| --- | --- | --- | --- |
| `sword.fbx` | 长剑 | 1.1 m | 3,000 |
| `shield.fbx` | 圆盾 | 0.7 m | 3,000 |
| `potion.fbx` | 药瓶 | 0.25 m | 2,000 |
| `torch.fbx` | 火把 | 0.6 m | 3,000 |
| `chest.fbx` | 宝箱 | 0.9 m | 8,000（高模烘焙） |

### 你要做什么

1. **按尺寸建模**：先用方块搭比例，确认最大边接近参考值再细化
2. **展 UV**：不重叠、留边距；宝箱这种硬表面按面分块
3. **宝箱做高模→低模**：高模倒角 + 铆钉 + 木纹起伏，拓扑低模后烘焙 Normal 和 AO
4. **画三张贴图**：`_Albedo` / `_Normal` / `_MaskMap`，1024（宝箱可 2048）
5. **对轴心**：把轴心移到模型底部中心，应用全部变换
6. **导出**：`<id>.fbx`，名字必须是表里那五个
7. **放进** `Task1_Props/DropHere/`，回 Unity 按 `1` 看效果

### 验收

- 面数不超上限（超 20% 直接判错）
- 最大边不超过参考尺寸的 1.3 倍
- 有 UV、轴心在底部、三张贴图齐全
- 美术：剪影可读、比例可信、五件风格统一

---

## 3. 课题二 · 材质

**目标**：掌握 PBR 贴图绘制、通道打包、UV 展开与铺设。

| 类型 | 文件 | 说明 |
| --- | --- | --- |
| 可平铺 ×3 | `rock` / `metal` / `wood` | 必须四边无缝 |
| 贴模型 ×2 | `crate` / `pillar` | 用工程提供的模型，**清空原 UV 重新展** |

### 你要做什么

**三套平铺材质**

1. 在 DCC 或 Substance 里做无缝材质，四边必须能接上
2. 导出 `rock_Albedo.png` / `rock_Normal.png` / `rock_MaskMap.png`（`metal`、`wood` 同理）
3. 1024 × 1024，MaskMap 的 R 填金属度、G 填 1、A 填光滑度

**两套模型材质**

1. 导入 `Base/Crate.fbx` 和 `Base/Pillar.fbx`
2. **删掉原来的 UV**，重新展一次——纹素密度要均匀（1 米约 512–1024 像素）
3. 木箱的木板缝要对齐 UV 缝，石柱的接缝尽量藏在侧面
4. 按 `<id>_Albedo/_Normal/_MaskMap.png` 命名导出

**全部放进** `Task2_Materials/DropHere/`，回 Unity 按 `2`：
三个球体看平铺接缝，木箱和石柱看你贴模型的效果。

### 验收

- 五套的 Albedo 齐全；法线/Mask 缺失会警告
- 法线图类型是 Normal Map，法线和 Mask 的 sRGB 关闭
- 美术：平铺无缝、纹素密度均匀、五套材质风格协调

---

## 4. 课题三 · 场景美化

**题目**：废弃的炼金工坊一角

### 工程提供

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

### 你要做什么

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
| 工作台 | `table`，上面摆 `potion` / `pot` / `books` 之类 |
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
- 超过 30k 面 / 4 套材质 / 4.5 m 包围盒会变红

### 交付

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

### 验收

- 技术：面数、材质数、包围盒不超限
- 美术：构图有视觉中心、前中后景层次清楚、材质统一、打光能看出主光方向

---

## 5. 课题四 · 骨骼动画

### 工程提供

| 文件 | 说明 |
| --- | --- |
| `Base/Dummy.fbx` | 白色低模假人，Humanoid 骨架：22 根人体骨骼 / 43 根骨架节点 |
| `Base/BaseCharacter.prefab` | 已挂 Animator 与材质 |

- 素材是 **CC0**，可以放心使用与修改
- 你**不需要**建模、绑定、刷权重，只负责动画
- 角色约 2.4 单位高，脚底在原点下方约 0.2，预览会自动对齐地面

### 你要做什么

1. **导入** `Base/Dummy.fbx`，不要改骨骼数量、名字和层级
2. **做三个 Action**：`Idle`（1.0–1.2 s）、`Run`（0.6–0.8 s）、`StartRun`（0.2–0.3 s）
3. **24 fps**，原地动画，不要 Root Motion
4. `StartRun` 的最后一帧要和 `Run` 的第一帧对得上
5. 导出三个文件：`Idle.fbx` / `Run.fbx` / `StartRun.fbx`（只选骨骼和动画）
6. 放进 `Task4_Character/DropHere/`，回 Unity 按 `4`，空格切换 Idle/Run

### 状态机（助教维护，你不用管）

```
Idle --(Speed>0.1)--> StartRun --(exitTime 0.9)--> Run --(Speed<0.1)--> Idle
```

没交 `StartRun` 时会直接 `Idle -> Run`，不会卡死。

### 验收

- 技术：三个 clip 名字正确、时长在区间内、Idle/Run 循环、StartRun 不循环
- 美术：重心转移可信、跑步循环首尾自然、起步有加速感

---

## 6. 预览与自检

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

## 7. 评分方式

技术自检是**门槛**：不过就打回，不计分；过了之后只看美术。

---

## 8. 常见问题

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 按 `3` 只有白模 | 还没交 `Diorama.fbx`，这是起点 | 按第 4 节的步骤做 |
| 模型全黑 | 材质 Shader 不兼容，或法线图 sRGB 没关 | 用 URP/Lit；法线图设为 Normal Map |
| 模型是粉的 | 材质 Shader 不是 URP 版本 | 换 `Universal Render Pipeline/Lit` |
| 交的 FBX 在 Unity 里是白模 | 导出时没嵌入贴图 | 重新导出，勾 Embed Textures |
| 模型悬空 / 陷地 | 轴心不在底部 | 在 DCC 里把轴心移到脚底/底部中心 |
| 比例不对 | 导出时没按米制 | 统一 1 unit = 1m，重新导出 |
| 面是黑的 | 负缩放导致法线翻转 | 应用变换，别用镜像缩放 |
| 识别不出动画 | 文件名和 take 名都不是 Idle/Run/StartRun | 按 `Idle.fbx` / `Run.fbx` / `StartRun.fbx` 命名 |
| 每跑一圈抖一下 | 首尾姿势不一致 | 让最后一帧和第一帧完全对齐 |
| 贴图发灰 | 法线图没关 sRGB | 导入时关掉 sRGB |
| 平铺有接缝 | 四边像素不连续 | 用平铺工具重新处理边缘 |
