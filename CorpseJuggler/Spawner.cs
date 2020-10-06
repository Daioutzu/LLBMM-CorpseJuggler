using GameplayEntities;
using LLHandlers;
using LLModdingTools;
using UnityEngine;

namespace CorpseJuggler
{
    class Spawner : MonoBehaviour
    {

        static bool UsingController => InputHandler.GetLastActiveController() != Controller.mouseKeyboard;
        static int CurrentControllerMapID => InputHandler.GetLastActiveController().GetMap().id;
        static int previousControllerMapID;


        private BallEntity ball = null;
        private PlayerEntity player = null;
        private OEMHHMCDHGK dummy = null;
        private HHBCPNCDNDH prevBallSpeed;
        private IBGCBLLKIHA ballPos;
        private Vector2 ballMoveDirection;
        private float ballMoveSpeed;
        private bool expressCancelled;
        private bool ballMoveToggle;
        private bool corpseMode;
        private bool mouseCursorLocked;

        void TrainingModeStart()
        {
            Debug.Log("[LLBMM] \"Spawner\" Invoked TrainingModeStart");
            dummy = PlayerHandler.instance.CreateDummyPlayerEntity(6);
            dummy.SetPlayerState(PlayerState.DISABLED);
            dummy.SetPosition(World.OFF_SCREEN_POSITION);
            ball = BallHandler.instance.GetBall();
            player = PlayerHandler.instance.GetPlayerEntity(0);
        }

        void TrainingModeEnded()
        {
            Debug.Log("[LLBMM] \"Spawner\" Invoked TrainingModeEnded");
            Cursor.lockState = CursorLockMode.None;
            RestoreControllerExpress(previousControllerMapID);
            RestoreControllerExpress(CurrentControllerMapID);
            dummy.player.OKDEILOGKFB();
            ball = null;
            Destroy(dummy);
            Destroy(this);
        }
        void OnDestroy()
        {
            CorpseJuggler.Instance.OnTrainingModeStart -= TrainingModeStart;
            CorpseJuggler.Instance.OnTrainingModeEnd -= TrainingModeEnded;

            CorpseJuggler.Instance.OnEnableCorpseSpawner -= EnableCorpseSpawner;
            CorpseJuggler.Instance.OnSpawnCorpseKeyDown -= SpawnCorpseKeyDown;

            CorpseJuggler.Instance.OnMoveBallKeyDown -= MoveBallKeyDown;

            CorpseJuggler.Instance.OnCursorMoveHori -= MouseMoveHori;
            CorpseJuggler.Instance.OnCursorMoveVert -= MouseMoveVert;
        }

        void Awake()
        {
            CorpseJuggler.Instance.OnTrainingModeStart += TrainingModeStart;
            CorpseJuggler.Instance.OnTrainingModeEnd += TrainingModeEnded;

            CorpseJuggler.Instance.OnEnableCorpseSpawner += EnableCorpseSpawner;
        }

        private void MoveBallKeyDown()
        {
            if (ballMoveToggle == false)
            {
                CorpseJuggler.Instance.OnCursorMoveHori += MouseMoveHori;
                CorpseJuggler.Instance.OnCursorMoveVert += MouseMoveVert;
                ballMoveToggle = true;
            }
            else
            {
                ballMoveToggle = false;
                CorpseJuggler.Instance.OnCursorMoveHori -= MouseMoveHori;
                CorpseJuggler.Instance.OnCursorMoveVert -= MouseMoveVert;
            }
        }

        private void EnableCorpseSpawner()
        {
            bool inServeState = ball.ballData.ballState == BallState.STANDBY || ball.ballData.ballState == BallState.APPEAR;
            if (inServeState == false && ball.InHitstunByAPlayer() == false)
            {
                if (corpseMode == false)
                {
                    CorpseJuggler.Instance.OnMoveBallKeyDown += MoveBallKeyDown;
                    CorpseJuggler.Instance.OnSpawnCorpseKeyDown += SpawnCorpseKeyDown;
                    MoveBallKeyDown();
                    CorpseModeEnabled();
                }
                else
                {
                    CorpseModeDisabled();
                    CorpseJuggler.Instance.OnSpawnCorpseKeyDown -= SpawnCorpseKeyDown;
                    CorpseJuggler.Instance.OnMoveBallKeyDown -= MoveBallKeyDown;
                }
            }
        }

        void StickBallToPlayer()
        {
            ball.ballData.lastHitterIndex = 0;
            ball.SetToTeam(0);
            ball.SetBallState(BallState.STICK_TO_PLAYER_SERVE);
        }

        private void MouseMoveVert(float f, bool isGamePad)
        {
            ballMoveSpeed = isGamePad ? CorpseJuggler.Instance.joyStickBallMoveSpeed : CorpseJuggler.Instance.mouseBallMoveSpeed;
            ballMoveDirection.y = f;
        }

        private void MouseMoveHori(float f, bool isGamePad)
        {
            ballMoveSpeed = isGamePad ? CorpseJuggler.Instance.joyStickBallMoveSpeed : CorpseJuggler.Instance.mouseBallMoveSpeed;
            ballMoveDirection.x = f;
        }

        private void SpawnCorpseKeyDown()
        {
            SpawnCorpse();
        }

        private void CorpseModeEnabled()
        {
            ballPos = ball.GetPosition();
            ball.ballData.flyDirection = new IBGCBLLKIHA(0, 0);
            prevBallSpeed = ball.GetFlySpeed(true);
            ball.hitableData.canHitPlayer = false;
            ball.SetToTeam((BGHNEHPFHGC)4); //Set to Team NONE
            AudioHandler.PlaySfx(Sfx.MENU_CONFIRM);
            corpseMode = true;
        }

        private void CorpseModeDisabled()
        {
            if (expressCancelled)
            {
                RestoreControllerExpress(previousControllerMapID);
            }
            UnlockCursor();
            ballMoveToggle = false;
            expressCancelled = false;
            ball.SetFlySpeed(prevBallSpeed, true);
            ball.hitableData.canHitPlayer = true;
            ball.EndHitstun();
            ball.SetToTeam(0); //Set ball to Red Team
            ball.ballData.flyDirection = new IBGCBLLKIHA(0, 0);
            corpseMode = false;
            StickBallToPlayer();
            AudioHandler.PlaySfx(Sfx.MENU_SET);
        }

        void UpdateExpress()
        {
            if (UsingController && expressCancelled == false)
            {
                int i = CurrentControllerMapID;
                previousControllerMapID = i;
                DisableControllerExpress(previousControllerMapID);
                expressCancelled = true;
            }

            if (CurrentControllerMapID != previousControllerMapID && expressCancelled == true)
            {
                RestoreControllerExpress(previousControllerMapID);
                expressCancelled = false;
            }
        }

        void DisableControllerExpress(int mapID)
        {
            if (UsingController == true)
            {
                foreach (var name in Rewired.ReInput.mapping.GetControllerMap(mapID).AllMaps)
                {
                    if (!name.actionDescriptiveName.Contains("Express")) continue;
                    name.enabled = false;
                }
#if DEBUG
                Debug.Log($"[LLBMM] Spawner: Disabled Express for {mapID}"); 
#endif
            }
        }

        void RestoreControllerExpress(int mapID)
        {
            foreach (var name in Rewired.ReInput.mapping.GetControllerMap(mapID).AllMaps)
            {
                if (!name.actionDescriptiveName.Contains("Express")) continue;
                name.enabled = true;
            }
#if DEBUG
            Debug.Log($"[LLBMM] Spawner: Re-enabled Express for {mapID}"); 
#endif
        }

        private void SpawnCorpse()
        {
            if (!ballMoveToggle)
            {
                bool isRight = GreaterThanAndEqual(ball.GetPosition().GCPKPHMKLBN, player.GetPosition().GCPKPHMKLBN);
                ItemHandler.instance.SpawnCorpse(dummy.playerIndex, ball.GetPosition(), isRight ? Side.RIGHT : Side.LEFT);
            }
        }
        void CantHitBall()
        {
            if (ball.InHitstunByAPlayer()) return;
            ball.StartHitstun(new HHBCPNCDNDH(10), HitstunState.NONE);
            ball.SetPosition(ballPos);
            if (CorpseJuggler.CurrentGameState == JOFJHDJHJGI.LGILIJKMKOD && ballMoveToggle)
            {
                ball.SetColorOutlinesColor(new Color(1f, 1f, 0.4f));
                return;
            }
            ball.SetColorOutlinesColor(new Color(1f, 0.4f, 0.4f));
            ball.SetScale(new Vector3(2, 2, 2));
        }

        void MoveBall()
        {
            ball.EndHitstun();
            ball.hitableData.canBeHitByPlayer = false;
            ball.hitableData.canHitPlayer = false;
            ball.SetColorOutlinesColor(new Color(0.4f, 1f, 0.4f));
            ball.ballData.flyDirection = new IBGCBLLKIHA(Floatf(ballMoveDirection.x), Floatf(ballMoveDirection.y));
            ball.bouncingData.flySpeed = Floatf(ballMoveSpeed * 0.6f);
            ballMoveDirection = Vector2.zero;
            ballPos = ball.GetPosition();
        }

        private void LockCursor()
        {
            if (Cursor.lockState == CursorLockMode.None && mouseCursorLocked == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
                mouseCursorLocked = true;
            }
        }
        private void UnlockCursor()
        {
            if (Cursor.lockState >= CursorLockMode.Locked && mouseCursorLocked == true)
            {
                Cursor.lockState = CursorLockMode.None;
                mouseCursorLocked = false;
            }
        }

        void Update()
        {
            if (corpseMode == true)
            {
                UpdateExpress();
                if (ballMoveToggle == false || CorpseJuggler.CurrentGameState == JOFJHDJHJGI.LGILIJKMKOD)
                {
                    UnlockCursor();
                    CantHitBall();
                }
                else
                {
                    LockCursor();
                    MoveBall();
                }
            }
        }
        public static bool GreaterThanAndEqual(HHBCPNCDNDH a, HHBCPNCDNDH b)
        {
            return HHBCPNCDNDH.OCDKNPDIPOB(a, b);
        }

        public static HHBCPNCDNDH Floatf(float a)
        {
            return HHBCPNCDNDH.NKKIFJJEPOL((decimal)a);
        }

        const int GUI_HEIGHT = 1080;
        const int GUI_WIDTH = 1920;
        void OnGUI()
        {
            if (CorpseJuggler.CreditsEnabled && corpseMode)
            {
                GUITools.ScaleGUIToViewPort();
                GUIStyle label = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                };
                int height = 30;
                GUI.Label(new Rect(0, GUI_HEIGHT - height, 390, height), $"CorpseJuggler v{CorpseJuggler.modVersion} by {CorpseJuggler.repositoryOwner}", label);
            }
        }
    }
}