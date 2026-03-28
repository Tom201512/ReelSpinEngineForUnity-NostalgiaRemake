using ReelSpinGame_Option.Components;
using ReelSpinGame_Option.MenuContent;
using System;
using System.Collections;
using UnityEngine;

namespace ReelSpinGame_Option.MenuBar
{
    // メニューリストの管理マネージャー
    public class MenuManager : MonoBehaviour
    {
        // 各種ボタン
        [SerializeField] ButtonComponent howToPlayButton;       // 遊び方ガイド
        [SerializeField] ButtonComponent slotDataButton;        // スロット情報画面
        [SerializeField] ButtonComponent forceFlagButton;       // 強制フラグ
        [SerializeField] ButtonComponent autoSettingButton;     // オート設定
        [SerializeField] ButtonComponent otherSettingButton;    // その他設定 

        // 各種画面
        [SerializeField] HowToPlayScreen howToPlayScreen;               // 遊び方ガイドの画面
        [SerializeField] SlotDataScreen slotDataScreen;                 // スロット情報画面
        [SerializeField] ForceFlagScreen forceFlagScreen;               // 強制役設定画面
        [SerializeField] AutoPlaySettingScreen autoPlaySettingScreen;   // オート設定画面
        [SerializeField] OtherSettingScreen otherSettingScreen;         // その他設定画面

        // 何かしらのボタンを押したときのイベント
        public delegate void PressesMenu();
        public event PressesMenu PressedMenuEvent;

        // 何かしらの画面を閉じたときのイベント
        public delegate void ClosedScreen();
        public event ClosedScreen ClosedScreenEvent;

        public bool CanInteract { get; set; }           // 操作ができる状態か(アニメーション中などはつけないこと)
        public bool HasOptionLock { get; set; }         // オプションがロックされているか

        private CanvasGroup canvasGroup;        // フェードイン、アウト用

        void Awake()
        {
            // ボタン登録
            howToPlayButton.ButtonPushedEvent += HowToPlayOpen;
            slotDataButton.ButtonPushedEvent += SlotDataOpen;
            forceFlagButton.ButtonPushedEvent += ForceFlagOpen;
            autoSettingButton.ButtonPushedEvent += AutoPlayOpen;
            otherSettingButton.ButtonPushedEvent += OtherSettingOpen;

            howToPlayScreen.ClosedScreenEvent += HowToPlayClose;
            slotDataScreen.ClosedScreenEvent += SlotDataClose;
            forceFlagScreen.ClosedScreenEvent += ForceFlagClose;
            autoPlaySettingScreen.ClosedScreenEvent += AutoPlayClose;
            otherSettingScreen.ClosedScreenEvent += OtherSettingClose;
            CanInteract = false;
            HasOptionLock = false;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            // 画面を非表示にする
            SetInteractiveAllButton(false);
            howToPlayScreen.gameObject.SetActive(false);
            slotDataScreen.gameObject.SetActive(false);
            forceFlagScreen.gameObject.SetActive(false);
            autoPlaySettingScreen.gameObject.SetActive(false);
            otherSettingScreen.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            // ボタン登録解除
            howToPlayButton.ButtonPushedEvent -= HowToPlayOpen;
            slotDataButton.ButtonPushedEvent -= SlotDataOpen;
            forceFlagButton.ButtonPushedEvent -= ForceFlagOpen;
            autoSettingButton.ButtonPushedEvent -= AutoPlayOpen;
            otherSettingButton.ButtonPushedEvent -= OtherSettingOpen;

            howToPlayScreen.ClosedScreenEvent -= HowToPlayClose;
            slotDataScreen.ClosedScreenEvent -= SlotDataClose;
            forceFlagScreen.ClosedScreenEvent -= ForceFlagClose;
            autoPlaySettingScreen.ClosedScreenEvent -= AutoPlayClose;
            otherSettingScreen.ClosedScreenEvent -= OtherSettingClose;

            StopAllCoroutines();
        }

        // 全メニューのロック設定
        public void SetInteractiveAllButton(bool value)
        {
            howToPlayButton.ToggleInteractive(value);
            slotDataButton.ToggleInteractive(value);
            forceFlagButton.ToggleInteractive(value);
            autoSettingButton.ToggleInteractive(value);
            otherSettingButton.ToggleInteractive(value);
        }

        // メニューを開く
        public void OpenScreen()
        {
            StartCoroutine(nameof(FadeInBehavior));
        }

        // 画面を閉じる
        public void CloseScreen()
        {
            SetInteractiveAllButton(false);
            StartCoroutine(nameof(FadeOutBehavior));
        }

        // 遊び方ガイドを開いた時の処理
        void HowToPlayOpen(int signalID)
        {
            howToPlayScreen.gameObject.SetActive(true);
            howToPlayScreen.OpenScreen();
            OpenScreenBehavior();
        }

        // 遊び方ガイドを閉じた時の処理
        void HowToPlayClose()
        {
            howToPlayScreen.CloseScreen();
            howToPlayScreen.gameObject.SetActive(false);
            CloseScreenBehavior();
        }

        // スロット情報を開いた時の処理
        void SlotDataOpen(int signalID)
        {
            slotDataScreen.gameObject.SetActive(true);
            slotDataScreen.OpenScreen();
            OpenScreenBehavior();
        }

        // スロット情報を閉じた時の処理
        void SlotDataClose()
        {
            slotDataScreen.CloseScreen();
            slotDataScreen.gameObject.SetActive(false);
            CloseScreenBehavior();
        }

        // 強制役設定画面を開いた時の処理
        void ForceFlagOpen(int signalID)
        {
            forceFlagScreen.gameObject.SetActive(true);
            forceFlagScreen.OpenScreen();
            OpenScreenBehavior();
        }

        // 強制役設定画面を閉じた時の処理
        void ForceFlagClose()
        {
            forceFlagScreen.CloseScreen();
            forceFlagScreen.gameObject.SetActive(false);
            CloseScreenBehavior();
        }

        // オートプレイ設定画面を開いた時の処理
        void AutoPlayOpen(int signalID)
        {
            autoPlaySettingScreen.gameObject.SetActive(true);
            autoPlaySettingScreen.OpenScreen();
            OpenScreenBehavior();
        }

        // オートプレイ設定画面を閉じた時の処理
        void AutoPlayClose()
        {
            autoPlaySettingScreen.CloseScreen();
            autoPlaySettingScreen.gameObject.SetActive(false);
            CloseScreenBehavior();
        }

        // オートプレイ設定画面を開いた時の処理
        void OtherSettingOpen(int signalID)
        {
            otherSettingScreen.gameObject.SetActive(true);
            otherSettingScreen.OpenScreen();
            OpenScreenBehavior();
        }

        // オートプレイ設定画面を閉じた時の処理
        void OtherSettingClose()
        {
            otherSettingScreen.CloseScreen();
            otherSettingScreen.gameObject.SetActive(false);
            CloseScreenBehavior();
        }

        // 画面を開いたときの処理
        void OpenScreenBehavior()
        {
            SetInteractiveAllButton(false);
            PressedMenuEvent?.Invoke();
        }

        // 画面を閉じたときの処理
        void CloseScreenBehavior()
        {
            SetInteractiveAllButton(true);
            ClosedScreenEvent?.Invoke();
        }

        // フェードイン
        IEnumerator FadeInBehavior()
        {
            canvasGroup.alpha = 0;
            float fadeSpeed = Time.deltaTime / OptionScreenFade.FadeTime;

            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha = Math.Clamp(canvasGroup.alpha + fadeSpeed, 0f, 1f);
                yield return new WaitForEndOfFrame();
            }

            CanInteract = true;
            SetInteractiveAllButton(!HasOptionLock);
        }

        // フェードアウト
        IEnumerator FadeOutBehavior()
        {
            canvasGroup.alpha = 1;
            float fadeSpeed = Time.deltaTime / OptionScreenFade.FadeTime;

            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = Math.Clamp(canvasGroup.alpha - fadeSpeed, 0f, 1f);
                yield return new WaitForEndOfFrame();
            }

            CanInteract = false;
        }
    }
}
