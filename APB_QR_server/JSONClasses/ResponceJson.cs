using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace APB_QR_server.JSONClasses
{
    internal class ResponceJson
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public ResponceData ResponceData { get; set; }
        public ObservableCollection<ResponceData> ResponceDataList { get; set; }
        public string XmlList { get; set; }

    }

    internal class ResponceData
    {
        private int status;
        public int Id { get; set; }
        public string RRN { get; set; }
        public string XmlResponce { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public int Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                try
                {
                    StatusText = StateIdText[value];
                }
                catch (Exception e)
                {
                    
                }
            }
        }
        public string StatusText { get; private set; }

        public string DataCreate { get; set; }

        private Dictionary<int, string> StateIdText;

        public ResponceData()
        {
            StateIdText = new Dictionary<int, string>()
            {
                {1, "Введен"},
                {2, "Оплачен"},
                {3, "Завершён"},
                {4, "Отменён"},
                {5, "Частичный реверс"},
                {6, "Завершён без оплаты"}

            };
        }
    }
}
