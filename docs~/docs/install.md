# インストール

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 |
| VRChat SDK | 不要です |

パッケージ全体として必要なパッケージはありません。次のツールだけ、ほかのパッケージが入っていないと有効にできません。

| ツール | 必要なパッケージ |
|---|---|
| 01_Helper for MA Blendshape Sync | Modular Avatar 1.17.0 以降 |
| 02_Helper for AAO Merge PhysBone | AAO: Avatar Optimizer 1.9.0 以降 |
| 03_Helper for AAO Merge Bone | AAO: Avatar Optimizer 1.9.0 以降 |
| 07_Parts Set Adjuster | VRChat SDK - Base 3.5.0 以降 |
| 08_FaceEmo Splitter | FaceEmo 1.7.0 以降（2.x は未対応） |

04_Hair Tone Matcher は、色を合わせる先の髪のマテリアルが lilToon か Poiyomi のときに使えます。

## VCC / ALCOM から入れる

1. VCC または ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Packages を開き、「Poyo's Avatar Toolbox」を追加します。

## .unitypackage から入れる

[リリースページ](https://github.com/Poyotoron/Avatar-Toolbox/releases)から `.unitypackage` をダウンロードし、Unity にドラッグ＆ドロップしてインポートします。

## 開き方

メニューの `Tools > Poyo's Avatar Toolbox` で設定ウィンドウが開きます。メニューはこの 1 つだけで、各ツールは設定ウィンドウから開きます。入れた直後はどのツールも無効なので、[設定ウィンドウ](settings.md)で有効にしてください。

## 更新する

VCC / ALCOM で更新するか、新しい `.unitypackage` をインポートします。有効にしたツールは、更新しても有効のままです。
