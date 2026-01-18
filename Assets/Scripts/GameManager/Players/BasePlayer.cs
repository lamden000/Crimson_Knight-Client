using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameManager.Players
{
    public abstract class BasePlayer : BaseObject
    {
        private PlayerMovementController playerMovementControllerPrefab;
        private Character characterPrefab;
        private PlayerAnimationController playerAnimationControllerPrefab;

        public readonly ItemEquipment[] WearingItems = new ItemEquipment[4];

        public void SetupPrefab(bool isMainPlayer = false)
        {
            playerMovementControllerPrefab = this.GetComponent<PlayerMovementController>();
            playerMovementControllerPrefab.SetUp(this);
            characterPrefab = this.GetComponent<Character>();
            characterPrefab.SetUp(this.ClassType);
            playerAnimationControllerPrefab = this.GetComponent<PlayerAnimationController>();
            playerAnimationControllerPrefab.SetUp();
            playerMovementControllerPrefab.IsMainPlayer = isMainPlayer;
            LoadBasePart();
        }

        public ClassType ClassType;
        public PkType PkType;
        public Gender Gender;

        protected GameObject PkIcon;
        protected Vector3 PkIconOriginalScale;
        protected Quaternion PkIconOriginalRotation;
        public void SetPkIcon(GameObject icon)
        {
            this.PkIcon = icon;
            PkIconOriginalScale = PkIcon.transform.localScale;
            PkIconOriginalRotation = PkIcon.transform.localRotation;
        }

        public override void AutoMoveToXY(int x, int y)
        {
            playerMovementControllerPrefab.MoveToXY(x, y);
        }


        bool isAnimationDie = false;
        protected override void CheckCurrentHp()
        {
            if (IsDie())
            {
                if (!isAnimationDie)
                {
                    isAnimationDie = true;
                    playerAnimationControllerPrefab.SetDeadState();
                }
            }
            else
            {
                if (isAnimationDie)
                {
                    isAnimationDie = false;
                    playerAnimationControllerPrefab.SetAliveState();
                    LoadPartWearing(this.WearingItems);
                }
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            CheckCurrentHp();
            if (PkIcon != null)
            {
                Vector3 targetScale = PkIconOriginalScale;
                Quaternion targetRotation = PkIconOriginalRotation;

                if (transform.localScale.x < 0)
                {
                    targetScale.x = -Mathf.Abs(PkIconOriginalScale.x);
                }
                else
                {
                    targetScale.x = Mathf.Abs(PkIconOriginalScale.x);
                }

                float yRotation = transform.rotation.eulerAngles.y;
                if (Mathf.Abs(yRotation - 180f) < 1f)
                {
                    targetRotation = Quaternion.Euler(0, 180f, 0) * PkIconOriginalRotation;
                }

                PkIcon.transform.localScale = targetScale;
                PkIcon.transform.localRotation = targetRotation;
            }
        }

        public void ChangePkType(PkType type)
        {
            this.PkType = type;
            PkIconManager pkIconManager = PkIcon.GetComponent<PkIconManager>();
            if (pkIconManager != null)
            {
                pkIconManager.SetPkState(this.PkType);
            }
        }

        public override void AniTakeDamage(int dam, BaseObject attacker)
        {
            characterPrefab.AniTakeDamage();
            SpawnManager.GI().SpawnTxtDisplayTakeDamagePrefab(this.GetX(), this.GetY() + (int)this.GetTopOffsetY(), dam);
        }



        public override void AniAttack(BaseObject target = null)
        {
            if (IsDie())
            {
                return;
            }

            PlayerAnimationController playerAnimation = playerAnimationControllerPrefab;
            Direction dirToTarget = playerAnimation.GetCurrentDirection();

            if (target != null)
            {
                int x1 = target.GetX();
                int y1 = target.GetY();
                int x2 = this.GetX();
                int y2 = this.GetY();
                int deltaX = x1 - x2;
                int deltaY = y1 - y2;
                if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
                {
                    if (deltaX > 0)
                        dirToTarget = Direction.Right;
                    else
                        dirToTarget = Direction.Left;
                }
                else
                {
                    if (deltaY > 0)
                        dirToTarget = Direction.Up;
                    else
                        dirToTarget = Direction.Down;
                }

            }
            playerAnimation.SetAnimation(dirToTarget, State.Attack);
        }


        private void LoadBasePart()
        {
            if (this.Gender == Gender.Male)
            {
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Hair, 0);
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Body, 0);
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Legs, 0);
            }
            else
            {
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Hair, 41);
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Body, 3);
                playerAnimationControllerPrefab.LoadPart(CharacterPart.Legs, 3);
            }

            playerAnimationControllerPrefab.LoadPartVuKhi(this.ClassType, 0);
        }

        public void LoadPartWearing(ItemEquipment[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                this.WearingItems[i] = items[i];
            }

            if (IsDie())
            {
                return;
            }
            //load
            ItemEquipment vukhi = GetVuKhi();
            int partVk = -1;
            if (vukhi != null)
            {
                partVk = TemplateManager.ItemEquipmentTemplates[vukhi.TemplateId].PartId;
            }
            playerAnimationControllerPrefab.LoadPartVuKhi(this.ClassType, partVk);

            ItemEquipment ao = GetAo();
            int partAo = -1;
            if (ao == null)
            {
                if (this.Gender == Gender.Male)
                {
                    partAo = 0;
                }
                else
                {
                    partAo = 3;
                }
            }
            else
            {
                partAo = TemplateManager.ItemEquipmentTemplates[ao.TemplateId].PartId;
            }
            playerAnimationControllerPrefab.LoadPart(CharacterPart.Body, partAo);

            ItemEquipment quan = GetQuan();
            int partQuan = -1;
            if(quan == null)
            {
                if (this.Gender == Gender.Male)
                {
                    partQuan = 0;
                }
                else
                {
                    partQuan = 3;
                }
            }
            else
            {
                partQuan = TemplateManager.ItemEquipmentTemplates[quan.TemplateId].PartId;
            }
            playerAnimationControllerPrefab.LoadPart(CharacterPart.Legs, partQuan);

            ItemEquipment wing = GetWing();
            int partWing = -1;
            if (wing != null)
            {
                partWing = TemplateManager.ItemEquipmentTemplates[wing.TemplateId].PartId;
            }
            playerAnimationControllerPrefab.LoadPart(CharacterPart.Wings, partWing);
        }


        public ItemEquipment GetVuKhi()
        {
            return this.WearingItems[(int)EquipmentType.Weapon];
        }

        public ItemEquipment GetAo()
        {
            return this.WearingItems[(int)EquipmentType.Armor];
        }

        public ItemEquipment GetQuan()
        {
            return this.WearingItems[(int)EquipmentType.Pants];
        }

        public ItemEquipment GetWing()
        {
            return this.WearingItems[(int)EquipmentType.Wing];
        }


        public void AniPickupItem(BaseObject item)
        {
        }
    }
}
