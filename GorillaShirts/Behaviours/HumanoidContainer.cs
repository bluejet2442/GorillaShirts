using GorillaExtensions;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Patches;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [RequireComponent(typeof(RigContainer), typeof(VRRig))]
    [DisallowMultipleComponent]
    internal class HumanoidContainer : ShirtHumanoid
    {
        public static HumanoidContainer LocalHumanoid;

        public VRRig Rig;

        public GorillaIK IK;

        public float NameTagZOffset = 0f;

        public void Awake()
        {
            Rig = GetComponent<VRRig>();
            Root = gameObject;

            IK = Rig.myIk ?? gameObject.GetOrAddComponent<GorillaIK>();

            // Use IK clothing anchors, with fallbacks for older rigs.
            Head = IK.headBone != null ? IK.headBone : (Rig.headMesh != null ? Rig.headMesh.transform : null);
            Body = IK.bodyBone != null ? IK.bodyBone.Find("body") ?? IK.bodyBone : null;
            LeftHand = IK.leftHand;
            RightHand = IK.rightHand;
            LeftUpper = IK.leftUpperArm != null ? IK.leftUpperArm : (IK.bodyBone != null ? IK.bodyBone.Find("shoulder.L") : null);
            RightUpper = IK.rightUpperArm != null ? IK.rightUpperArm : (IK.bodyBone != null ? IK.bodyBone.Find("shoulder.R") : null);
            LeftLower = IK.leftLowerArm != null ? IK.leftLowerArm : (IK.bodyBone != null ? IK.bodyBone.Find("shoulder.L/forearm.L") : null);
            RightLower = IK.rightLowerArm != null ? IK.rightLowerArm : (IK.bodyBone != null ? IK.bodyBone.Find("shoulder.R/forearm.R") : null);

            MainSkin = Rig.mainSkin;
            FaceSkin = Rig.faceSkin;

            PlayerNameTags = [Rig.playerText1];
            NameTagZOffset = Rig.playerText1 != null ? Rig.playerText1.transform.localPosition.z : 0f;

            if (LocalHumanoid == null && (Rig.isOfflineVRRig || Rig.isLocal || (Rig.Creator is NetPlayer creator && creator.IsLocal)))
            {
                LayerOverrides.TryAdd(EShirtObject.Head, UnityLayer.MirrorOnly);
                LocalHumanoid = this;
            }

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer += OnLocalInvisibilityChanged;
        }

        public new void OnDestroy()
        {
            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer -= OnLocalInvisibilityChanged;
            base.OnDestroy();
            if (LocalHumanoid == this) LocalHumanoid = null;

            try
            {
                ClearShirts();
                OffsetNameTag(0);
            }
            finally
            {
                RemoveObjects();
            }
        }

        public override void OnShirtWorn()
        {
            RefreshBodyRenderer();
            MoveNameTag();
        }

        public override void OnShirtRemoved()
        {
            RefreshBodyRenderer();
            MoveNameTag();
        }

        public override void MoveNameTagTransform(Transform transform, float offset)
        {
            Vector3 offsetVector = transform.localPosition;
            offsetVector.z = NameTagZOffset + ((float)offset * 0.02f);
            transform.localPosition = offsetVector;
        }

        public override void MoveNameTag()
        {
            if (Rig != null && Rig.TryGetComponent(out VRRigAnchorOverrides anchorOverrides))
            {
                anchorOverrides.UpdateName();
                anchorOverrides.UpdateBadge();
            }

            base.MoveNameTag();
        }

        private void OnLocalInvisibilityChanged(VRRig targetRig, bool isInvisible)
        {
            if (targetRig is null || targetRig != Rig)
                return;

            InvisibleToLocalPlayer = isInvisible;

            var shirts = Shirts;
            foreach (var shirtAsset in shirts)
            {
                if (Objects.TryGetValue(shirtAsset, out var objects))
                {
                    objects.ForEach(gameObject =>
                    {
                        if (gameObject != null) gameObject.SetActive(!isInvisible);
                    });
                }
            }
        }

        private void RefreshBodyRenderer()
        {
            if (Rig == null || Rig.bodyRenderer == null) return;
            GorillaBodyRenderer bodyRenderer = Rig.bodyRenderer;
            bodyRenderer.Refresh();
        }
    }
}


