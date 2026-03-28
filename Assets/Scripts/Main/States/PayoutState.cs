using ReelSpinGame_AutoPlay;
using ReelSpinGame_Interface;
using ReelSpinGame_Lots;
using ReelSpinGame_Reels;
using static ReelSpinGame_Bonus.BonusSystemData;
using static ReelSpinGame_Payout.PayoutManager;

namespace ReelSpinGame_State.PayoutState
{
    // 払い出しステート
    public class PayoutState : IGameStatement
    {
        private GameManager gM;         // ゲームマネージャ

        public PayoutState(GameManager gameManager)
        {
            gM = gameManager;
        }
        public void StateStart()
        {
            // 成立時の出目を記録する(ただし表示するのは入賞した後)
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone &&
                (gM.Lots.GetCurrentFlag() == FlagID.FlagBig || gM.Lots.GetCurrentFlag() == FlagID.FlagReg))
            {
                gM.Player.SetBonusHitPos(gM.Reel.GetLastStoppedReelData().LastPos);
                gM.Player.SetBonusPushOrder(gM.Reel.GetLastStoppedReelData().LastPushOrder);
                gM.Player.SetBonusHitDelay(gM.Reel.GetLastStoppedReelData().LastReelDelay);
            }

            // 高速オートが解除されたかチェック
            gM.Auto.CheckFastAutoCancelled();

            // 払い出し確認
            gM.Payout.CheckPayouts(gM.Medal.LastBetAmount, gM.Reel.GetLastStoppedReelData());

            // 小役カウンタの増減(通常時のみ)
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone)
            {
                if (gM.Payout.LastPayoutResult.Payout > 0 || gM.Payout.LastPayoutResult.IsReplayOrJacIn)
                {
                    gM.Lots.IncreaseCounter(gM.Payout.LastPayoutResult.Payout);
                }
            }

            // 小役成立回数の記録
            gM.Player.PlayerAnalyticsData.IncreaseHitCountByFlag(gM.Lots.GetCurrentFlag(), gM.Bonus.GetCurrentBonusStatus());

            // 小役入賞回数の記録(払い出しがあれば)
            if (gM.Payout.LastPayoutResult.Payout > 0 || gM.Medal.HasReplay)
            {
                gM.Player.PlayerAnalyticsData.IncreaseLineUpCountByFlag(gM.Lots.GetCurrentFlag(), gM.Bonus.GetCurrentBonusStatus());
            }

            // ボーナス開始をチェック
            CheckStartBonus();

            // 払い出し開始
            PayoutUpdate();

            // ボーナス終了をチェック
            CheckBonusEnd();

            // ボーナス状態の更新
            BonusStatusUpdate();

            // JACハズシの記録
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusBIGGames && gM.Lots.GetCurrentFlag() == FlagID.FlagReplayJacIn)
            {
                gM.Player.PlayerAnalyticsData.CountJacAvoidCounts(gM.Reel.GetLastPushedLowerPos((int)ReelID.ReelLeft), gM.Reel.GetRandomValue());
            }

            // 払い出し開始
            gM.Medal.StartPayout(gM.Payout.LastPayoutResult.Payout, gM.Auto.HasAuto && gM.Auto.CurrentSpeed > AutoSpeedName.Normal);
            // リプレイ処理
            UpdateReplay();

            // オート残りゲーム数が0になったかチェック
            gM.Auto.DecreaseAutoSpin();

            // 通常時のみの処理
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone)
            {
                // 差枚数のカウント(通常時のみ)
                gM.Player.PlayerMedalData.CountCurrentSlumpGraph();
                // 回転数が打ち止め規定数に達していたらオート終了
                gM.Auto.CheckAutoEndByLimitReached(gM.Player.TotalGames);
            }

            // セーブ処理
            SaveData();
            // オプション設定の反映
            gM.Option.SetForceFlagSetting(gM.Bonus.GetCurrentBonusStatus(), gM.Bonus.GetHoldingBonusID());
            // 演出処理へ
            gM.MainFlow.StateManager.ChangeState(gM.MainFlow.EffectState);
        }

        public void StateUpdate()
        {

        }

        public void StateEnd()
        {

        }

        // データのセーブ
        void SaveData()
        {
            gM.PlayerSave.RecordPlayerSave(gM.Player.MakeSaveData());
            gM.PlayerSave.RecordMedalSave(gM.Medal.MakeSaveData());
            gM.PlayerSave.RecordFlagCounter(gM.Lots.GetCounter());
            gM.PlayerSave.RecordReelPos(gM.Reel.GetLastStoppedReelData().LastPos);
            gM.PlayerSave.RecordBonusData(gM.Bonus.MakeSaveData());
        }

        // リプレイ処理
        void UpdateReplay()
        {
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone &&
                    gM.Payout.LastPayoutResult.IsReplayOrJacIn)
            {
                // 最後に賭けた枚数をOUTに反映
                gM.Player.PlayerMedalData.IncreaseOutMedal(gM.Medal.LastBetAmount);
                gM.Medal.EnableReplay();
            }
            else if (gM.Medal.HasReplay)
            {
                gM.Medal.DisableReplay();
            }
        }

        // 払い出し処理
        void PayoutUpdate()
        {
            // プレイヤーメダルの増加、OUT枚数の増加(データのみ変更)
            gM.Player.PlayerMedalData.IncreasePlayerMedal(gM.Payout.LastPayoutResult.Payout);
            gM.Player.PlayerMedalData.IncreaseOutMedal(gM.Payout.LastPayoutResult.Payout);

            // ボーナス中なら各ボーナスの払い出しを増やす
            // また、払い出し時にボーナスが揃っていればボーナス払い出しを増やす
            if (gM.Bonus.GetCurrentBonusStatus() != BonusStatus.BonusNone ||
                gM.Payout.LastPayoutResult.BonusID != 0)
            {
                gM.Bonus.ChangeBonusPayout(gM.Payout.LastPayoutResult.Payout);
                gM.Player.ChangeLastBonusPayout(gM.Bonus.GetCurrentBonusPayout());
            }
            // ゾーン区間(50G)にいる間はその払い出しを計算
            if (gM.Bonus.GetHasZone())
            {
                gM.Bonus.ChangeZonePayout(gM.Payout.LastPayoutResult.Payout);
            }
        }

        // ボーナスをストックさせる
        void StockBonus()
        {
            // ボーナス未成立でいずれかのボーナスが成立した場合はストック
            if (gM.Bonus.GetHoldingBonusID() == BonusTypeID.BonusNone)
            {
                if (gM.Lots.GetCurrentFlag() == FlagID.FlagBig)
                {
                    gM.Bonus.SetBonusStock(BonusTypeID.BonusBIG);
                }
                if (gM.Lots.GetCurrentFlag() == FlagID.FlagReg)
                {
                    gM.Bonus.SetBonusStock(BonusTypeID.BonusREG);
                }
            }
        }

        // ボーナス開始をチェック
        void CheckStartBonus()
        {
            // 通常時
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone)
            {
                // ボーナスがあればボーナス開始
                if (gM.Payout.LastPayoutResult.BonusID != (int)BonusTypeID.BonusNone)
                {
                    StartBonus();
                }
                // 取りこぼした場合はストックさせる
                else
                {
                    StockBonus();
                }

                // オートがあり、条件がボーナス成立なら終了判定
                if (gM.Auto.HasAuto)
                {
                    gM.Auto.CheckAutoEndByBonus(gM.Payout.LastPayoutResult.BonusID);
                }
            }

            // 連チャン区間の処理
            // 50Gを迎えた場合は連チャン区間を終了させる(但しボーナス非成立時のみ)
            if (gM.Player.CurrentGames == MaxZoneGames &&
                gM.Bonus.GetHoldingBonusID() == BonusTypeID.BonusNone)
            {
                gM.Bonus.ResetZonePayout();
            }
        }

        //　ボーナス開始
        void StartBonus()
        {
            // リールから揃ったボーナス図柄を得る
            gM.Bonus.ResetBigType();
            BigType color = gM.Reel.GetBigLinedUpCount(gM.Medal.LastBetAmount, 3);

            // ビッグチャンスの場合
            if (gM.Payout.LastPayoutResult.BonusID == (int)BonusTypeID.BonusBIG)
            {
                gM.Bonus.StartBigChance(color);
                gM.Player.SetLastBigChanceColor(color);
            }
            // ボーナスゲームの場合
            else if (gM.Payout.LastPayoutResult.BonusID == (int)BonusTypeID.BonusREG)
            {
                gM.Bonus.StartBonusGame();
            }

            // カウンタリセット
            gM.Lots.ResetCounter();
            // 入賞時ゲーム数を記録
            gM.Player.SetLastBonusStart();
        }

        // ボーナス終了をチェック
        void CheckBonusEnd()
        {
            if (gM.Bonus.GetCurrentBonusStatus() != BonusStatus.BonusNone)
            {
                if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusBIGGames)
                {
                    gM.Bonus.CheckBigGameStatus(gM.Payout.LastPayoutResult.IsReplayOrJacIn);
                }
                else if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusJACGames)
                {
                    gM.Bonus.CheckBonusGameStatus(gM.Payout.LastPayoutResult.Payout > 0);
                }

                // オートがあり終了条件がボーナス終了時の場合はここで判定する
                if (gM.Auto.HasAuto)
                {
                    gM.Auto.CheckAutoEndByBonusFinish((int)gM.Bonus.GetCurrentBonusStatus());
                }
            }
        }

        // ボーナス状態によるデータ変更
        void BonusStatusUpdate()
        {
            // ビッグチャンス中に移行した場合はMAXBETを3, BIG中の抽選、払出テーブルへ変更
            if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusBIGGames)
            {
                gM.Lots.ChangeTable(FlagLotTable.BigBonus);
                gM.Payout.ChangePayoutCheckMode(PayoutCheckMode.PayoutBIG);
                gM.Medal.ChangeMaxBet(3);
                gM.Lots.ResetCounter();
            }
            // ボーナスゲーム中に移行した場合はMAXBETを1, JAC中の抽選、払出テーブルへ変更
            else if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusJACGames)
            {
                gM.Medal.ChangeMaxBet(1);
                gM.Lots.ChangeTable(FlagLotTable.JacGame);
                gM.Payout.ChangePayoutCheckMode(PayoutCheckMode.PayoutJAC);
            }
            // 通常時に移行した場合はMAXBETを3, 通常時の抽選、払出テーブルへ変更
            else if (gM.Bonus.GetCurrentBonusStatus() == BonusStatus.BonusNone)
            {
                gM.Lots.ChangeTable(FlagLotTable.Normal);
                gM.Payout.ChangePayoutCheckMode(PayoutCheckMode.PayoutNormal);
                gM.Medal.ChangeMaxBet(3);
            }
        }
    }
}