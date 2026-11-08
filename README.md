# Poyo's Avatar Toolbox

VRChat のアバター改変で使う小さな Unity エディタ拡張をまとめたパッケージです。使うツールだけを有効にして使います。無効のツールはコンパイルされません。

使い方: <https://ghp.maaaaa.net/Avatar-Toolbox/>

## 動作環境

- Unity 2022.3
- パッケージ全体として必要なパッケージはありません。一部のツールは、下の表のパッケージが必要です。

## インストール

1. VCC / ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Packages で「Poyo's Avatar Toolbox」を追加します。

VCC を使わない場合は、[リリースページ](https://github.com/Poyotoron/Avatar-Toolbox/releases)の `.unitypackage` をインポートしてください。

## 使い方

`Tools > Poyo's Avatar Toolbox` で設定ウィンドウを開き、使うツールにチェックを入れて「適用」を押します。再コンパイルが終わったら、ツールの行を選んで「開く」を押します。

## ツール

| ツール | 内容 | 必要なパッケージ |
|---|---|---|
| 00_Blendshape Keeper | 表情アニメーションで、改変したブレンドシェイプの値が戻らないようにする | なし |
| 01_Helper for MA Blendshape Sync | 衣装のブレンドシェイプを素体に追従させる MA Blendshape Sync の設定を、まとめて作る | Modular Avatar 1.17.0 以降 |
| 02_Helper for AAO Merge PhysBone | 統合する PhysBone の値を比べ、override する値を提案する | AAO: Avatar Optimizer 1.9.0 以降 |
| 03_Helper for AAO Merge Bone | AAO Merge Bone の設定を、ボーンのツリーからまとめて切り替える | AAO: Avatar Optimizer 1.9.0 以降 |
| 04_Hair Tone Matcher | 差し替えた髪の色を、元の髪の色に近づける | なし（新しい髪のマテリアルが lilToon か Poiyomi） |
| 05_Bone Weight Collapser | あるボーンのウェイトを、別のボーンに移す | なし |
| 06_BlendShape Backuper | ブレンドシェイプの値を保存し、あとで戻す | なし |
| 07_Parts Set Adjuster | 別のアバター用の髪や小物を、体に合わせて取り付ける | VRChat SDK - Base 3.5.0 以降 |
| 08_FaceEmo Splitter | FaceEmo の表情を、Prefab Variant ごとに分けて適用する | FaceEmo 1.7.0 以降（2.x は未対応） |

どのツールも、内容を確かめて「適用」を押したときに変更します。適用した変更はほとんどが Undo で取り消せます。取り消せないもの（作ったファイル、テクスチャの焼き直し、08 の Prefab の書き換え）は、ドキュメントの各ページに書いています。

00_Blendshape Keeper は非推奨です。後継は C01_Face Keeper です。

05_Bone Weight Collapser は非推奨です。後継は C02_Bone Weight Transfer です。

## コンポーネント

設定ウィンドウで有効にする必要はありません。アバターの GameObject を選び、Add Component の「Poyo's Avatar Toolbox」から追加します。アップロードのときに処理され、コンポーネント自身は消えます。

- **C00_Animation Helper（PAT Animation Helper）**: 位置・回転・大きさやブレンドシェイプを動かす Radial Puppet と、時間をかけて動くトグルのメニューを作ります。Modular Avatar 1.17.0 以降が必要です。
- **C01_Face Keeper（PAT Face Keeper）**: 全アニメーションの顔のキーを今の値まで引き上げ、表情で顔の改変が戻るのを防ぎます。一覧と比較プレビューを使えます。NDMF 1.14.0 以降（2.x は未対応）と VRChat SDK - Base 3.5.0 以降が必要です。
- **C02_Bone Weight Transfer（PAT Bone Weight Transfer）**: ボーンウェイトを別のボーンへ移します。元のメッシュを書き換えず、メッシュのファイルも作りません。NDMF 1.14.0 以降（2.x は未対応）が必要です。

## ライセンス

MIT License（[LICENSE](LICENSE)）
