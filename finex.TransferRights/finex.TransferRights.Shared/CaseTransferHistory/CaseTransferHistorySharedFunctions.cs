using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.TransferRights.CaseTransferHistory;

namespace finex.TransferRights.Shared
{
  partial class CaseTransferHistoryFunctions
  {

    /// <summary>
		/// Формирование имени
		/// </summary>
		public virtual void FillName()
		{
		  _obj.Name = CaseTransferHistories.Resources.NameFormatFormat(_obj.UserFrom != null ? _obj.UserFrom.Name : string.Empty, 
		                                                               _obj.UserTo != null ? _obj.UserTo.Name : string.Empty,  
		                                                               _obj.CreateDate);
		}	
		
		/// <summary>
		/// Проверить наличие не обработанных записей 
		/// </summary>
		/// <param name="user">От кого (пользователь)</param>
		/// <returns>True - если записи есть, иначе False</returns>
		public static bool ChangeUserFromRecords(IUser user)
		{
		  if (user == null)
		    return false;
		  
		  return CaseTransferHistories.GetAll().Any(c => Equals(c.UserFrom, user) && c.Status != TransferRights.CaseTransferHistory.Status.Done);
		}
  }
}