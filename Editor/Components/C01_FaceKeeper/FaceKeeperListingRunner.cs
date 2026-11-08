using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperListingRunner
    {
        internal static FaceKeeperListing Run(PATFaceKeeper component, out string error)
        {
            error = null;
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                error = "再生中とコンパイル中は一覧を作れません。";
                return null;
            }
            VRCAvatarDescriptor descriptor = component.GetComponentInParent<VRCAvatarDescriptor>(true);
            if (descriptor == null)
            {
                error = "アバターの中に置いてください。";
                return null;
            }
            var meshes = new System.Collections.Generic.List<SkinnedMeshRenderer>();
            FaceKeeperMeshes.Resolve(component, descriptor.transform, meshes, out string meshError);
            if (meshError != null)
            {
                error = meshError;
                return null;
            }
            var paths = new System.Collections.Generic.List<string>();
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                paths.Add(AnimationUtility.CalculateTransformPath(mesh.transform, descriptor.transform));
            }
            Scene preview = EditorSceneManager.NewPreviewScene();
            GameObject clone = null;
            FaceKeeperListing listing = null;
            try
            {
                // NOTE: 利用者のシーンに複製を残すと変更済みになるため、プレビュー専用シーンでビルドする。
                clone = UnityEngine.Object.Instantiate(descriptor.gameObject);
                clone.name = descriptor.gameObject.name;
                SceneManager.MoveGameObjectToScene(clone, preview);
                FaceKeeperListingSession.Begin(clone);
                EditorUtility.DisplayProgressBar("Poyo's Avatar Toolbox", "表情の一覧を作っています…", 0.5f);
                AvatarProcessor.ProcessAvatar(clone);
                listing = FaceKeeperListingSession.End();
                if (listing == null)
                {
                    error = "一覧を作れませんでした。ビルドの中で PAT Face Keeper が処理されませんでした。";
                }
                if (listing != null)
                {
                    listing.TargetPaths = paths;
                }
                return listing;
            }
            catch (Exception exception)
            {
                FaceKeeperListingBuilder.DestroyClips(listing);
                listing = FaceKeeperListingSession.End();
                FaceKeeperListingBuilder.DestroyClips(listing);
                error = "ビルドが途中で止まりました: " + exception.Message;
                Debug.LogException(exception);
                return null;
            }
            finally
            {
                // NOTE: 例外で止まっても複製・受け口・ビルドの一時ファイルを残さない。
                FaceKeeperListingSession.End();
                EditorUtility.ClearProgressBar();
                if (clone != null)
                {
                    UnityEngine.Object.DestroyImmediate(clone);
                }
                EditorSceneManager.ClosePreviewScene(preview);
                AvatarProcessor.CleanTemporaryAssets();
            }
        }
    }
}
