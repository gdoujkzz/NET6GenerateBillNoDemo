using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 高并发下生成单据号.Entity
{
	public class SFC_BARCODE_SEQUENCE : OrgModelBase
	{
		#region Constructor 
		/// <summary>
		/// 实例化数据模型，将自动生成数据行 ID 号及指定行状态。
		/// </summary>
		public SFC_BARCODE_SEQUENCE()
		{
			//this.ID = GenerateNewID();
			this.STATE = "A";
		}

		/// <summary>
		/// 实例化数据模型，用户指定数据行 ID 号并设置数据行默认状态。
		/// </summary>
		/// <param name="id">数据行 ID 号。</param>
		public SFC_BARCODE_SEQUENCE(string id)
		{
			this.ID = id;
			this.STATE = "A";
		}
		#endregion
		public string BARCODE_CATEGORY { get; set; }
		public string PREFIX { get; set; }
		public int CURRENT_VALUE { get; set; }
		public string BARCODE_RULE { get; set; }
	}


}
