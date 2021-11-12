using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace finex.ClearLogsFromRX.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateConstants();
    }
    
    /// <summary>
    /// Создание констант
    /// </summary>
    public static void CreateConstants()
    {
      //Создание группы констант
      EditableConstants.PublicInitializationFunctions.Module.CreateGroup("Логирование", "Группа констант связанных с логирование в системе DirectumRX");
      
      //Создание констант
      var listString = new List<string> {"C:\\inetpub\\logs"};
      EditableConstants.PublicInitializationFunctions.Module.CreateConstants("ClearLogsPaths", listString, "Путь к лог файлам DirectumRX", "Логирование");
      EditableConstants.PublicInitializationFunctions.Module.CreateConstants("LogsLifeTime", 10, "Время жизни логов (в днях)", "Логирование");
    }
    
  }
}
