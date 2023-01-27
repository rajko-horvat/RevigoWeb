using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using System.IO;
using System.Threading;
using System.Web.Configuration;
using System.Configuration.Provider;
using RevigoWeb;

/// <summary>
/// Summary description for DBConnection
/// </summary>
public class DBConnection : IDisposable
{
	private bool bDisposed = false;
	private string sConnection = null;
	private MySqlConnection oConnection = null;
	private string sError = null;
	private bool bReconnect = false;
	private DateTime dtReconnect = DateTime.Now;
	private TimeSpan tsReconnectDelay = new TimeSpan(0, 0, 30);

	public DBConnection(string connection)
		: this(connection, new TimeSpan(0, 0, 30))
	{ }

	public DBConnection(string connection, int reconnectDelaySeconds)
		: this(connection, new TimeSpan(0, 0, reconnectDelaySeconds))
	{ }

	public DBConnection(string connection, TimeSpan reconnectDelay)
	{
		this.tsReconnectDelay = reconnectDelay;

		try
		{
			if (!string.IsNullOrEmpty(connection))
			{
				this.sConnection = connection;
				this.oConnection = new MySqlConnection(this.sConnection);
			}
			else
			{
				this.sConnection = null;
				Global.WriteToSystemLog(this.GetType().FullName, "Connection string is invalid");
				this.sError = "Connection string is invalid";
			}
		}
		catch (Exception ex)
		{
			this.sError = ex.Message;
			this.oConnection = null;

			// log this error to local log file
			Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
		}

		if (this.oConnection != null)
		{
			try
			{
				this.oConnection.Open();
			}
			catch (Exception ex)
			{
				this.sError = ex.Message;
				this.oConnection = null;
				this.bReconnect = true;
				this.dtReconnect = DateTime.Now.Add(this.tsReconnectDelay);

				// log this error to local log file
				Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
			}
		}
	}

	~DBConnection()
	{
		Dispose(false);
	}

	#region IDisposable Members

	public bool Disposes
	{
		get
		{
			return this.bDisposed;
		}
	}

	protected void Dispose(bool disposing)
	{
		if (!bDisposed)
		{
			bDisposed = true;
			if (disposing)
			{
				// release managed objects
				if (this.oConnection != null)
				{
					try
					{
						this.oConnection.Close();
					}
					catch { }
				}

				GC.SuppressFinalize(this);
			}
			this.oConnection = null;
		}
	}

	public void Dispose()
	{
		if (!bDisposed)
		{
			this.Dispose(true);
		}
	}

	#endregion

	public void Open()
	{
		if (this.oConnection == null && !this.IsConnected && !string.IsNullOrEmpty(this.sConnection) &&
			this.bReconnect && (DateTime.Now - this.dtReconnect).TotalMinutes > 0.0)
		{
			try
			{
				this.bReconnect = false;
				this.oConnection = new MySqlConnection(this.sConnection);
				this.oConnection.Open();
				this.sError = "";
			}
			catch (Exception ex)
			{
				this.sError = ex.Message;
				this.oConnection = null;
				this.bReconnect = true;
				this.dtReconnect = DateTime.Now.Add(this.tsReconnectDelay);

				// log this error to local log file
				Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
			}
		}
	}

	public void Close()
	{
		if (this.oConnection != null)
		{
			try
			{
				this.oConnection.Close();
				this.oConnection.Dispose();
			}
			catch
			{ }
			this.oConnection = null;
			this.sError = "";
			this.bReconnect = true;
			this.dtReconnect = DateTime.Now;
		}
	}

	public MySqlConnection Connection
	{
		get
		{
			return this.oConnection;
		}
	}

	public bool IsConnected
	{
		get
		{
			if (this.oConnection != null)
			{
				switch (this.oConnection.State)
				{
					case ConnectionState.Open:
					case ConnectionState.Executing:
					case ConnectionState.Fetching:
					case ConnectionState.Connecting:
						return true;
					default:
						return false;
				}
			}

			return false;
		}
	}

	public string ErrorMessage
	{
		get
		{
			return this.sError;
		}
	}

	public static Boolean ToBoolean(object value)
	{
		bool bValue = false;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				bValue = Convert.ToBoolean(value);
			}
			catch
			{ }
		}

		return bValue;
	}

	public static uint ToUInt32(object value)
	{
		uint uiValue = 0;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				uiValue = Convert.ToUInt32(value);
			}
			catch
			{ }
		}

		return uiValue;
	}

	public static int ToInt32(object value)
	{
		int iValue = 0;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				iValue = Convert.ToInt32(value);
			}
			catch
			{ }
		}

		return iValue;
	}

	public static long ToInt64(object value)
	{
		long lValue = 0;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				lValue = Convert.ToInt64(value);
			}
			catch
			{ }
		}

		return lValue;
	}

	public static double ToDouble(object value)
	{
		double dValue = 0.0;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				dValue = Convert.ToDouble(value);
			}
			catch
			{ }
		}

		return dValue;
	}

	public static string ToString(object value)
	{
		string sValue = null;

		if (value != null && value != DBNull.Value)
		{
			try
			{
				sValue = Convert.ToString(value);
			}
			catch
			{ }
		}

		return sValue;
	}
}
