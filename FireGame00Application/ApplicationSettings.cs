using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FireGame00Application
{
	/// <summary>
	/// アプリケーションの設定を XML ファイルにシリアル化して
	/// 保存するためのクラスです。
	/// </summary>
	public class ApplicationSettings
	{
		/// <summary>
		/// 設定ファイルのパス。
		/// </summary>
		public readonly String FilePath;

		/// <summary>
		/// コンストラクタ
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
		/// コンストラクタ
		/// </summary>
		/// <param name="path">設定ファイルのパス</param>
		public ApplicationSettings(String path) 
		{
			FilePath = path;
		}

		/// <summary>
		/// 指定したオブジェクトを XML ファイル形式にシリアル化します。
		/// </summary>
		/// <param name="targetObject">シリアル化するオブジェクト</param>
		public void SaveSettings(Object targetObject) 
		{
			if(Directory.Exists(Path.GetDirectoryName(FilePath)) == false)
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
			XmlSerializer serializer = new XmlSerializer(targetObject.GetType());
			StreamWriter writer = new StreamWriter(FilePath);
			// メンバ変数をすべてシリアル化
			serializer.Serialize(writer, targetObject);
			writer.Close();
		}

		/// <summary>
		/// XML ファイルからオブジェクトをシリアル化解除します。
		/// </summary>
		/// <param name="objectType">オブジェクトの型</param>
		/// <returns>シリアル化解除されたオブジェクト</returns>
		/// <remarks>ファイルが存在しない場合は null を返します。</remarks>
		public Object LoadSetting(Type objectType) 
		{
			XmlSerializer serializer = null;
			StreamReader reader = null;
			Object setting = null;
			try 
			{
				serializer = new XmlSerializer(objectType);
				reader = new StreamReader(FilePath);
				// シリアル化解除
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
