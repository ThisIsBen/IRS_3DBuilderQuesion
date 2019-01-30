using System;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace suro.util
{
	/// <summary>
	/// SqlDB ªººK­n´y­z¡C
	/// </summary>
	public class SqlDB
	{
		private string m_connstr;

		public SqlDB(string connstr)
		{
			m_connstr = connstr;
		}
		public string ConnectionString
		{
			get { return this.m_connstr; }
			set { this.m_connstr = value; }
		}


		/// <summary>
		/// Extract table name from select query.
		/// If select query is "SELECT * FROM table1 WHERE ...",
		/// "table1" will be returned.
		/// 
		/// This function applys only to select query where only one table is selected.
		/// Select query such as "SELECT * FROM table1, table2 WHERE ..."
		/// "table1," will be returned which is not a desirable result.
		/// </summary>
		/// <param name="selectstr"></param>
		/// <returns></returns>
		private string getTableName(string selectstr)
		{
			Regex r = new Regex("\\s+FROM\\s+(\\S+)", RegexOptions.IgnoreCase);
			Match m = r.Match(selectstr);
			return m.Groups[1].Value;
		}

		/// <summary>
		/// Get a DataSet for the select query string.
		/// (ex: "SELECT * FROM table1 WHERE ...")
		/// The returned DataSet will contain one table only which has no table name.
		/// </summary>
		/// <param name="selectstr"></param>
		/// <returns></returns>
		public DataSet getDataSet(string selectstr)
		{
			SqlDataAdapter da = new SqlDataAdapter(selectstr, m_connstr);
			DataSet ds = new DataSet();
			try
			{
				da.Fill(ds);
				return ds;
			}
			finally
			{
				da.Dispose();
			}
		}

		/// <summary>
		/// Get a DataSet for the select command.
		/// Select query string and parameters of the command have to be
		/// properly configured before invoking this function.
		/// 
		/// However, the connection of the command will be taken care of
		/// inside this function.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public DataSet getDataSet(SqlCommand cmd)
		{
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			cmd.Connection = new SqlConnection(m_connstr);
			DataSet ds = new DataSet();

			try
			{
				//cmd.Connection.Open();
				da.Fill(ds);
				return ds;
			}
			finally
			{
				//cmd.Connection.Close();
				cmd.Connection.Dispose();
				da.Dispose();
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="selectstr"></param>
		/// <param name="pList"></param>
		/// <returns></returns>
		public DataSet getDataSet(string selectstr, object[] pList)
		{
			SqlCommand cmd = new SqlCommand(selectstr);
			this.fillSqlParameters(cmd, pList);
			return this.getDataSet(cmd);
		}
		public void ExecuteNonQuery(string cmdstr)
		{
			SqlCommand cmd = new SqlCommand(cmdstr);
			this.ExecuteNonQuery(cmd);
		}
		public void ExecuteNonQuery(SqlCommand cmd)
		{
			try
			{
				cmd.Connection = new SqlConnection(m_connstr);
				cmd.Connection.Open();
				cmd.ExecuteNonQuery();
			}
			finally
			{
				cmd.Connection.Close();
				cmd.Connection.Dispose();
			}
		}
		public void ExecuteNonQuery(string cmdstr, object[] pList)
		{
			SqlCommand cmd = new SqlCommand(cmdstr);
			this.fillSqlParameters(cmd, pList);
			this.ExecuteNonQuery(cmd);
		}
		public object ExecuteScalar(string selectstr)
		{
			SqlCommand cmd = new SqlCommand(selectstr);
			return this.ExecuteScalar(cmd);
		}
		public object ExecuteScalar(SqlCommand cmd)
		{
			try
			{
				cmd.Connection = new SqlConnection(m_connstr);
				cmd.Connection.Open();
				return cmd.ExecuteScalar();
			}
			finally
			{
				cmd.Connection.Close();
				cmd.Connection.Dispose();
			}
		}
		public object ExecuteScalar(string selectstr, object[] pList)
		{
			SqlCommand cmd = new SqlCommand(selectstr);
			this.fillSqlParameters(cmd, pList);
			return this.ExecuteScalar(cmd);
		}

		/// <summary>
		/// Update database from an offline DataTable.
		/// Notice that, the DataTable and selectstr used here must
		/// be consistent with those you used in invoking getDataSet().
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="selectstr"></param>
		public bool Update(DataTable dt, string selectstr)
		{
			SqlDataAdapter da = new SqlDataAdapter(selectstr, m_connstr);            
			SqlCommandBuilder cb = new SqlCommandBuilder(da);

           
            try
            {

                da.Update(dt);
                return true;

            }
            catch { return false; }
			finally
			{
				da.Dispose();
				cb.Dispose();
			}
		}


       

		public void Update(DataTable dt, SqlCommand selectcmd)
		{
			SqlDataAdapter da = new SqlDataAdapter(selectcmd);
			SqlCommandBuilder cb = new SqlCommandBuilder(da);
			try
			{
				selectcmd.Connection = new SqlConnection(m_connstr);
				selectcmd.Connection.Open();
				da.Update(dt);
			}
			finally
			{
				selectcmd.Connection.Close();
				selectcmd.Connection.Dispose();
				da.Dispose();
				cb.Dispose();
			}
		}
		public void fillSqlParameters(SqlCommand cmd, object[] pList)
		{
			Regex r = new Regex("(@\\w+)", RegexOptions.IgnoreCase);
			MatchCollection matches = r.Matches(cmd.CommandText);
			for(int i=0; i<matches.Count; i++)
			{
				Match m = matches[i];
				cmd.Parameters.Add(m.Value, pList[i]);
			}
		}
	}
}
