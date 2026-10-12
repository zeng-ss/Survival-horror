#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""生成 .gitignore 中的「大体积美术资源排除」清单。

背景：GitHub 免费仓库不适合存放大体积二进制素材（单文件 100MB 硬限制、
仓库建议 <1GB、免费 Git LFS 仅 1GB）。因此本项目只把「代码 / 工程配置 /
轻量资源」提交到 Git，大体积美术/媒体资源排除在外，另行分发。

本脚本扫描 Assets/ 下 **大于等于阈值** 的「美术/媒体类」二进制文件
（贴图 / 模型 / 音频 / 视频 / unitypackage 等），
把它们写进 .gitignore 的标记区块里。代码相关的二进制（dll/so/字体等）
不会被排除，以免破坏工程。

美术资源变动后，重新运行本脚本即可更新清单：
    python tools/gen_gitignore.py          # 默认阈值 1 MB
    python tools/gen_gitignore.py 2        # 阈值改为 2 MB
"""

import os
import sys

# ---------------------------------------------------------------------------
# 只排除「美术 / 媒体」类二进制；代码相关的二进制（dll/so/a/字体）保持提交
# ---------------------------------------------------------------------------
EXCLUDE_EXT = {
    # 图片 / 贴图
    "png", "jpg", "jpeg", "tga", "tif", "tiff", "psd", "exr", "hdr", "dds",
    "bmp", "gif", "webp", "iff", "pict",
    # 3D 模型
    "fbx", "blend", "blend1", "obj", "dae", "3ds", "3dm", "ma", "mb", "max",
    "c4d", "lwo", "lws", "stl", "ply", "skp", "ztl", "dxf", "jas",
    "glb", "gltf", "abc",
    # 音频 / 视频
    "wav", "mp3", "ogg", "aif", "aiff", "it", "mod", "s3m", "xm",
    "mp4", "mov", "avi", "wmv", "mpg", "mpeg", "flv", "asf", "ogv", "webm",
    # 其它大体积资源
    "unitypackage", "sbsar", "sbs",
}

BEGIN = "# ==== BEGIN 大体积资源排除（由 tools/gen_gitignore.py 自动生成，请勿手改此区块） ===="
END = "# ==== END 大体积资源排除 ===="

STANDARD = """\
# ---------------------------------------------------------------------------
# Unity 工程标准忽略（UUID 为 github/gitignore 的 Unity 模板）
# 生成：tools/gen_gitignore.py  阈值：{threshold}
# ---------------------------------------------------------------------------
.utmp/
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
/[Rr]ecordings/
*.log

# 编辑器 / IDE
.vs/
.idea/
.vscode/
.gradle/
.consulo/
*.csproj
*.unityproj
*.sln
*.slnx
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
*.pidb.meta
*.pdb.meta
*.mdb.meta
*.DotSettings.user

# Unity Version Control (Plastic) 本地元数据
.plastic/

# 构建产物 / 崩溃文件
*.apk
*.aab
*.unitypackage
*.unitypackage.meta
*.app
sysinfo.txt
crashlytics-build.properties
mono_crash.*

# Addressables / Visual Scripting 自动生成
/ServerData
/[Aa]ssets/StreamingAssets/aa*
/[Aa]ssets/AddressableAssetsData/link.xml*
/[Aa]ssets/Addressables_Temp*
/[Aa]ssets/AddressableAssetsData/*/*.bin*
/[Aa]ssets/Unity.VisualScripting.Generated/VisualScripting.Flow/UnitOptions.db
/[Aa]ssets/Unity.VisualScripting.Generated/VisualScripting.Flow/UnitOptions.db.meta
/[Aa]ssets/Unity.VisualScripting.Generated/VisualScripting.Core/Property Providers
/[Aa]ssets/Unity.VisualScripting.Generated/VisualScripting.Core/Property Providers.meta
/[Aa]ssets/[Ii]nit[Tt]est[Ss]cene*.unity*
/[Aa]ssets/[Ss]ceneDependencyCache*

# 其它
*.blend1
*.blend1.meta
"""


def escape(path: str) -> str:
    """转义 gitignore 通配字符。"""
    out = []
    for ch in path:
        if ch in "\\*?[]":
            out.append("\\" + ch)
        else:
            out.append(ch)
    return "".join(out)


def scan(root: str, threshold: int):
    hits = []
    for dp, _dn, fn in os.walk(root):
        for f in fn:
            p = os.path.join(dp, f)
            try:
                s = os.path.getsize(p)
            except OSError:
                continue
            ext = f.rsplit(".", 1)[1].lower() if "." in f else ""
            if ext in EXCLUDE_EXT and s >= threshold:
                rel = os.path.relpath(p, ".").replace(os.sep, "/")
                hits.append((s, rel))
    hits.sort(key=lambda x: x[1])
    return hits


def main() -> None:
    threshold_mb = float(sys.argv[1]) if len(sys.argv) > 1 else 1.0
    threshold = int(threshold_mb * 1024 * 1024)

    hits = scan("Assets", threshold)
    total = sum(s for s, _ in hits)

    blocks = [
        BEGIN,
        "# 下列 {} 个文件均 >= {} MB，合计 {:.0f} MB。".format(
            len(hits), _fmt(threshold_mb), total / 1048576
        ),
        "# 它们在本地保留、不提交到 Git；需要时从备份/网盘恢复（保持同名同路径）。",
        "",
    ]
    blocks += [("!" + escape(r)) if r.startswith("!") else escape(r) for _s, r in hits]
    blocks += ["", END, ""]
    generated = "\n".join(blocks)

    # 保留用户在标记区块之外的手改内容
    existing = ""
    if os.path.exists(".gitignore"):
        with open(".gitignore", encoding="utf-8") as f:
            existing = f.read()

    if BEGIN in existing and END in existing:
        head = existing.split(BEGIN)[0].rstrip("\n")
        tail = existing.split(END, 1)[1].lstrip("\n")
    else:
        head = STANDARD.format(threshold=_fmt(threshold_mb)).rstrip("\n")
        tail = existing.strip("\n")

    parts = [head, "", generated]
    if tail:
        parts += [tail, ""]

    with open(".gitignore", "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(parts))

    print("已写入 .gitignore：排除 {} 个文件，合计 {:.0f} MB".format(len(hits), total / 1048576))


def _fmt(v: float) -> str:
    return str(int(v)) if float(v).is_integer() else str(v)


if __name__ == "__main__":
    main()
