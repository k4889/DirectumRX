using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;

namespace finex.CollectionFunctions.Shared
{
  public class ModuleFunctions
  {
    
    #region Число прописью
    
    /// <summary>
    /// Перевод дробного числа в строку
    /// </summary>
    /// <param name="sum">Число</param>
    /// <param name="isCapitalFirstLetter">Первая буква результата заглавная (по умолчанию = true)</param>
    /// <returns>Возвращает KeyValuePair (Key = целая часть, Value = дробная часть или string.Empty)</returns>
    [Public]
    public static System.Collections.Generic.KeyValuePair<string, string> NumberInWords(double sum, bool? isCapitalFirstLetter)
		{
			var sumArray = sum.ToString().Split(',');
      var firstPart = string.Empty;
			var secondPart = string.Empty;
			
			if (!sumArray.Any())
			 return new KeyValuePair<string, string>(firstPart, secondPart);
			
			if (!isCapitalFirstLetter.HasValue)
				isCapitalFirstLetter = true;
			
			int val;
			if (int.TryParse(sumArray[0], out val))
			  firstPart = sum < 0 ? string.Format("минус {0}", NumberInWords(val, isCapitalFirstLetter)).Trim() : NumberInWords(val, isCapitalFirstLetter).Trim();
			
			if (sumArray.Count() > 1 && int.TryParse(sumArray[1], out val))
				secondPart = NumberInWords(val, false).Trim();

			return new KeyValuePair<string, string>(firstPart, secondPart);
		}
    
    /// <summary>
    /// Перевод целого числа в строку
    /// </summary>
    /// <param name="sum">Число</param>
    /// <param name="isCapitalFirstLetter">Первая буква результата заглавная</param>
    /// <returns>Возвращает строковую запись числа</returns>
    [Public]
    public static string NumberInWords(int sum, bool? isCapitalFirstLetter)
    {
      bool minus = false;
      
      if (!isCapitalFirstLetter.HasValue)
        isCapitalFirstLetter = true;
      
      if (sum < 0)
      {
        sum = Math.Abs(sum);
        minus = true;
      }
      
      int n = (int)sum;
      
      var r = new StringBuilder();
      
      if (0 == n)
        r.Append("0 ");
      if (n % 1000 != 0)
        r.Append(Str(n, true, "", "", ""));
      
      n /= 1000;
      
      r.Insert(0, Str(n, false, "тысяча", "тысячи", "тысяч"));
      n /= 1000;
      
      r.Insert(0, Str(n, true, "миллион", "миллиона", "миллионов"));
      n /= 1000;
      
      r.Insert(0, Str(n, true, "миллиард", "миллиарда", "миллиардов"));
      n /= 1000;
      
      r.Insert(0, Str(n, true, "триллион", "триллиона", "триллионов"));
      n /= 1000;
      
      r.Insert(0, Str(n, true, "триллиард", "триллиарда", "триллиардов"));
      if (minus) r.Insert(0, "минус ");
      
      //Делаем первую букву заглавной
      if (isCapitalFirstLetter.Value)
        r[0] = char.ToUpper(r[0]);
      
      return r.ToString().Trim();
    }
    
    /// <summary>
    /// Перевод в строку числа с учётом падежного окончания относящегося к числу существительного
    /// </summary>
    /// <param name="sum">Число</param>
    /// <param name="male">Род существительного, которое относится к числу</param>
    /// <param name="one">Форма существительного в единственном числе</param>
    /// <param name="two">Форма существительного от двух до четырёх</param>
    /// <param name="five">Форма существительного от пяти и больше</param>
    /// <returns></returns>
    private static string Str(int sum, bool male, string one, string two, string five)
    {
      //Наименование чисел до 20
      string[] frac20 = {"", "один ", "два ", "три ", "четыре ", "пять ", "шесть ", "семь ", "восемь ", "девять ", "десять ", "одиннадцать ", "двенадцать ", "тринадцать ",
        "четырнадцать ", "пятнадцать ", "шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "};
      
      //Наименования десятков
      string[] tens = {"", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ", "шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто "};
      
      //Наименования сотен
      string[] hunds = {"", "сто ", "двести ", "триста ", "четыреста ", "пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот "};
      
      int num = sum % 1000;
      
      if(0 == num) return "";
      
      if(num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не может быть отрицательным");
      
      if(!male)
      {
        frac20[1] = "одна ";
        frac20[2] = "две ";
      }
      
      StringBuilder r = new System.Text.StringBuilder(hunds[num / 100]);
      
      if(num % 100 < 20)
      {
        r.Append(frac20[num % 100]);
      }
      else
      {
        r.Append(tens[num % 100 / 10]);
        r.Append(frac20[num % 10]);
      }
      
      r.Append(Case(num, one, two, five));
      
      if(r.Length != 0) r.Append(" ");
      return r.ToString();
    }
    
    /// <summary>
    /// Выбор правильного падежного окончания сущесвительного
    /// </summary>
    /// <param name="val">Число</param>
    /// <param name="one">Форма существительного в единственном числе</param>
    /// <param name="two">Форма существительного от двух до четырёх</param>
    /// <param name="five">Форма существительного от пяти и больше</param>
    /// <returns>Возвращает существительное с падежным окончанием, которое соответсвует числу</returns>
    private static string Case(int val, string one, string two, string five)
    {
      int t=(val % 100 > 20) ? val % 10 : val % 20;
      
      switch (t)
      {
          case 1: return one;
          case 2: case 3: case 4: return two;
          default: return five;
      }
    }
    
    #endregion
    
    
    #region Работа с датами
    
    /// <summary>
    /// Получить начало квартала
    /// </summary>
    [Public]
    public static DateTime GetBeginOfQuarter()
    {
      var today = Calendar.Today;
      int quarterCount = (today.Month + 2) % 3;
      return Calendar.BeginningOfMonth(today.AddMonths(quarterCount * -1));
    }
    
    #endregion
  
    
    #region Функции инициализации
    
    /// <summary>
    /// Получить GUID действия.
    /// </summary>
    /// <param name="action">Действие.</param>
    /// <returns>Строка, содержащая GUID.</returns>
    public static string GetActionGuid(Sungero.Domain.Shared.IActionInfo action)
    {
      var internalAction = action as Sungero.Domain.Shared.IInternalActionInfo;
      return internalAction == null ? string.Empty : internalAction.NameGuid.ToString();
    }
		
		/// <summary>
    /// Получить действие по отправке документа.
    /// </summary>
    /// <param name="action">Информация о действии.</param>
    /// <returns>Действие по отправке документа.</returns>
    public static Sungero.Docflow.IDocumentSendAction GetSendAction(Sungero.Domain.Shared.IActionInfo action)
    {
      return Sungero.Docflow.DocumentSendActions.GetAllCached(a => a.ActionGuid == Functions.Module.GetActionGuid(action)).Single();
    }
    
    #endregion
    
  }
}