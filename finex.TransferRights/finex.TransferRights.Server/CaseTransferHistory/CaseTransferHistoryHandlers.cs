using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.TransferRights.CaseTransferHistory;

namespace finex.TransferRights
{
  partial class CaseTransferHistoryFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (finex.CollectionFunctions.PublicFunctions.Module.Remote.IsAdministrator())
        return query;
      
      var current = Users.Current;
      return query.Where(c => c.UserFrom.Equals(current) || c.UserTo.Equals(current));
    }
  }

  partial class CaseTransferHistoryServerHandlers
  {

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
      e.AddError("Удаление записи запрещено !");
      return;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.CaseTransferHistory.FillName(_obj);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = CaseTransferHistory.Status.Active;
      _obj.CreateDate = Calendar.Now;
    }
  }

}