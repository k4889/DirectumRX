using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.TransferRights.CaseTransferHistory;

namespace finex.TransferRights.Client
{
  partial class CaseTransferHistoryActions
  {
    public virtual void SendForTransfer(Sungero.Domain.Client.ExecuteActionArgs e)
    {            
      Functions.CaseTransferHistory.Remote.StartTransferRightsHandler(_obj);
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanSendForTransfer(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == CaseTransferHistory.Status.Error;
    }
  }

}