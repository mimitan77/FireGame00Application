using System;
using System.Collections.Generic;
using System.Text;

namespace FireGame00Application
{
    /// <summary>
    /// スコアを管理するクラスです。
    /// </summary>
    class ScoreClass
    {
        /// <summary>
        /// 一時　発射した回数
        /// </summary>
        int _shotNumber;
        /// <summary>
        /// 一時　ゲーム時間
        /// </summary>
        TimeSpan _gameTime;
        /// <summary>
        /// 一時　スコアを取った日時
        /// </summary>
        DateTime _dateTime;

        /// <summary>
        /// スコアを保存するインスタンスを作成します。
        /// </summary>
        /// <param name="shotNumber">発射した回数</param>
        /// <param name="gameTime">ゲーム時間</param>
        /// <param name="dateTime">スコアを取った日時</param>
        public ScoreClass(int shotNumber, TimeSpan gameTime, DateTime dateTime)
        {
            _shotNumber = shotNumber;
            _gameTime = gameTime;
            _dateTime = dateTime;
        }

        /// <summary>
        /// 発射した回数を取得します。
        /// </summary>
        public int ShotNumber
        {
            get
            {
                return _shotNumber;
            }
        }
        /// <summary>
        /// ゲーム時間を取得します。
        /// </summary>
        public TimeSpan GameTime
        {
            get
            {
                return _gameTime;
            }
        }
        /// <summary>
        /// スコアを取った日時を取得します。
        /// </summary>
        public DateTime dateTime
        {
            get
            {
                return _dateTime;
            }
        }
    }

    /// <summary>
    /// ハイスコアを管理するクラスです。
    /// </summary>
    class HiscoreClass
    {
        /// <summary>
        /// リスト
        /// </summary>
        List<ScoreClass> list;
        /// <summary>
        /// 保存するハイスコアの数
        /// </summary>
        int _max;

        /// <summary>
        /// ハイスコアを保持するインスタンスを作成します。
        /// </summary>
        /// <param name="max">保存するハイスコアの数</param>
        public HiscoreClass(int max)
        {
            list = new List<ScoreClass>();
            _max = max;
        }

        /// <summary>
        /// 保存ファイルからセーブするときに使う発射回数配列
        /// </summary>
        public int[] Shot
        {
            get
            {
                int[] data = new int[list.Count];
                for (int i = 0; i < list.Count; i++)
                    data[i] = list[i].ShotNumber;
                return data;
            }
        }
        /// <summary>
        /// 保存ファイルからセーブするときに使うゲーム時間配列
        /// </summary>
        public long[] GameTime
        {
            get
            {
                long[] data = new long[list.Count];
                for (int i = 0; i < list.Count; i++)
                    data[i] = list[i].GameTime.Ticks;
                return data;
            }
        }
        /// <summary>
        /// 保存ファイルからセーブするときに使う達成日時配列
        /// </summary>
        public long[] DateTime
        {
            get
            {
                long[] data = new long[list.Count];
                for (int i = 0; i < list.Count; i++)
                    data[i] = list[i].dateTime.Ticks;
                return data;
            }
        }

        /// <summary>
        /// ハイスコアにスコアを追加します。
        /// (ただし、ただ追加するのではなく保存するハイスコアの数に入るスコアを指定された場所に保存します。)
        /// </summary>
        /// <param name="score">追加するスコア</param>
        /// <returns>追加された位置を返します。(ただし範囲外などで追加されなかった場合は-1を返します。)</returns>
        public int Add(ScoreClass score)
        {
            DateTime dt = score.dateTime;
            list.Add(score);
            list.Sort(new Comparer());
            if (list.Count > _max)
                list.RemoveAt(_max);
            int i;
            for (i = 0; i < list.Count; i++)
                if (list[i].dateTime.Ticks == dt.Ticks)
                    break;
            if (i >= list.Count)
                i = -1;
            return i;
        }

        public IEnumerator<ScoreClass> GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
    
    /// <summary>
    /// 並び替える方法を定義するクラス
    /// IComparerインターフェイスを実装する
    /// </summary>
    class Comparer : System.Collections.Generic.IComparer<ScoreClass>
    {
        public int Compare(ScoreClass x, ScoreClass y)
        {
            int n = x.ShotNumber - y.ShotNumber;
            if (n == 0)
                n = x.GameTime.CompareTo(y.GameTime);
            if (n == 0)
                n = x.dateTime.CompareTo(y.dateTime);
            return n;
        }
    }
}
