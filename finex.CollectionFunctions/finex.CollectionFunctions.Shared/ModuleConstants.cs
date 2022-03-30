using System;
using Sungero.Core;

namespace finex.CollectionFunctions.Constants
{
  public static class Module
  {
    /// <summary>
    /// Количество страниц документа, на которых ищется якорь, для добавления отметки об ЭП.
    /// </summary>
    public const int SearchablePagesLimit = 5;
    
    /// <summary>
    /// Имя файла c подписью.
    /// </summary>
    [Sungero.Core.Public]
    public const string SigFileName =  "sign.sgn";
    
    /// <summary>
    /// Ограничение длины названия файла при выгрузке.
    /// </summary>
    [Sungero.Core.Public]
    public const int ExportNameLength = 50;
    
    /// <summary>
    /// Типы реципиентов.
    /// </summary>
    public static class RecipientTypes
    {
      /// <summary>
      /// GUID справочника "НОР".
      /// </summary>
      [Sungero.Core.Public]
      public const string BusinessUnitTypeGuid = "eff95720-181f-4f7d-892d-dec034c7b2ab";
      
      /// <summary>
      /// GUID справочника "Сотрудники".
      /// </summary>
      [Sungero.Core.Public]
      public const string EmployeeTypeGuid = "b7905516-2be5-4931-961c-cb38d5677565";
      
      /// <summary>
      /// GUID справочника "Подразделения".
      /// </summary>
      [Sungero.Core.Public]
      public const string DepartmentTypeGuid = "61b1c19f-26e2-49a5-b3d3-0d3618151e12";
      
      /// <summary>
      /// GUID справочника "Вид документа".
      /// </summary>
      [Sungero.Core.Public]
      public const string DocumentKindGuid = "14a59623-89a2-4ea8-b6e9-2ad4365f358c";
    }
  }
}