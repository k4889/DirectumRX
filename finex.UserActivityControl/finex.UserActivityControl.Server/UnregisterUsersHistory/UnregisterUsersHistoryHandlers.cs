using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.UserActivityControl.UnregisterUsersHistory;

namespace finex.UserActivityControl
{
	partial class UnregisterUsersHistoryServerHandlers
	{

		public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
		{
			_obj.Name = string.Format("Отключение пользователя {0} ({1})", _obj.UserID, _obj.DateTimeUnregister);
		}
	}

}