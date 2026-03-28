using ReelSpinGame_Option.Components;
using ReelSpinGame_Option.MenuBar;
using ReelSpinGame_Option.MenuContent;
using ReelSpinGame_Save.Database.Option;
using UnityEngine;

using static ReelSpinGame_Bonus.BonusSystemData;

namespace ReelSpinGame_Option
{
    // オプションマネージャー
    public class OptionManager : MonoBehaviour
    {
        [SerializeField] private ButtonComponent openButton;    // メニュー開閉ボタン
        [SerializeField] private MenuManager menuBarUI;         // メニューバーのUI

        // 各種設定画面のデータ
        [SerializeField] ForceFlagScreen forceFlagScreen;               // 強制フラグ
        [SerializeField] AutoPlaySettingScreen autoPlaySettingScreen;   // オートプレイ
        [SerializeField] OtherSettingScreen otherSettingScreen;         // その他設定

        public bool HasOptionScreen { get; private set; }       // オプション画面を開いているか(UIボタンの表示に使用する)
        public bool HasOptionMode { get; private set; }         // 設定変更中か(ゲームの操作ができなくなる)
        public bool LockOptionMode { get; private set; }        // 設定が開けない状態か(リール回転中やオート実行中は設定を開けない)

        public bool DeletePlayerSave { get => otherSettingScreen.DeletePlayerSave; }  // プレイヤーセーブを消すか
        public bool DeleteOptionSave { get => otherSettingScreen.DeleteOptionSave; }  // 設定セーブを消すか

        public AutoOptionData AutoOptionData { get => autoPlaySettingScreen.GetAutoSettingData(); }
        public OtherOptionData OtherOptionData { get => otherSettingScreen.GetSettingData(); }

        // 設定変更時のイベント
        // オート設定
        public delegate void AutoSettingChanged();
        public event AutoSettingChanged AutoSettingChangedEvent;

        // システム設定
        public delegate void OtherSettingChanged();
        public event OtherSettingChanged OtherSettingChangedEvent;

        // 終了が実行された時の処理
        public delegate void GameExit();
        public event GameExit GameExitEvent;

        void Awake()
        {
            HasOptionScreen = false;
            HasOptionMode = false;
            LockOptionMode = false;

            // イベント登録
            openButton.ButtonPushedEvent += ToggleOptionScreen;
            menuBarUI.PressedMenuEvent += EnterOptionMode;
            menuBarUI.ClosedScreenEvent += DisableOptionMode;
            autoPlaySettingScreen.SettingChangedEvent += OnAutoSettingChanged;
            otherSettingScreen.SettingChangedEvent += OnOtherSettingChanged;
            otherSettingScreen.GameExitEvent += OnGameExit;
        }

        void Start()
        {
            // ボタン有効化設定を変更
            openButton.ToggleInteractive(true);
        }

        void OnDestroy()
        {
            // イベント登録解除
            openButton.ButtonPushedEvent -= ToggleOptionScreen;
            menuBarUI.PressedMenuEvent -= EnterOptionMode;
            menuBarUI.ClosedScreenEvent -= DisableOptionMode;
            autoPlaySettingScreen.SettingChangedEvent -= OnAutoSettingChanged;
            otherSettingScreen.SettingChangedEvent -= OnOtherSettingChanged;
            otherSettingScreen.GameExitEvent -= OnGameExit;
        }

        // オプション画面を開く
        public void ToggleOptionScreen(int signalID)
        {
            if (openButton.CanInteractable)
            {
                if (menuBarUI.CanInteract)
                {
                    menuBarUI.CloseScreen();
                }
                else
                {
                    menuBarUI.OpenScreen();
                }
            }
        }

        // メニュー開閉ボタンのロック
        public void SetOpenButtonInteractive(bool value) => openButton.ToggleInteractive(value);

        // 設定変更のロック
        public void ToggleOptionLock(bool value)
        {
            LockOptionMode = value;
            menuBarUI.HasOptionLock = value;
            if (menuBarUI.CanInteract)
            {
                menuBarUI.SetInteractiveAllButton(!value);
            }
        }

        // 各画面の設定情報取得
        public int GetForceFlagSelectID() => forceFlagScreen.CurrentSelectFlagID; // 選択したフラグ値
        public int GetForceFlagRandomID() => forceFlagScreen.CurrentSelectRandomID; // 選択したランダム値   

        // 各画面の設定情報変更
        // セーブからオート設定を読み込む
        public void LoadAutoSettingFromSave(AutoOptionData autoOptionData) => autoPlaySettingScreen.LoadSettingData(autoOptionData);

        // その他設定を読み込む
        public void LoadOtherSettingFromSave(OtherOptionData otherOptionData) => otherSettingScreen.LoadSettingData(otherOptionData);

        // 強制フラグのボタン有効化設定を変更
        public void SetForceFlagSetting(BonusStatus currentBonusStatus, BonusTypeID holdingBonusID) => forceFlagScreen.SetBonusStatus(currentBonusStatus, holdingBonusID);

        // 強制フラグの設定リセット
        public void ResetForceFlagSetting() => forceFlagScreen.ResetFlagSetting();

        // オプションモードに入れる
        void EnterOptionMode()
        {
            HasOptionMode = true;
            openButton.ToggleInteractive(false);
        }

        // オプションモード解除
        void DisableOptionMode()
        {
            HasOptionMode = false;
            openButton.ToggleInteractive(true);
        }

        // オート設定が変更された時の処理
        void OnAutoSettingChanged() => AutoSettingChangedEvent?.Invoke();

        // その他設定が変更された時の処理
        void OnOtherSettingChanged() => OtherSettingChangedEvent?.Invoke();

        // ゲームが終了したときの処理
        void OnGameExit() => GameExitEvent?.Invoke();
    }
}
