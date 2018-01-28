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
        /// �Q�[���^�C�v
        /// </summary>
        enum GameType
        {
            /// <summary>
            /// �^�C�g�����
            /// </summary>
            Title,
            /// <summary>
            /// �Q�[�����
            /// </summary>
            Game,
            /// <summary>
            /// �n�C�X�R�A���
            /// </summary>
            Hiscore,
        }

        #region �萔�ϐ��錾
        /// <summary>
        /// �ۑ�����t�@�C���p�X
        /// </summary>
        readonly string saveFilePath = Application.StartupPath + "\\save.xml";
        /// <summary>
        /// �E�B���h�E�T�C�Y
        /// </summary>
        readonly Size WindowSize = new Size(800, 600);
        /// <summary>
        /// �W���d�͉����xG(�萔)
        /// </summary>
        const double Gravity = 9.80665;
        /// <summary>
        /// �}�E�X���x������
        /// </summary>
        const double MouseVelocity = 1 / 5.0;
        /// <summary>
        /// ��n�̑傫��
        /// </summary>
        const int BaseSize = 20;
        /// <summary>
        /// �G�̈�ԑO�ɂ���X���W
        /// </summary>
        const int enemyStartX = 400;
        /// <summary>
        /// ��Q�����W(���㒸�_�A�������_�A�E�����_)
        /// </summary>
        readonly Point[] syougaiObject = new Point[] { new Point(300, 400), new Point(200, 600), new Point(400, 600) };
        #endregion

        #region �ϐ��錾
        /// <summary>
        /// �A�v���̐ݒ�ǂݍ���
        /// </summary>
        ApplicationSettings AppSet;
        /// <summary>
        /// �I�u�W�F�N�g�ϐ�
        /// </summary>
        SaveDataClass mset;
        /// <summary>
        /// �o�b�N�o�b�t�@
        /// </summary>
        Bitmap backbuffer;
        /// <summary>
        /// ���݂̃Q�[��
        /// </summary>
        GameType nowGameType = GameType.Title;
        /// <summary>
        /// �n�C�X�R�A
        /// </summary>
        HiscoreClass hiScore = new HiscoreClass(10);
        /// <summary>
        /// �Q�[���p����
        /// </summary>
        System.Diagnostics.Stopwatch gameStopWatch = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// �e�𔭎˂�����
        /// </summary>
        int ShotNumber;
        /// <summary>
        /// �C�e�V�~�����[�g�p����
        /// </summary>
        System.Diagnostics.Stopwatch timeStopwatch = new System.Diagnostics.Stopwatch();
        /// <summary>
        /// �}�E�X�̈ʒu
        /// </summary>
        Point mousePoint;
        /// <summary>
        /// �C�e���ˑ��xv0[m/s]
        /// </summary>
        double v0;
        /// <summary>
        /// �C�e���ˊp�x��[rad]
        /// </summary>
        double angle;
        /// <summary>
        /// �C�e���݂�X���W
        /// </summary>
        double x;
        /// <summary>
        /// �C�e���݂�Y���W
        /// </summary>
        double y;
        /// <summary>
        /// �����̈ʒu
        /// </summary>
        Rectangle myRectangle;
        /// <summary>
        /// �G�̈ʒu
        /// </summary>
        RectangleF enemyRectangle;
        /// <summary>
        /// 1s�Ԃ̓G�̈ړ���
        /// </summary>
        float enemyMovedt = 0.5f;
        /// <summary>
        /// �G���O�ɐi��ł��邩
        /// </summary>
        bool enemyMoveAdvance = true;
        #endregion

        //�t�H�[�������[�h����悤�Ƃ��Ă���Ƃ��A�A�A
        private void MainForm_Load(object sender, EventArgs e)
        {
            //�^�C�g���ύX
            this.Text = Application.ProductName + " Ver." + Application.ProductVersion;
            //�o�b�N�o�b�t�@�쐬
            backbuffer = new Bitmap(WindowSize.Width, WindowSize.Height);
            //�����̈ʒu����
            myRectangle = new Rectangle(10, WindowSize.Height - BaseSize, BaseSize, BaseSize);
            //�G�̈ʒu����
            Random rnd = new Random();
            enemyRectangle = new Rectangle(rnd.Next(enemyStartX, WindowSize.Width - BaseSize), WindowSize.Height - BaseSize,
                BaseSize, BaseSize);
            //�A�v���P�[�V�����̃f�[�^��ǂݍ���
            AppSet = new ApplicationSettings(saveFilePath);
            mset = (SaveDataClass)AppSet.LoadSetting(typeof(SaveDataClass));
            if (mset != null)
                for (int i = 0; i <= mset.shot.GetUpperBound(0); i++)
                    hiScore.Add(new ScoreClass(mset.shot[i], new TimeSpan(mset.gameTime[i]), new DateTime(mset.dateTime[i])));
            else
                mset = new SaveDataClass();

        }
        //�t�H�[������悤�Ƃ��Ă���Ƃ��A�A�A
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //�Q�[�����̏ꍇ�̂��܂��܂ȏ������ꎞ��~������B
            bool timeSW = timeStopwatch.IsRunning;
            timeStopwatch.Stop();
            gameStopWatch.Stop();
            //���[�U�[�Ɏ��₷��
            DialogResult ans = MessageBox.Show("�u" + Application.ProductName + "�v���I�����܂����B", "����", MessageBoxButtons.YesNo,
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
            //�Q�[�����̏ꍇ�̂��܂��ȏ������ĊJ������B
            gameStopWatch.Start();
            if (timeSW)
                timeStopwatch.Start();
        }
        //�}�E�X�{�^���������ꂽ�Ƃ��A�A�A
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            //���{�^���łȂ���΁A�A�A
            if (e.Button != MouseButtons.Left)
                return;
            //�}�E�X�̈ʒu��ۑ�����
            mousePoint = e.Location;
            //���݂̃Q�[���ɂ���Č�������
            switch (nowGameType)
            {
                case GameType.Title://�^�C�g�����
                    {
                        //�Q�[���X�^�[�g�Ȃ�΁A�A�A
                        if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            200 <= mousePoint.Y && mousePoint.Y <= 230)
                        {
                            nowGameType = GameType.Game;
                            ShotNumber = 0;
                            gameStopWatch.Reset();
                            gameStopWatch.Start();
                        }
                        else if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            250 <= mousePoint.Y && mousePoint.Y <= 280)//�n�C�X�R�A�Ȃ�΁A�A�A
                            nowGameType = GameType.Hiscore;
                        else if (WindowSize.Width / 2 - 100 <= mousePoint.X && mousePoint.X <= WindowSize.Width / 2 + 100 &&
                            300 <= mousePoint.Y && mousePoint.Y <= 330)//�I���Ȃ�΁A�A�A
                            Application.Exit();
                        break;
                    }
                case GameType.Game://�Q�[����
                    {
                        //�����̊�n���N���b�N�����Ȃ�΁A�A�A
                        if (myRectangle.X <= mousePoint.X && mousePoint.X <= myRectangle.Right &&
                            myRectangle.Y <= mousePoint.Y && mousePoint.Y <= myRectangle.Bottom)
                        {
                            //�Q�[�����̂��܂��܂ȏ������ꎞ��~������B
                            bool timeSW = timeStopwatch.IsRunning;
                            timeStopwatch.Stop();
                            gameStopWatch.Stop();
                            //���[�U�[�Ɏ��₷��
                            DialogResult ans = MessageBox.Show("�Q�[�����I�����܂����B", "����",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (ans == DialogResult.Yes)
                                nowGameType = GameType.Title;
                            //�Q�[�����̂��܂��ȏ������ĊJ������B
                            gameStopWatch.Start();
                            if (timeSW)
                                timeStopwatch.Start();
                            break;
                        }
                        //�w�����̒�������C�e�̏����x���v�Z����
                        v0 = Math.Sqrt(Math.Pow(myRectangle.X + (myRectangle.Width / 2) - mousePoint.X - mousePoint.X, 2) +
                            Math.Pow(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y, 2)) * MouseVelocity;
                        //�w�����̊p�x����C�e���ˊp�x���v�Z����
                        angle = (180 * Math.PI / 180) -
                            Math.Atan2(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y,
                            myRectangle.X + (myRectangle.Width / 2) - mousePoint.X);
                        //�C�e�V�~�����[�g�J�n
                        timeStopwatch.Reset();
                        timeStopwatch.Start();
                        //�e�𔭎˂����񐔂𑝂₷
                        ShotNumber++;
                        break;
                    }
                case GameType.Hiscore://�n�C�X�R�A���
                    nowGameType = GameType.Title;
                    break;
            }
        }
        //�}�E�X���������Ƃ�
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousePoint = e.Location;
            //���݂̃Q�[���ɂ���Č�������
            switch (nowGameType)
            {
                case GameType.Game://�Q�[����
                    {
                        //�C�e�V�~�����[�g���Ȃ�΁A�A�A
                        if (timeStopwatch.IsRunning == false)
                        {
                            //�w�����̒�������C�e�̏����x���v�Z����
                            v0 = Math.Sqrt(Math.Pow(myRectangle.X + (myRectangle.Width / 2) - mousePoint.X - mousePoint.X, 2) +
                                Math.Pow(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y, 2)) * MouseVelocity;
                            //�w�����̊p�x����C�e���ˊp�x���v�Z����
                            angle = (180 * Math.PI / 180) -
                                Math.Atan2(myRectangle.Y + (myRectangle.Height / 2) - mousePoint.Y,
                                myRectangle.X + (myRectangle.Width / 2) - mousePoint.X);
                        }
                        break;
                    }
            }
        }

        //�^�C�}�[�̎��Ԃ��������Ƃ��A�A�A
        private void timer_Tick(object sender, EventArgs e)
        {
            //�o�b�N�o�b�t�@�ɕ`��J�n
            Graphics g = Graphics.FromImage(backbuffer);
            g.Clear(Color.White);

            //���݂̃Q�[���Ō�������
            switch (nowGameType)
            {
                case GameType.Title://�^�C�g��
                    {
                        Font font = new Font(this.Font.FontFamily, 40, FontStyle.Bold);
                        g.DrawString(Application.ProductName, font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString(Application.ProductName, font).Width / 2, 20);

                        font = new Font(this.Font.FontFamily, 20);
                        string text = "�Q�[���X�^�[�g";
                        int titleY = 200;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        text = "�n�C�X�R�A";
                        titleY = 250;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        text = "�I��";
                        titleY = 300;
                        g.DrawRectangle(new Pen(Brushes.Black), WindowSize.Width / 2 - 100, titleY, 200, 30);
                        g.DrawString(text, font, Brushes.Black, WindowSize.Width / 2 - g.MeasureString(text, font).Width / 2, titleY);
                        break;
                    }
                case GameType.Game://�Q�[����
                    {
                        //��Q����`�悷��
                        g.FillPolygon(Brushes.Green, syougaiObject);

                        //�G�̊�n��`�悷��
                        g.FillRectangle(Brushes.Black, enemyRectangle);
                        //�G�̈ړ�
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

                        //�����̊�n��`��
                        g.FillRectangle(Brushes.Black, myRectangle);
                        //�C�e�V�~�����[�g���Ȃ�΁A�A�A
                        if (timeStopwatch.IsRunning)
                        {
                            //�����_�ł̎��Ԃ�ۑ�����
                            double time = timeStopwatch.Elapsed.TotalSeconds;
                            //�����_�̖C�e�ʒu���v�Z����
                            x = v0 * Math.Cos(angle) * time + myRectangle.X + (myRectangle.Width / 2);
                            y = v0 * Math.Sin(angle) * time - 0.5 * Gravity * Math.Pow(time, 2) +
                                (WindowSize.Height - myRectangle.Y - myRectangle.Height / 2);
                            //�C�e��`�悷��
                            g.FillPie(Brushes.Black, (float)x - 5, WindowSize.Height - (float)y - 5, 10, 10, 0, 360);
                            //�����蔻��
                            if (enemyRectangle.IntersectsWith(new Rectangle((int)x - 5, WindowSize.Height - (int)y - 5, 10, 10)))
                            {
                                //���Ԃ��~����
                                timeStopwatch.Stop();
                                //�Q�[�����Ԃ��~����
                                gameStopWatch.Stop();
                                //�����̈ʒu����
                                myRectangle = new Rectangle(10, WindowSize.Height - BaseSize, BaseSize, BaseSize);
                                //�G�̈ʒu����
                                Random rnd = new Random();
                                enemyRectangle = new Rectangle(rnd.Next(enemyStartX, WindowSize.Width - BaseSize),
                                    WindowSize.Height - BaseSize,
                                    BaseSize, BaseSize);
                                //�X�R�A�ɒǉ�����B
                                int i = hiScore.Add(new ScoreClass(ShotNumber, gameStopWatch.Elapsed, DateTime.Now));
                                //�Q�[���^�C�v��ύX
                                nowGameType = GameType.Title;
                                //�C�������������b�Z�[�W��\������B
                                if (i == -1)
                                    MessageBox.Show("�C�������I\n�c�O�Ȃ���X�R�A�O�ł��B�B�B\n���͂���΂��Ă����������B�B�B\n\n" +
                                        "���ˉ�=" + ShotNumber + "��\n���v����=" + gameStopWatch.Elapsed.ToString());
                                else
                                    MessageBox.Show("�C�������I\n�������ł����B�B�B\n" + (i + 1) + "�ʂł��I�I���������B�B�B\n\n" +
                                        "���ˉ�=" + ShotNumber + "��\n���v����=" + gameStopWatch.Elapsed.ToString());
                                break;
                            }
                            //�C�e���n�ʂ������ƉE�𒴂��Ă��܂����Ȃ�΁A�A�A
                            if (y < 0 || x < 0 || x >= WindowSize.Width)
                                timeStopwatch.Stop();

                            //�C�e�Ə�Q���Ƃ̓����蔻��
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
                        else if (gameStopWatch.IsRunning)//�Q�[���������Ă���Ȃ�΁A�A�A
                        {
                            //�w������`�悷��
                            Pen pen = new Pen(Brushes.Black, 5);
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                            g.DrawLine(pen, myRectangle.X + (myRectangle.Width / 2), myRectangle.Y + (myRectangle.Height / 2),
                                mousePoint.X, mousePoint.Y);
                        }

                        //���\��
                        int showV0 = (int)v0;
                        int showAngle = (int)(angle * 180 / Math.PI);
                        g.DrawString("�����x=" + showV0.ToString("d3") + "[m/s]\n�p�x=" + showAngle.ToString("d3") + "[��]" +
                             "\n���ˉ�=" + ShotNumber + "��\n�����̊�n���N���b�N����ƃQ�[�����I�����܂��B",
                             this.Font, Brushes.Red, 10, 10);
                        break;
                    }
                case GameType.Hiscore://�n�C�X�R�A���
                    {
                        Font font = new Font(this.Font.FontFamily, 40, FontStyle.Bold);
                        g.DrawString("�n�C�X�R�A", font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString("�n�C�X�R�A", font).Width / 2, 20);
                        font = new Font(this.Font.FontFamily, 20);
                        g.DrawString("�N���b�N����ƃ^�C�g����ʂɖ߂�܂��B", font, Brushes.Black,
                            WindowSize.Width / 2 - g.MeasureString("�N���b�N����ƃ^�C�g����ʂɖ߂�܂��B", font).Width / 2, 70);

                        //��������`�悷��B
                        int yy = -1;
                        for (; yy <= 10; yy++)
                            g.DrawLine(new Pen(Brushes.Black), 50, yy * 30 + 200, 750, yy * 30 + 200);
                        //��������`�悷��
                        g.DrawLine(new Pen(Brushes.Black), 50, 170, 50, 500);
                        g.DrawLine(new Pen(Brushes.Black), 120, 170, 120, 500);
                        g.DrawLine(new Pen(Brushes.Black), 240, 170, 240, 500);
                        g.DrawLine(new Pen(Brushes.Black), 500, 170, 500, 500);
                        g.DrawLine(new Pen(Brushes.Black), 750, 170, 750, 500);
                        //�w�b�_��`�悷��
                        string text = "����";
                        g.DrawString(text, font, Brushes.Black,
                            (120 - 50) / 2 + 50 - g.MeasureString(text, font).Width / 2, 170);
                        text = "���ˉ�";
                        g.DrawString(text, font, Brushes.Black,
                            (240 - 120) / 2 + 120 - g.MeasureString(text, font).Width / 2, 170);
                        text = "���v����";
                        g.DrawString(text, font, Brushes.Black,
                            (500 - 240) / 2 + 240 - g.MeasureString(text, font).Width / 2, 170);
                        text = "�B������";
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

            //�f�o�b�O���b�Z�[�W�`��
#if DEBUG
            g.DrawString("DebugMessage Mouse=(" + mousePoint.X + "," + mousePoint.Y + ") TIME=" + timeStopwatch.Elapsed +
                " Fire=(" + (int)x + "," + (int)y + ")" + " GameTime=" + gameStopWatch.Elapsed, this.Font, Brushes.Black, 0, 0);
#endif

            //�`��I��
            g.Dispose();
            //�`��
            g = CreateGraphics();
            g.DrawImage(backbuffer, 0, 0);
            g.Dispose();
        }
    }

    /// <summary>
    /// �Z�[�u�f�[�^
    /// </summary>
    public class SaveDataClass
    {
        /// <summary>
        /// ���˂����񐔔z��[��]
        /// </summary>
        public int[] shot;
        /// <summary>
        /// �Q�[�����Ԕz��[100ns]
        /// </summary>
        public long[] gameTime;
        /// <summary>
        /// �B������[100ns]
        /// </summary>
        public long[] dateTime;
    }
}