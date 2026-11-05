# はじめに

Poyo's Avatar Toolbox は、VRChat のアバター改変で使う小さなエディタ拡張をまとめたパッケージです。使うツールだけを有効にして使います。

## ツールの一覧

| ツール | 内容 | 必要なパッケージ |
|---|---|---|
| [00_Blendshape Keeper](blendshape-keeper.md) | 表情アニメーションで、改変したブレンドシェイプの値が戻らないようにする | なし |
| [01_Helper for MA Blendshape Sync](ma-blendshape-sync.md) | 衣装のブレンドシェイプを素体に追従させる MA Blendshape Sync の設定を、まとめて作る | Modular Avatar 1.17.0 以降 |
| [02_Helper for AAO Merge PhysBone](merge-physbone.md) | 統合する PhysBone の値を比べ、override する値を提案する | AAO: Avatar Optimizer 1.9.0 以降 |
| [03_Helper for AAO Merge Bone](merge-bone.md) | AAO Merge Bone の設定を、ボーンのツリーからまとめて切り替える | AAO: Avatar Optimizer 1.9.0 以降 |
| [04_Hair Tone Matcher](hair-tone-matcher.md) | 差し替えた髪の色を、元の髪の色に近づける | なし（新しい髪のマテリアルが lilToon か Poiyomi） |
| [05_Bone Weight Collapser](bone-weight-collapser.md) | あるボーンのウェイトを、別のボーンに移す | なし |
| [06_BlendShape Backuper](blendshape-backuper.md) | ブレンドシェイプの値を保存し、あとで戻す | なし |
| [07_Parts Set Adjuster](parts-set-adjuster.md) | 別のアバター用の髪や小物を、体に合わせて取り付ける | VRChat SDK - Base 3.5.0 以降 |
| [08_FaceEmo Splitter](face-emo-splitter.md) | FaceEmo の表情を、Prefab Variant ごとに分けて適用する | FaceEmo 1.7.0 以降（2.x は未対応） |

入れた直後は、どのツールも無効です。[設定ウィンドウ](settings.md)で有効にします。無効のツールはコンパイルされません。

![設定ウィンドウ](images/at-settings-overview.png){ .shot loading=lazy }

## 共通の動き

- どのツールも、対象を指定して内容を確かめ、「適用」を押したときに変更します。ウィンドウを開いたり、確認のボタンを押したりしただけでは、シーンもアセットも変わりません。
- アバターに独自のコンポーネントを追加しません。すべてエディタ専用です。
- 適用した変更は、ほとんどが Undo（Ctrl+Z）で取り消せます。取り消せないものは各ページに書いています。
- ファイルを作るのは、出力先を指定したときだけです。

Undo の履歴は、Unity を再起動したり、スクリプトが再コンパイルされたりすると消えます。大事な作業の前には、コミットするかプロジェクトを複製してください。

## スクリーンショットについて

このドキュメントのスクリーンショットには、次のアセットを使っています。

| 種類 | アセット | 配布ページ |
|---|---|---|
| アバター | Suiha | <https://dockyard.booth.pm/items/7534638> |
| 衣装 | 白凪館のリボンフリルメイド | <https://387.booth.pm/items/8995256> |
| 髪 | H24_宵編みロング | <https://kiriha-astra.booth.pm/items/7773808> |
