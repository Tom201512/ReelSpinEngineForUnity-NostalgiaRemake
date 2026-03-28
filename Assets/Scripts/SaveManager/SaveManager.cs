using ReelSpinGame_Save.Database;
using ReelSpinGame_Save.Encryption;
using ReelSpinGame_Save.Util;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ReelSpinGame_System
{
    // セーブ機能
    public class SaveManager
    {
        const string PlayerSavePath = "/Nostalgia/save.sav"; // プレイヤーセーブ
        const string PlayerKeyPath = "/Nostalgia/save.key"; // プレイヤー暗号鍵
        const string OptionSavePath = "/Nostalgia/core.sav"; // オプションセーブ
        const string OptionKeyPath = "/Nostalgia/core.key"; // オプション暗号鍵

        // アドレス番地
        private enum AddressID
        {
            Setting,
            Player,
            Medal,
            FlagC,
            Reel,
            Bonus,
        }

        // プレイヤーのセーブデータ
        public SaveDatabase PlayerSaveData
        {
            get { return playerSaveManager.CurrentSave; }
        }
        // オプションのセーブデータ
        public OptionSave OptionSave
        {
            get { return optionSaveManager.CurrentSave; }
        }

        PlayerSaveManager playerSaveManager;        // プレイヤーのセーブマネージャー
        OptionSaveManager optionSaveManager;        // オプションのセーブマネージャー
        SaveEncryptor saveEncryptor;                // 暗号化機能
        SaveDecryptor saveDecryptor;                // 複合化機能
        HashChecker hashChecker;                    // ハッシュ値チェック

        public SaveManager()
        {
            saveEncryptor = new SaveEncryptor();
            playerSaveManager = new PlayerSaveManager();
            optionSaveManager = new OptionSaveManager();
            saveDecryptor = new SaveDecryptor();
            hashChecker = new HashChecker();
        }

        // セーブフォルダ作成
        public bool GenerateSaveFolder()
        {
            string path = Application.persistentDataPath + "/Nostalgia";

            // フォルダがあるか確認、ない場合は作成
            if (Directory.Exists(path))
            {
                return false;
            }
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return true;
        }

        // プレイヤーセーブ削除
        public void DeletePlayerSave()
        {
            string path = Application.persistentDataPath + PlayerSavePath;
            string keyPath = Application.persistentDataPath + PlayerKeyPath;
            DeleteSave(path, keyPath);
        }

        // プレイヤーセーブファイル作成
        public bool GeneratePlayerSave()
        {
            string path = Application.persistentDataPath + PlayerSavePath;
            string keyPath = Application.persistentDataPath + PlayerKeyPath;

            // 前のセーブを消去
            if (File.Exists(path))
            {
                DeleteSave(path, keyPath);
            }

            try
            {
                using (FileStream file = File.OpenWrite(path))
                using (StreamWriter sw = new StreamWriter(file))
                {
                    List<int> dataBuffer = new List<int>(playerSaveManager.GenerateDataBuffer());
                    // すべての数値を書き込んだらバイト配列にし、暗号化して保存
                    sw.Write(saveEncryptor.EncryptData(BitConverter.ToString(ByteArrayUtil.GetBytesFromList(dataBuffer)), keyPath));
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return true;
        }

        // プレイヤーセーブ読み込み
        public bool LoadPlayerSave()
        {
            string path = Application.persistentDataPath + PlayerSavePath;
            string keyPath = Application.persistentDataPath + PlayerKeyPath;

            // ファイルがない場合は読み込まない
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                // ファイルの復号化をする
                using (FileStream file = File.OpenRead(path))
                {
                    string playerData = saveDecryptor.DecodeFile(file, keyPath); // プレイヤーのデータ

                    // 文字列をバイト配列に戻し復元開始。ハッシュ値参照も行う
                    using (MemoryStream ms = new MemoryStream(ByteArrayUtil.GetBytesFromString(playerData)))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        using (Stream baseStream = br.BaseStream)
                        {
                            // ハッシュ値が正しければデータ読み込み
                            if (hashChecker.CheckHash(baseStream, br))
                            {
                                playerSaveManager.LoadDataBuffer(baseStream, br);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return true;
        }

        // オプションセーブファイル作成
        public bool GenerateOptionSave()
        {
            string path = Application.persistentDataPath + OptionSavePath;
            string keyPath = Application.persistentDataPath + OptionKeyPath;

            // 前のセーブを消去
            if (File.Exists(path))
            {
                DeleteSave(path, keyPath);
            }
            try
            {
                using (FileStream file = File.OpenWrite(path))
                using (StreamWriter sw = new StreamWriter(file))
                {
                    List<int> dataBuffer = new List<int>(optionSaveManager.GenerateDataBuffer());
                    // すべての数値を書き込んだらバイト配列にし、暗号化して保存
                    sw.Write(saveEncryptor.EncryptData(BitConverter.ToString(ByteArrayUtil.GetBytesFromList(dataBuffer)), keyPath));
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

            return true;
        }

        // オプションファイル読み込み
        public bool LoadOptionSave()
        {
            string path = Application.persistentDataPath + OptionSavePath;
            string keyPath = Application.persistentDataPath + OptionKeyPath;

            // ファイルがない場合は読み込まない
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                // ファイルの復号化をする
                using (FileStream file = File.OpenRead(path))
                {
                    string optionData = saveDecryptor.DecodeFile(file, keyPath); // 設定データ

                    // 文字列をバイト配列に戻し復元開始。ハッシュ値参照も行う
                    using (MemoryStream ms = new MemoryStream(ByteArrayUtil.GetBytesFromString(optionData)))
                    {
                        using (BinaryReader br = new BinaryReader(ms))
                        using (Stream baseStream = br.BaseStream)
                        {
                            // ハッシュ値が正しければデータ読み込み
                            if (hashChecker.CheckHash(baseStream, br))
                            {
                                optionSaveManager.LoadDataBuffer(baseStream, br);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        // オプションセーブ削除
        public void DeleteOptionSave()
        {
            string path = Application.persistentDataPath + OptionSavePath;
            string keyPath = Application.persistentDataPath + OptionKeyPath;
            DeleteSave(path, keyPath);
        }

        // ファイル削除
        bool DeleteSave(string path, string keyPath)
        {
            try
            {
                File.Delete(path);
                File.Delete(keyPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }
    }
}
