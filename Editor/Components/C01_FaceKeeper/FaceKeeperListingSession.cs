using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperListingSession
    {
        private static GameObject _root;
        private static FaceKeeperListing _listing;

        internal static void Begin(GameObject root)
        {
            _root = root;
            _listing = null;
        }

        internal static bool IsTarget(GameObject avatarRoot)
        {
            return _root != null && _root == avatarRoot;
        }

        internal static void Store(FaceKeeperListing listing)
        {
            _listing = listing;
        }

        internal static FaceKeeperListing End()
        {
            FaceKeeperListing listing = _listing;
            _root = null;
            _listing = null;
            return listing;
        }
    }
}
