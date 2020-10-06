using LLHandlers;
using LLModdingTools;
using ModMenuIntergration;
using UnityEngine;

namespace CorpseJuggler
{
    public class CorpseJuggler : MonoBehaviour
    {
#pragma warning disable IDE0051 // Remove unused private members
        public const string modVersion = "1.0.0";
        public const string repositoryOwner = "Daioutzu";
        public const string repositoryName = "LLBMM-CorpseJuggler";
#pragma warning restore IDE0051

        public static ModMenuIntegration MMI = null;
        private bool initialModIntegration;
        private Rewired.Controller Controller => Rewired.ReInput.controllers.GetLastActiveController();
        private Rewired.IGamepadTemplate gamePad;
        private KeyCode enableCorpseSpawner, spawnCorpseKey, moveBallKey;
        private static bool gamePadSpawnCorpse, gamePadenableCorpseSpawner, gamePadMoveBall;
        public int joyStickBallMoveSpeed;
        public readonly int mouseBallMoveSpeed = 18;

        public static bool CreditsEnabled { get; private set; }
        private const float DEAD_ZONE = 0.2f;
        private bool trainingModeStart = false;

        internal delegate void DirectionPressed(float f, bool isGamePad);
        internal delegate void StandardEvent();
        internal event DirectionPressed OnCursorMoveVert, OnCursorMoveHori;
        internal event StandardEvent OnTrainingModeStart, OnTrainingModeEnd, OnSpawnCorpseKeyDown, OnEnableCorpseSpawner, OnMoveBallKeyDown;
        internal static JOFJHDJHJGI CurrentGameState => DNPFJHMAIBP.HHMOGKIMBNM();
        internal static GameMode CurrentGameMode => JOMBNFKIHIC.GIGAKBJGFDI.PNJOKAICMNN;
        internal static bool InGame => World.instance != null && (DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.CDOFDJMLGLO || DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.LGILIJKMKOD) && !LLScreen.UIScreen.loadingScreenActive;
        internal static bool IsOffline => !JOMBNFKIHIC.GDNFJCCCKDM;
        internal static Spawner CorpseSpawner { get; private set; }
        internal static CorpseJuggler Instance { get; private set; }

        public static void Initialize()
        {
            GameObject gameObject = new GameObject("CorpseJuggler", typeof(CorpseJuggler), typeof(ModMenuIntegration));
            Instance = gameObject.GetComponent<CorpseJuggler>();
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Debug.Log("[LLBMM] CorpseJuggler Started");
            if (MMI == null)
            {
                MMI = gameObject.GetComponent<ModMenuIntegration>();
                Debug.Log("[LLBMM] CorpseJuggler: Added GameObject \"ModMenuIntegration\"");
            }
        }

        void ModMenuInit()
        {
            if ((MMI != null && initialModIntegration == false) || LLModMenu.ModMenu.Instance.inModSubOptions && LLModMenu.ModMenu.Instance.currentOpenMod == "CorpseJuggler")
            {
                PopulateModOptions();
                if (initialModIntegration == false)
                {
                    Debug.Log("[LLBMM] CorpseJuggler: Initial ModMenuIntegration Done");
                    initialModIntegration = true;
                };
            }
        }

        void PopulateModOptions()
        {
            enableCorpseSpawner = MMI.GetKeyCode(MMI.configKeys["(key)enableCorpseJugglerKey"]);
            spawnCorpseKey = MMI.GetKeyCode(MMI.configKeys["(key)spawnCorpseKey"]);
            moveBallKey = MMI.GetKeyCode(MMI.configKeys["(key)enableBallPositioningKey"]);

            gamePadenableCorpseSpawner = MMI.GetTrueFalse(MMI.configBools["(bool)enableCorpseSpawnerWithSelect"]);
            gamePadSpawnCorpse = MMI.GetTrueFalse(MMI.configBools["(bool)SpawnCorpseWithRightStickDown"]);
            gamePadMoveBall = MMI.GetTrueFalse(MMI.configBools["(bool)enableBallPositioningWithRightStickPressed"]);
            joyStickBallMoveSpeed = MMI.GetSliderValue("(slider)ballSpeedSlider");
            CreditsEnabled = MMI.GetTrueFalse(MMI.configBools["(bool)In-GameCredit"]);
        }

        void GetGamePad()
        {
            if (Controller?.ImplementsTemplate<Rewired.IGamepadTemplate>() != null)
            {
                gamePad = Controller.GetTemplate<Rewired.IGamepadTemplate>();
            }
        }

        void Update()
        {
            ModMenuInit();
            if (GameObject.Find("AdvancedTraining"))
            {
                return;
            }
            GetGamePad();

            bool selectPressed = (gamePad?.center1.justPressed ?? false) && gamePadenableCorpseSpawner == true;
            bool rightstickPressed = (gamePad?.rightStick.press.justPressed ?? false) && gamePadMoveBall == true;
            bool rightStickDown = !Input.anyKey && gamePad?.rightStick.value.y < -0.35 && !(gamePad?.rightStick.valuePrev.y < -0.35) && gamePadSpawnCorpse == true;

            if (CurrentGameMode == GameMode.TRAINING && InGame && IsOffline)
            {
                if (trainingModeStart == false)
                {
                    trainingModeStart = true;
                    TrainingModStart();
                }
            }
            else
            {
                if (trainingModeStart == true)
                {
                    trainingModeStart = false;
                    TrainingModeEnd();
                }
            }

            if (Input.GetKeyDown(enableCorpseSpawner) || selectPressed)
            {
                OnEnableCorpseSpawner?.Invoke();
            }
            else if (Input.GetKeyDown(spawnCorpseKey) || rightStickDown)
            {
                OnSpawnCorpseKeyDown?.Invoke();
            }
            else if (Input.GetKeyDown(moveBallKey) || rightstickPressed)
            {
                OnMoveBallKeyDown?.Invoke();
            }

            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.025f)
            {
                OnCursorMoveHori?.Invoke(Input.GetAxis("Mouse X"), false);
            }
            else if (Mathf.Abs(gamePad?.rightStick.value.x ?? 0) > DEAD_ZONE)
            {
                OnCursorMoveHori?.Invoke(gamePad.rightStick.value.x, true);
            }

            if (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.025f)
            {
                OnCursorMoveVert?.Invoke(Input.GetAxis("Mouse Y"), false);
            }
            else if (Mathf.Abs(gamePad?.rightStick.value.y ?? 0) > DEAD_ZONE)
            {
                OnCursorMoveVert?.Invoke(gamePad.rightStick.value.y, true);
            }
        }

        const int GUI_HEIGHT = 1080;
        const int GUI_WIDTH = 1920;
        void OnGUI()
        {
            if (GameObject.Find("AdvancedTraining"))
            {
                GUITools.ScaleGUIToViewPort();
                GUIStyle label = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                };
                int height = 60;
                GUI.Label(new Rect(0, GUI_HEIGHT - height, 390, height), $"Incompatible Mod \"AdvancedTraining - v1.3b\" Detected.\nDisabling \"CorpseJuggler\"", label);
            }
        }

        protected virtual void TrainingModStart()
        {
            CorpseSpawner = World.instance.gameObject.AddComponent<Spawner>();
            OnTrainingModeStart?.Invoke();
        }
        protected virtual void TrainingModeEnd()
        {
            OnTrainingModeEnd?.Invoke();
            CorpseSpawner = null;
        }
    }
}
