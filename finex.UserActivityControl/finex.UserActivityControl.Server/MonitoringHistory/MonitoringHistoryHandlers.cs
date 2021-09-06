using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.UserActivityControl.MonitoringHistory;

namespace finex.UserActivityControl
{
	partial class MonitoringHistoryServerHandlers
	{

		public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
		{
			_obj.Name = UserActivityControl.MonitoringHistories.Resources.RecordNameFormat(_obj.DateTimeExecute, _obj.UsersActiveCount);
		}

		public override void Created(Sungero.Domain.CreatedEventArgs e)
		{
			_obj.UsersActiveCount = 0;
			_obj.DateTimeExecute = Calendar.Now;
		}
	}

}