using System;
using System.Collections.Generic;
using System.Text;

namespace FireGame00Application
{
    /// <summary>
    /// �X�R�A���Ǘ�����N���X�ł��B
    /// </summary>
    class ScoreClass
    {
        /// <summary>
        /// �ꎞ�@���˂�����
        /// </summary>
        int _shotNumber;
        /// <summary>
        /// �ꎞ�@�Q�[������
        /// </summary>
        TimeSpan _gameTime;
        /// <summary>
        /// �ꎞ�@�X�R�A�����������
        /// </summary>
        DateTime _dateTime;

        /// <summary>
        /// �X�R�A��ۑ�����C���X�^���X���쐬���܂��B
        /// </summary>
        /// <param name="shotNumber">���˂�����</param>
        /// <param name="gameTime">�Q�[������</param>
        /// <param name="dateTime">�X�R�A�����������</param>
        public ScoreClass(int shotNumber, TimeSpan gameTime, DateTime dateTime)
        {
            _shotNumber = shotNumber;
            _gameTime = gameTime;
            _dateTime = dateTime;
        }

        /// <summary>
        /// ���˂����񐔂��擾���܂��B
        /// </summary>
        public int ShotNumber
        {
            get
            {
                return _shotNumber;
            }
        }
        /// <summary>
        /// �Q�[�����Ԃ��擾���܂��B
        /// </summary>
        public TimeSpan GameTime
        {
            get
            {
                return _gameTime;
            }
        }
        /// <summary>
        /// �X�R�A��������������擾���܂��B
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
    /// �n�C�X�R�A���Ǘ�����N���X�ł��B
    /// </summary>
    class HiscoreClass
    {
        /// <summary>
        /// ���X�g
        /// </summary>
        List<ScoreClass> list;
        /// <summary>
        /// �ۑ�����n�C�X�R�A�̐�
        /// </summary>
        int _max;

        /// <summary>
        /// �n�C�X�R�A��ێ�����C���X�^���X���쐬���܂��B
        /// </summary>
        /// <param name="max">�ۑ�����n�C�X�R�A�̐�</param>
        public HiscoreClass(int max)
        {
            list = new List<ScoreClass>();
            _max = max;
        }

        /// <summary>
        /// �ۑ��t�@�C������Z�[�u����Ƃ��Ɏg�����ˉ񐔔z��
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
        /// �ۑ��t�@�C������Z�[�u����Ƃ��Ɏg���Q�[�����Ԕz��
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
        /// �ۑ��t�@�C������Z�[�u����Ƃ��Ɏg���B�������z��
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
        /// �n�C�X�R�A�ɃX�R�A��ǉ����܂��B
        /// (�������A�����ǉ�����̂ł͂Ȃ��ۑ�����n�C�X�R�A�̐��ɓ���X�R�A���w�肳�ꂽ�ꏊ�ɕۑ����܂��B)
        /// </summary>
        /// <param name="score">�ǉ�����X�R�A</param>
        /// <returns>�ǉ����ꂽ�ʒu��Ԃ��܂��B(�������͈͊O�ȂǂŒǉ�����Ȃ������ꍇ��-1��Ԃ��܂��B)</returns>
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
    /// ���ёւ�����@���`����N���X
    /// IComparer�C���^�[�t�F�C�X����������
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
