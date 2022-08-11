using System;
using APB_QR_server.com.agroprombank.ws;

namespace APB_QR_server
{
    internal class ApbQROperations
    {
        private QRMCredentials qrmCredentials;
        private QRMobileService qrMobileService;


        public string TerminalId { get; set; }

        public ApbQROperations(string terminalId, string login, string password, string deviceId = "-1", string pushId = "-1", string externalToken = "")
        {
            SetQRMCredentials(login, password, deviceId, pushId, externalToken);
            TerminalId = terminalId;
            qrMobileService = new QRMobileService();
            qrMobileService.QRMCredentialsValue = qrmCredentials;
        }

        public void SetQRMCredentials(string login, string password, string deviceId = "-1", string pushId = "-1", string externalToken = "")
        {
            qrmCredentials = new QRMCredentials();
            qrmCredentials.Login = login;
            qrmCredentials.Password = password;
            qrmCredentials.DeviceId = deviceId;
            qrmCredentials.PushId = pushId;
            qrmCredentials.ExternalToken = externalToken;
        }




        public (bool IsSuccessCreatePayment, string Message, bool IsSuccessOperation) TryCreatePayment(decimal amount, string currencycode)
        {
            try
            {
                return (qrMobileService.CreatePayment(TerminalId, amount, currencycode), "Success", true);
            }
            catch (Exception e)
            {
                return (false, e.Message, false);
            }
        }

        public (int PaymentId, string Message, bool IsSuccessOperation) TryCreatePaymentId(decimal amount, string currencycode)
        {
            try
            {
                return (Convert.ToInt32(qrMobileService.CreatePaymentID(TerminalId, amount, currencycode, "")), "Success", true);
            }
            catch (Exception e)
            {
                return (-1, e.Message, false);
            }
        }

        public (string Message, bool IsSuccessOperation) TryClosePayment()
        {
            try
            {
                qrMobileService.ClosePayment(TerminalId);
                return ("Success", true);
            }
            catch (Exception e)
            {
                return (e.Message, false);
            }
        }


        public (string PaymentStateXml, string Message, bool IsSuccessOperation) TryGetPaymentState(decimal id)
        {
            try
            {
                return (qrMobileService.GetPaymentState(TerminalId, id), "Success", true);
            }
            catch (Exception e)
            {
                return ("", e.Message, false);
            }
        }


        public (string Message, bool IsSuccessOperation) TryReversePayment(decimal id, string rrn)
        {
            try
            {
                qrMobileService.ReversePayment(TerminalId, id, rrn);
                return ("Success", true);
            }
            catch (Exception e)
            {
                return (e.Message, false);
            }
        }

        public (string PaymentsXml, string Message, bool IsSuccessOperation) TryGetPayments(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            try
            {
                return (qrMobileService.GetPayments(dateTimeFrom, dateTimeTo, TerminalId), "Success", true);
            }
            catch (Exception e)
            {
                return ("", e.Message, false);
            }
        }


        //private void Test()
        //{
        //    qrMobileService.
        //}
    }
}
