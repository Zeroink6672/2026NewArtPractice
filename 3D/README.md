# 3D 课题规范（编写中）

> 工程目录：`3D/` · Unity **2022.3.62f3c1** · **URP 14.0.6** · **Linear** 色彩空间
> 仓库流程（Fork / 克隆 / 分支 / PR）见[根目录 README](../README.md)

## 当前状态

工程骨架已经搭好：

- URP 管线资产：`Assets/Settings/URP-Pipeline.asset`、`UniversalRenderer.asset`
- 预览场景：`Assets/Scenes/Preview3D.unity`（相机 + 平行光 + `PreviewRoot`）
- 色彩空间：Linear（3D PBR 必须）

**课题契约（`task.json`）、导入预设、材质/Prefab 生成器、自检工具尚未接入**，
下面四个课题是已确定的方向，验收规则和目录结构定稿后会补齐。

## 计划的四个课题

| 课题 | 方向 | 交付量 |
| --- | --- | --- |
| 一 · 道具建模 | 从零建模 + 1 件高模烘焙 | 5 件，其中 1 件做高模→低模（法线 + AO） |
| 二 · 材质 | 贴图绘制与铺设 | 5 套：3 套可平铺 + 2 套贴在指定模型上 |
| 三 · 场景 | 找素材 + 布景 + 打光 | 4m×4m 小样，外部素材占比 ≤ 30%，交素材来源清单 |
| 四 · 骨骼动画 | 蒙皮与动画 | Idle / Run / StartRun 三个 clip，同一个角色 |

课题四的 `StartRun` 是 Idle → Run 的起步过渡：状态机是
`Idle --(Speed>0.1)--> StartRun --(exitTime 0.9)--> Run --(Speed<0.1)--> Idle`。
如果 `StartRun` 没有交，生成器会跳过该状态、直接 Idle→Run，不会把角色卡死。

## 工程约定（先按这个准备素材）

| 项 | 约定 |
| --- | --- |
| 单位 | 1 Unity unit = 1 米，按真实尺度 |
| 轴向 | Y 轴向上，`+Z` 朝前 |
| 轴心 | 道具在底部中心，角色在脚底中心 |
| 导出 | FBX，应用全部变换（Apply All Transforms），不要带缩放/旋转 |
| 贴图 | 1024（主道具可 2048），2 的幂 |
| 法线 | Tangent Space，导入时类型必须设为 Normal Map |
| 源文件 | `.blend` / `.max` 一律不提交 |

## 目录规划（待建）

```
Assets/
├── Tasks/
│   ├── Task1_Props/DropHere/
│   ├── Task2_Materials/DropHere/
│   ├── Task3_Diorama/DropHere/
│   └── Task4_Character/<State>/
├── Scenes/Preview3D.unity
└── Scripts/
```

生成物（预览数据、材质、Prefab）不入库，打开工程时自动重建。

## 常见问题

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 模型全黑 | 材质用了不兼容的 Shader，或法线贴图 sRGB 没关 | 用 URP/Lit；法线图设为 Normal Map |
| 模型是粉的 | 材质 Shader 不是 URP 版本 | 换 `Universal Render Pipeline/Lit` |
| 模型悬空 / 陷地 | 轴心不在底部 | 在 DCC 里把轴心移到脚底/底部中心再导出 |
| 比例不对 | 导出时没按米制 | 统一 1 unit = 1m，重新导出 |
| 面是黑的 | 负缩放导致法线翻转 | 应用变换，别用镜像缩放 |
