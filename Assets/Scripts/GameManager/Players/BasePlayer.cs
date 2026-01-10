using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameManager.Players
{
    public abstract class BasePlayer: BaseObject
    {
        private PlayerMovementController playerMovementControllerPrefab;
        private Character characterPrefab;
        private PlayerAnimationController playerAnimationControllerPrefab;



        public void SetupPrefab(bool isMainPlayer = false)
        {
            playerMovementControllerPrefab = this.GetComponent<PlayerMovementController>();
            characterPrefab = this.GetComponent<Character>();
            playerAnimationControllerPrefab = this.GetComponent<PlayerAnimationController>();
            playerMovementControllerPrefab.IsMainPlayer = isMainPlayer;
        }

        public ClassType ClassType;
        public PkType PkType;


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
   
        protected override void LateUpdate()
        {
            base.LateUpdate();
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
    }
}
