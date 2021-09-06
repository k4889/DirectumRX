using System;
using Sungero.Core;

namespace finex.TransferRights.Constants
{
  public static class Module
  {
    /// <summary>
    /// Разделитель
    /// </summary>
    public const string Sepataror =  "|";
    
    /// <summary>
    /// Операция "Передача прав"
    /// </summary>
    public const string StatusRights =  "TransferRigths";
    
    public static class Status
    {
      /// <summary>
      /// Статус "Все"
      /// </summary>
      public const string StatusAll =  "All";
      
      /// <summary>
      /// Статус "В работе"
      /// </summary>
      public const string StatusInWork =  "InWork";
      
      /// <summary>
      /// Статус "Не передавать"
      /// </summary>
      public const string StatusNotTransfer =  "NotTransfer";
    }
  }
}