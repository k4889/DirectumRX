using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace finex.TransferRights.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      InitializationLogger.Debug("Init: Grant rights on Case transfer history to all users.");
      
      CaseTransferHistories.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      CaseTransferHistories.AccessRights.Save();
    }
  }


}
