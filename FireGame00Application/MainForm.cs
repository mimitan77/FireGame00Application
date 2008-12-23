using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FireGame00Application
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.ClientSize = WindowSize;
        }

        /// <summary>
        /// ゲームタイプ
        /// </summary>
        enum GameType
        {
            /// <summary>
            /// タイトル画面
            /// </summary>
            Title,
            /// <summary>
            /// ゲーム画面
            /// </summary>
            Game,
            /// <summary>
            /// ハイスコア画面
            /// </summary>
            Hiscore,
        }

        #region 定数変数宣言
        /// <summary>
        /// 保存するファイルパス
        /// </summary>
        readonly string saveFilePath = Application.StartupPath + "\\save.xml";
        /// <summary>
        /// ウィンドウサイズ
        /// </summary>
        readonly Size WindowSize = new Size(800, 600);
        /// <summary>
        /// 標準重力加速度G(定数)
        /// </summary>
        const double Gravity = 9.80665;
        /// <summary>
        /// マウス速度減衰率
        /// </summary>
        const double MouseVelocity = 1 / 5.0;
        /// <summary>
        /// 基地の大きさ
        /// </summary>
        const int BaseSize = 20;
        /// <summary>
        /// 敵の一番前にくるX座標
        /// </summary>
        const int enemyStartX = 400;
        /// <summary>
        /// 障害物座標(頂上頂点、左側頂点、右側頂点)
        /// </summary>
        readonly Point[] syougaiObject = new Point[] { new Point(300, 400), new Point(200, 600), new Point(400, 600) };
        #endregion

        #region 変数宣言
        /// <summary>
        /// アプリの設定読み込み
        /// </summary>
        ApplicationSettings AppSet;
        /// <summary>
        /// オブジェクト変数
        /// </summary>
        SaveDataClass mset;
        /// <summary>
        /// バックバッファ
        /// </summary>
        Bitmap backbuffer;
        /// <summary>
        /// 現在のゲーム
        /// </summary>
        GameType nowGameType = GameType.Title;
        /// <summary>
        /// ハイスコア
        /// </summary>
        HiscoreClass hiScore = new HiscoreClass(10);
        /// <summary>
        /// ゲーム用時間
        /// </summary>
        System.Diagnostics.Stopwatch gameStopWatch = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// 弾を発射した回数
        /// </summary>
        int ShotNumber;
        /// <summary>
        /// 砲弾シミュレート用時間
        /// </summary>
        System.Diagnostics.Stopwatch timeStopwatch = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// マウスの位置
        /// </summary>
        Point mousePoint;
        /// <summary>
        /// 砲弾発射速度v0[m/s]
        /// </summary>
        double v0;
        /// <summary>
        /// 砲弾発射角度θ[rad]
        /// </summary>
        double angle;
        /// <summary>
        /// 砲弾現在のX座標
        /// </summary>
        double x;
        /// <summary>
        /// 砲弾現在のY座標
        /// </summary>
        double y;
        /// <summary>
        /// 自分の位置
        /// </summary>
        Rectangle myRectangle;
        /// <summary>
        /// 敵の位置
        /// </summary>
        RectangleF enemyRectangle;
        /// <summary>
        /// 1s間の敵の移動量
        /// </summary>
        float enemyMovedt = 0.5f;
        /// <summary>
        /// 敵が前に進んでいるか
        /// </summary>
        bool enemyMoveAdvance = true;
        #endregion

        //フォームがロードされようとしているとき、、、
        private void MainForm_Load(object sender, EventArgs e)
        {
            //タイトル変更
            this.Text = Application.ProductName + " Ver." + Application.ProductVersion;
            //バックバッファ作成
            backbuffer = new Bitmap(WindowSize.Width, WindowSize.Height);
            //自分の位置決定
            myRectangle = new Rectangle(10, WindowSize.Height - BaseSize, BaseSize, BaseSize);
            //敵の位置決定
            Random rnd = new Random();
            enemyRectangle = new Rectangle(rnd.Next(enemyStartX, WindowSize.Width - BaseSize), WindowSize.Height - BaseSize,
                BaseSize, BaseSize);
            //アプリケーションのデータを読み込む
            AppSet = new ApplicationSettings(saveFilePath);
            mset = (SaveDataClass)AppSet.LoadSetting(typeof(SaveDataClass));
            if (mset != null)
                for (int i = 0; i <= mset.shot.GetUpperBound(0); i++)
                    hiScore.Add(new ScoreClass(mset.shot[i], new TimeSpan(mset.gameTime[i]), new DateTime(mset.dateTime[i])));
            else
                mset = new SaveDataClass();

        }
        //フォームを閉じようとしているとき、、、
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ゲーム中の場合のさまざまな処理を一時停止させる。
            bool timeSW = timeStopwatch.IsRunning;
            timeStopwatch.Stop();
            gameStopWatch.Stop();
            //ユーザーに質問する
            DialogResult ans = MessageBox.Show("「" + Application.ProductName + "」を終了しますか。", "質問", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (ans == DialogResult.Yes)
            {
                mset.shot = hiScore.Shot;
                mset.gameTime = hiScore.GameTime;
                mset.dateTime = hiScore.DateTime;
                AppSet.SaveSettings(mset);
                return;
            }
            e.Cancel = true;
            //ゲーム中の場合のさまざな処理を再開させる。
            gameStopWatch.Start();
            if (timeSW)
                timeStopwatch.Start();
        }
        //マウスボタンが押されたとき、、、
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            //左ボタンでなければ、、、
            if (e.Button != MouseButtons.Left)
                return;
            //マウスの位置を保存する
            mousePoint = e.Location;
            //現在のゲームによって見分ける
            switch (nowGameType)
            {
                case GameType.Title://タイトル画面
                    {
                        //ゲームスタートならば、、、
                        if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            200 <= mousePoint.Y && mousePoint.Y <= 230)
                        {
                            nowGameType = GameType.Game;
                            ShotNumber = 0;
                            gameStopWatch.Reset();
                            gameStopWatch.Start();
                        }
                        else if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            250 <= mousePoint.Y && mousePoint.Y <= 280)//ハイスコアならば、、、
                            nowGameType = GameType.Hiscore;
                        else if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            300 <= mousePoint.Y && mousePoint.Y <= 330)//終了ならば、、、
                            Application.Exit();
                        break;
                    }
                case GameType.Game://ゲーム中
                    {
                        //自分の基地をクリックしたならば、、、
                        if (myRectangle.X <= mousePoint.X && mousePoint.X <= myRectangle.Right &&
                            myRectangle.Y <= mousePoint.Y && mousePoint.Y <= myRectangle.Bottom)
                        {
                            //ゲーム中のさまざまな処理を一時停止させる。
                            bool timeSW = timeStopwatch.IsRunning;
                            timeStopwatch.Stop();
                            gameStopWatch.Stop();
                            //ユーザーに質問する
                            DialogResult ans = MessageBox.Show("ゲームを終了しますか。", "質問",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (ans == DialogResult.Yes)
                                nowGameType = GameType.Title;
                            //ゲーム中のさまざな処理を再開させる。
                            gameStopWatch.Start();
                            if (timeSW)
                                timeStopwatch.Start();
                            break;
                        }
                        //指示線の長さから砲弾の初速度を計算する
                        v0 = Math.Sqrt(Math.Pow(myRectangle.X + (myRectangle.Width / 2) - mousePoint.X - mousePoint.X, 2) +
                            Math.Pow(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y, 2)) * MouseVelocity;
                        //指示線の角度から砲弾発射角度を計算する
                        angle = (180 * Math.PI / 180) -
                            Math.Atan2(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y,
                            myRectangle.X + (myRectangle.Width / 2) - mousePoint.X);
                        //砲弾シミュレート開始
                        timeStopwatch.Reset();
                        timeStopwatch.Start();
                        //弾を発射した回数を増やす
                        ShotNumber++;
                        break;
                    }
                case GameType.Hiscore://ハイスコア画面
                    nowGameType = GameType.Title;
                    break;
            }
        }
        //マウスが動いたとき
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousePoint = e.Location;
            //現在のゲームによって見分ける
            switch (nowGameType)
            {
                case GameType.Game://ゲーム中
                    {
                        //砲弾シミュレート中ならば、、、
                        if (timeStopwatch.IsRunning == false)
                        {
                            //指示線の長さから砲弾の初速度を計算する
                            v0 = Math.Sqrt(Math.Pow(myRectangle.X + (myRectangle.Width / 2) - mousePoint.X - mousePoint.X, 2) +
                                Math.Pow(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y, 2)) * MouseVelocity;
                            //指示線の角度から砲弾発射角度を計算する
                            angle = (180 * Math.PI / 180) -
                                Math.Atan2(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y,
                                myRectangle.X + (myRectangle.Width / 2) - mousePoint.X);
                        }
                        break;
                    }
            }
        }

        //タイマーの時間がたったとき、、、
        private void timer_Tick(object sender, EventArgs e)
        {
            //バックバッファに描画開始
            Graphics g = Graphics.FromImage(backbuffer);
            g.Clear(Color.White);

            //現在のゲームで見分ける
            switch (nowGameType)
            {
                case GameType.Title://タイトル
                    {
                        Font font = new Font(this.Font.FontFamily, 40, FontStyle.Bold);
                        g.DrawString(Application.ProductName, font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString(Application.ProductName, font).Width / 2, 20);

                        font = new Font(this.Font.FontFamily, 20);
                        string text = "ゲームスタート";
                        int titleY = 200;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        text = "ハイスコア";
                        titleY = 250;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        text = "終了";
                        titleY = 300;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        break;
                    }
                case GameType.Game://ゲーム中
                    {
                        //障害物を描画する
                        g.FillPolygon(Brushes.Green, syougaiObject);

                        //敵の基地を描画する
                        g.FillRectangle(Brushes.Black, enemyRectangle);
                        //敵の移動
                        if (gameStopWatch.IsRunning)
                        {
                            if (enemyMoveAdvance)
                            {
                                enemyRectangle.X += enemyMovedt;
                                if (enemyRectangle.X > WindowSize.Width - BaseSize)
                                    enemyMoveAdvance = false;
                            }
                            else
                            {
                                enemyRectangle.X -= enemyMovedt;
                                if (enemyRectangle.X < enemyStartX)
                                    enemyMoveAdvance = true;
                            }
                        }

                        //自分の基地を描画
                        g.FillRectangle(Brushes.Black, myRectangle);
                        //砲弾シミュレート中ならば、、、
                        if (timeStopwatch.IsRunning)
                        {
                            //現時点での時間を保存する
                            double time = timeStopwatch.Elapsed.TotalSeconds;
                            //現時点の砲弾位置を計算する
                            x = v0 * Math.Cos(angle) * time + myRectangle.X + (myRectangle.Width / 2);
                            y = v0 * Math.Sin(angle) * time - 0.5 * Gravity * Math.Pow(time, 2) +
                                (WindowSize.Height - myRectangle.Y - myRectangle.Height / 2);
                            //砲弾を描画する
                            g.FillPie(Brushes.Black, (float)x - 5, WindowSize.Height - (float)y - 5, 10, 10, 0, 360);
                            //あたり判定
                            if (enemyRectangle.IntersectsWith(new Rectangle((int)x - 5, WindowSize.Height - (int)y - 5, 10, 10)))
                            {
                                //時間を停止する
                                timeStopwatch.Stop();
                                //ゲーム時間を停止する
                                gameStopWatch.Stop();
                                //自分の位置決定
                                myRectangle = new Rectangle(10, WindowSize.Height - BaseSize, BaseSize, BaseSize);
                                //敵の位置決定
                                Random rnd = new Random();
                                enemyRectangle = new Rectangle(rnd.Next(enemyStartX, WindowSize.Width - BaseSize),
                                    WindowSize.Height - BaseSize,
                                    BaseSize, BaseSize);
                                //スコアに追加する。
                                int i = hiScore.Add(new ScoreClass(ShotNumber, gameStopWatch.Elapsed, DateTime.Now));
                                //ゲームタイプを変更
                                nowGameType = GameType.Title;
                                //砲撃完了したメッセージを表示する。
                                if (i == -1)
                                    MessageBox.Show("砲撃完了！\n残念ながらスコア外です。。。\n次はがんばってくださいぃ。。。\n\n" +
                                        "発射回数=" + ShotNumber + "回\n所要時間=" + gameStopWatch.Elapsed.ToString());
                                else
                                    MessageBox.Show("砲撃完了！\nすごいですぅ。。。\n" + (i + 1) + "位です！！さすがぁ。。。\n\n" +
                                        "発射回数=" + ShotNumber + "回\n所要時間=" + gameStopWatch.Elapsed.ToString());
                                break;
                            }
                            //砲弾が地面よりも下と右を超えてしまったならば、、、
                            if (y < 0 || x < 0 || x >= WindowSize.Width)
                                timeStopwatch.Stop();

                            //砲弾と障害物との当たり判定
                            if (syougaiObject[1].X <= x && x <= syougaiObject[2].X &&
                                syougaiObject[0].Y <= WindowSize.Height - y && WindowSize.Height - y <= syougaiObject[1].Y)
                            {
                                System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                                gp.AddPolygon(syougaiObject);
                                if (gp.IsVisible((int)x, WindowSize.Height - (int)y))
                                    timeStopwatch.Stop();
                                gp.Dispose();
                            }
                        }
                        else if (gameStopWatch.IsRunning)//ゲームが動いているならば、、、
                        {
                            //指示線を描画する
                            Pen pen = new Pen(Brushes.Black, 5);
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                            g.DrawLine(pen, myRectangle.X + (myRectangle.Width / 2), myRectangle.Y + (myRectangle.Height / 2),
                                mousePoint.X, mousePoint.Y);
                        }

                        //情報表示
                        int showV0 = (int)v0;
                        int showAngle = (int)(angle * 180 / Math.PI);
                        g.DrawString("初速度=" + showV0.ToString("d3") + "[m/s]\n角度=" + showAngle.ToString("d3") + "[°]" +
                             "\n発射回数=" + ShotNumber + "回\n自分の基地をクリックするとゲームを終了します。",
                             this.Font, Brushes.Red, 10, 10);
                        break;
                    }
                case GameType.Hiscore://ハイスコア画面
                    {
                        Font font = new Font(this.Font.FontFamily, 40, FontStyle.Bold);
                        g.DrawString("ハイスコア", font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString("ハイスコア", font).Width / 2, 20);
                        font = new Font(this.Font.FontFamily, 20);
                        g.DrawString("クリックするとタイトル画面に戻れます。", font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString("クリックするとタイトル画面に戻れます。", font).Width / 2, 70);

                        //水平線を描画する。
                        int yy = -1;
                        for (; yy <= 10; yy++)
                            g.DrawLine(new Pen(Brushes.Black), 50, yy * 30 + 200, 750, yy * 30 + 200);
                        //垂直線を描画する
                        g.DrawLine(new Pen(Brushes.Black), 50, 170, 50, 500);
                        g.DrawLine(new Pen(Brushes.Black), 120, 170, 120, 500);
                        g.DrawLine(new Pen(Brushes.Black), 240, 170, 240, 500);
                        g.DrawLine(new Pen(Brushes.Black), 500, 170, 500, 500);
                        g.DrawLine(new Pen(Brushes.Black), 750, 170, 750, 500);
                        //ヘッダを描画する
                        string text = "順位";
                        g.DrawString(text, font, Brushes.Black,
                            (120 - 50) / 2 + 50 - g.MeasureString(text, font).Width / 2, 170);
                        text = "発射回数";
                        g.DrawString(text, font, Brushes.Black,
                            (240 - 120) / 2 + 120 - g.MeasureString(text, font).Width / 2, 170);
                        text = "所要時間";
                        g.DrawString(text, font, Brushes.Black,
                            (500 - 240) / 2 + 240 - g.MeasureString(text, font).Width / 2, 170);
                        text = "達成日時";
                        g.DrawString(text, font, Brushes.Black,
                            (750 - 500) / 2 + 500 - g.MeasureString(text, font).Width / 2, 170);
                        yy = 0;
                        foreach (ScoreClass sc in hiScore)
                        {
                            text = (yy + 1).ToString();
                            g.DrawString(text, font, Brushes.Black,
                                (120 - 50) / 2 + 50 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = sc.ShotNumber.ToString();
                            g.DrawString(text, font, Brushes.Black,
                                (240 - 120) / 2 + 120 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = sc.GameTime.ToString();
                            g.DrawString(text, font, Brushes.Black,
                                (500 - 240) / 2 + 240 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = sc.dateTime.ToString();
                            g.DrawString(text, font, Brushes.Black,
                                (750 - 500) / 2 + 500 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            yy++;
                        }
                        for (; yy < 10; yy++)
                        {
                            text = (yy + 1).ToString();
                            g.DrawString(text, font, Brushes.Black,
                                (120 - 50) / 2 + 50 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = "----";
                            g.DrawString(text, font, Brushes.Black,
                                (240 - 120) / 2 + 120 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = "--:--:--.-----";
                            g.DrawString(text, font, Brushes.Black,
                                (500 - 240) / 2 + 240 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                            text = "----/--/-- --:--:--";
                            g.DrawString(text, font, Brushes.Black,
                                (750 - 500) / 2 + 500 - g.MeasureString(text, font).Width / 2, yy * 30 + 200);
                        }
                        break;
                    }
            }

            //デバッグメッセージ描画
#if DEBUG
            g.DrawString("DebugMessage Mouse=(" + mousePoint.X + "," + mousePoint.Y + ") TIME=" + timeStopwatch.Elapsed +
                " Fire=(" + (int)x + "," + (int)y + ")" + " GameTime=" + gameStopWatch.Elapsed, this.Font, Brushes.Black, 0, 0);
#endif

            //描画終了
            g.Dispose();
            //描画
            g = CreateGraphics();
            g.DrawImage(backbuffer, 0, 0);
            g.Dispose();
        }
    }

    /// <summary>
    /// セーブデータ
    /// </summary>
    public class SaveDataClass
    {
        /// <summary>
        /// 発射した回数配列[回]
        /// </summary>
        public int[] shot;
        /// <summary>
        /// ゲーム時間配列[100ns]
        /// </summary>
        public long[] gameTime;
        /// <summary>
        /// 達成日時[100ns]
        /// </summary>
        public long[] dateTime;
    }
}