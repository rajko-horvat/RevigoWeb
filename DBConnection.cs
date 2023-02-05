#if STATISTICS
using System;
using System.Data;
using MySqlConnector;
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
				//Global.WriteToSystemLog(this.GetType().FullName, "Connection string is invalid");
				this.sError = "Connection string is invalid";
			}
		}
		catch (Exception ex)
		{
			this.sError = ex.Message;
			this.oConnection = null;

			// log this error to local log file
			//Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
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
				//Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
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
				this.sError = null;
			}
			catch (Exception ex)
			{
				this.sError = ex.Message;
				this.oConnection = null;
				this.bReconnect = true;
				this.dtReconnect = DateTime.Now.Add(this.tsReconnectDelay);

				// log this error to local log file
				//Global.WriteToSystemLog(this.GetType().FullName, ex.Message);
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
			this.sError = null;
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
}
#endif