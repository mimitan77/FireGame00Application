using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FireGame00Application
{
	/// <summary>
	/// �A�v���P�[�V�����̐ݒ�� XML �t�@�C���ɃV���A��������
	/// �ۑ����邽�߂̃N���X�ł��B
	/// </summary>
	public class ApplicationSettings
	{
		/// <summary>
		/// �ݒ�t�@�C���̃p�X�B
		/// </summary>
		public readonly String FilePath;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public ApplicationSettings() 
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
			sb.Append("\\");
			sb.Append(Application.CompanyName);
			sb.Append("\\");
			sb.Append(Application.ProductName);
			sb.Append(".config");
			FilePath = sb.ToString();
		}

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		/// <param name="path">�ݒ�t�@�C���̃p�X</param>
		public ApplicationSettings(String path) 
		{
			FilePath = path;
		}

		/// <summary>
		/// �w�肵���I�u�W�F�N�g�� XML �t�@�C���`���ɃV���A�������܂��B
		/// </summary>
		/// <param name="targetObject">�V���A��������I�u�W�F�N�g</param>
		public void SaveSettings(Object targetObject) 
		{
			if(Directory.Exists(Path.GetDirectoryName(FilePath)) == false)
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
			XmlSerializer serializer = new XmlSerializer(targetObject.GetType());
			StreamWriter writer = new StreamWriter(FilePath);
			// �����o�ϐ������ׂăV���A����
			serializer.Serialize(writer, targetObject);
			writer.Close();
		}

		/// <summary>
		/// XML �t�@�C������I�u�W�F�N�g���V���A�����������܂��B
		/// </summary>
		/// <param name="objectType">�I�u�W�F�N�g�̌^</param>
		/// <returns>�V���A�����������ꂽ�I�u�W�F�N�g</returns>
		/// <remarks>�t�@�C�������݂��Ȃ��ꍇ�� null ��Ԃ��܂��B</remarks>
		public Object LoadSetting(Type objectType) 
		{
			XmlSerializer serializer = null;
			StreamReader reader = null;
			Object setting = null;
			try 
			{
				serializer = new XmlSerializer(objectType);
				reader = new StreamReader(FilePath);
				// �V���A��������
				setting = serializer.Deserialize(reader);
			} 
			catch 
			{
				return null;
			} 
			finally 
			{
				if (reader != null) 
				{
					reader.Close();
				}
			}
			return setting;
		}
	}
}
