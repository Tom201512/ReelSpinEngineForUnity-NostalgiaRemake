using ReelSpinGame_Interface;
using ReelSpinGame_Lamps;
using ReelSpinGame_Save.Bonus;
using UnityEngine;
using static ReelSpinGame_Bonus.BonusSystemData;

namespace ReelSpinGame_Bonus
{
    // ボーナス処理
    public class BonusManager : MonoBehaviour, IHasSave<BonusSave>
    {
        [SerializeField] private BonusSevenSegment bonusSegments;       // ボーナス状態のセグメント

        private BonusSystemData data;        // ボーナス処理のデータ

        void Awake()
        {
            data = new BonusSystemData();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        // 各種数値を得る
        public BonusTypeID GetHoldingBonusID() => data.HoldingBonusID;              // ストック中のボーナス
        public BonusStatus GetCurrentBonusStatus() => data.CurrentBonusStatus;      // ボーナス状態
        public BigType GetBigChanceType() => data.BigChanceType;                 // BIGボーナス当選時の種類

        public int GetRemainingBigGames() => data.RemainingBigGames;                // 残り小役ゲーム数
        public int GetRemainingJacIn() => data.RemainingJacIn;                      // 残りJAC-IN回数
        public int GetRemainingJacGames() => data.RemainingJacGames;                // 残りJACゲーム数
        public int GetRemainingJacHits() => data.RemainingJacHits;                  // 残りJACゲーム当選回数

        public int GetCurrentBonusPayout() => data.CurrentBonusPayout;      // 獲得した枚数を表示
        public int GetCurrentZonePayout() => data.CurrentZonePayout;        // 連チャン区間中の枚数を表示
        public bool GetHasZone() => data.HasZone;                           // 連チャン区間にいるか
        public int GetLastZonePayout() => data.LastZonePayout;              // 最終連チャン区間での枚数

        public bool GetHasBonusStarted() => data.HasBonusStarted;           // ボーナスが開始したかを得る
        public bool GetHasBonusFinished() => data.HasBonusFinished;         // ボーナスが終了したかを得る

        // 獲得枚数の増減
        public void ChangeBonusPayout(int amount) => data.CurrentBonusPayout += amount;
        public void ChangeZonePayout(int amount) => data.CurrentZonePayout += amount;

        public bool GetIsDisplayingPayout() => bonusSegments.IsDisplaying;   // ボーナス獲得枚数を表示中か

        // ボーナスを開始したかを変更
        public void SetHasBonusStarted(bool value) => data.HasBonusStarted = value;
        public void SetHasBonusFinished(bool value) => data.HasBonusFinished = value;

        // 連チャン区間枚数を消す
        public void ResetZonePayout()
        {
            // 最後に獲得した連チャン枚数を表示
            data.LastZonePayout = data.CurrentZonePayout;
            data.HasZone = false;
            data.CurrentZonePayout = 0;
        }

        // セーブデータにする
        public BonusSave MakeSaveData()
        {
            BonusSave save = new BonusSave();
            save.RecordData(data);

            return save;
        }

        // セーブを読み込む
        public void LoadSaveData(BonusSave loadData)
        {
            data.HoldingBonusID = loadData.HoldingBonusID;
            data.CurrentBonusStatus = loadData.CurrentBonusStatus;
            data.BigChanceType = loadData.BigChanceType;
            data.RemainingBigGames = loadData.RemainingBigGames;
            data.RemainingJacIn = loadData.RemainingJacIn;
            data.RemainingJacHits = loadData.RemainingJacHits;
            data.RemainingJacGames = loadData.RemainingJacGames;
            data.CurrentBonusPayout = loadData.CurrentBonusPayout;
            data.CurrentZonePayout = loadData.CurrentZonePayout;
            data.LastZonePayout = loadData.LastZonePayout;
            data.HasZone = loadData.HasZone;
        }

        // ボーナスストック状態の更新
        public void SetBonusStock(BonusTypeID bonusType) => data.HoldingBonusID = bonusType;

        // ビッグチャンスの開始
        public void StartBigChance(BigType bigColor)
        {
            // ビッグチャンスの初期化
            data.CurrentBonusPayout = 0;
            data.RemainingBigGames = BigGames;
            data.RemainingJacIn = JacInTimes;
            data.CurrentBonusStatus = BonusStatus.BonusBIGGames;
            data.HoldingBonusID = BonusTypeID.BonusNone;
            data.BigChanceType = bigColor;
            // 連チャン区間の記録開始
            data.HasZone = true;
            data.HasBonusStarted = true;
        }

        // ボーナスゲームの開始
        public void StartBonusGame()
        {
            // 残りJAC-INがあれば減らす
            if (data.RemainingJacIn > 0)
            {
                data.RemainingJacIn -= 1;
            }
            // BIG中でない場合はボーナス払い出し枚数リセット
            if (data.CurrentBonusStatus != BonusStatus.BonusBIGGames)
            {
                data.CurrentBonusPayout = 0;
                data.HasBonusStarted = true;
            }

            // ボーナスゲームの初期化
            data.RemainingJacGames = JacGames;
            data.RemainingJacHits = JacHits;
            data.CurrentBonusStatus = BonusStatus.BonusJACGames;
            data.HoldingBonusID = BonusTypeID.BonusNone;
        }

        // ボーナス状態のセグメント更新
        public void UpdateSegments()
        {
            // BIG中
            if (data.CurrentBonusStatus == BonusStatus.BonusBIGGames)
            {
                bonusSegments.ShowBigStatus(data.RemainingJacIn, data.RemainingBigGames);
            }
            // JAC中
            else if (data.CurrentBonusStatus == BonusStatus.BonusJACGames)
            {
                bonusSegments.ShowJacStatus(data.RemainingJacIn + 1, data.RemainingJacHits);
            }
            // 通常時に戻った場合は獲得枚数表示とリセット
            else if (data.HasBonusFinished)
            {
                bonusSegments.StartDisplayBonusPayout(data.CurrentBonusPayout, data.CurrentZonePayout, data.HasZone);
            }
        }

        // セグメントをすべて消す
        public void TurnOffSegments()
        {
            bonusSegments.TurnOffAllSegments();
            if (bonusSegments.IsDisplaying)
            {
                bonusSegments.EndDisplayBonusPayout();
            }
        }

        // 小役ゲーム数、JACゲーム数を減らす
        public void DecreaseGames()
        {
            if (data.CurrentBonusStatus == BonusStatus.BonusBIGGames)
            {
                data.RemainingBigGames -= 1;
            }
            else if (data.CurrentBonusStatus == BonusStatus.BonusJACGames)
            {
                data.RemainingJacGames -= 1;
            }
        }

        // 小役ゲーム中の状態遷移
        public void CheckBigGameStatus(bool hasJacIn)
        {
            // JAC-INなら
            if (hasJacIn)
            {
                StartBonusGame();
            }
            // 30ゲームを消化した場合
            else if (data.RemainingBigGames == 0)
            {
                EndBonusStatus();
            }
        }

        // ボーナスゲームの状態遷移
        public void CheckBonusGameStatus(bool hasPayout)
        {
            // JAC役が当選(払い出しがあった)場合は残り入賞回数を減らす
            if (hasPayout)
            {
                data.RemainingJacHits -= 1;
            }
            // JACゲーム数が0, または入賞回数が0の場合は終了(BIG中なら残りゲーム数0で終了)
            if (data.RemainingJacGames == 0 || data.RemainingJacHits == 0)
            {
                // BIG中なら残りJAC-INの数があれば小役ゲームへ移行
                if (data.RemainingJacIn > 0 && data.RemainingBigGames > 0)
                {
                    data.CurrentBonusStatus = BonusStatus.BonusBIGGames;
                }
                else
                {
                    EndBonusStatus();
                }
            }
        }

        // BIG時の種類をリセットする
        public void ResetBigType() => data.BigChanceType = BigType.None;

        // ボーナスの終了処理
        void EndBonusStatus()
        {
            data.RemainingBigGames = 0;
            data.RemainingJacIn = 0;
            data.RemainingJacGames = 0;
            data.RemainingJacHits = 0;
            data.CurrentBonusStatus = BonusStatus.BonusNone;
            data.HasBonusFinished = true;
        }
    }
}