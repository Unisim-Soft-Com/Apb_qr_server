using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APB_QR_server.JSONClasses
{
    internal class RequestJson
    {
        public AuthenticationData AuthenticationData { get; set; }
        public int OpetaionType { get; set; }
        public RequestData RequestData { get; set; }
    }

    internal class RequestData
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public int Id { get; set; }
        public string RRN { get; set; }
        public DateTime DateTimeFrom { get; set; }
        public DateTime DateTimeTo { get; set; }

    }


    internal class AuthenticationData
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string TerminalId { get; set; }
        public string ExternalToken { get; set; }
        
    }

    internal interface IRequestData
    {
    }
    internal class OpetaionTypeDictionary
    {
        OpetaionTypeDictionary()
        {
            Dictionary<int, string> OpetaionTypeEnum = new Dictionary<int, string>()
            {
                {-1, "GetPayments"}, //Закрыть сервис
                {1, "CreatePayment"}, //Создает новый платёж
                {2, "CreatePaymentId"}, //Создает новый платеж и возвращает id платежа
                {3, "ClosePayment"}, //Закрывает текущий неоплаченный платеж
                {4, "GetPaymentState"}, //Получить состояние платежа
                {5, "ReversePayment"}, //Отменяет оплаченный платеж и возвращает деньги
                {6, "GetPayments"}, //Получить список всех платежей
                {7, "GetPaymentsAndWaitStatus"}, //Получить список всех платежей
            };

        }
    }

    
}
