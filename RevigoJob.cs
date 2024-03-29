﻿using IRB.Revigo.Core.Worker;
using System;

namespace IRB.RevigoWeb
{
	/// <summary>
	/// 
	/// Authors:
	/// 	Fran Supek (https://github.com/FranSupek)
	/// 	Rajko Horvat (https://github.com/rajko-horvat)
	/// 
	/// License:
	/// 	MIT
	/// 	Copyright (c) 2011-2023, Ruđer Bošković Institute
	///		
	/// 	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
	/// 	and associated documentation files (the "Software"), to deal in the Software without restriction, 
	/// 	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
	/// 	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
	/// 	subject to the following conditions: 
	/// 	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
	/// 	The names of authors and contributors may not be used to endorse or promote Software products derived from this software 
	/// 	without specific prior written permission.
	/// 	
	/// 	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
	/// 	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
	/// 	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
	/// 	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
	/// 	DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
	/// 	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	/// </summary>
	public class RevigoJob
	{
		private int iID = -1;
		private DateTime dtExpiration;
		private RevigoWorker oWorker;

		public RevigoJob(int id, DateTime expiration, RevigoWorker worker)
		{
			this.iID = id;
			this.dtExpiration = expiration;
			this.oWorker = worker;
		}

		public int ID
		{
			get
			{
				return this.iID;
			}
		}

		public DateTime Expiration
		{
			get
			{
				return this.dtExpiration;
			}
		}

		public RevigoWorker Worker
		{
			get
			{
				return this.oWorker;
			}
		}

		public bool IsExpired { get { return (DateTime.Now - this.dtExpiration).TotalMinutes > 0; } }

		public void ExtendExpiration()
		{
			DateTime newExpiration;

			if (this.oWorker.IsRunning)
			{
				newExpiration = DateTime.Now.Add(Global.JobTimeout);
			}
			else if (this.oWorker.RequestSource == RequestSourceEnum.WebPage)
			{
				newExpiration = DateTime.Now.Add(Global.SessionTimeout);
			}
			else
			{
				newExpiration = DateTime.Now.Add(Global.JobSessionTimeout);
			}

			if (this.dtExpiration < newExpiration)
				this.dtExpiration = newExpiration;
		}
	}
}
