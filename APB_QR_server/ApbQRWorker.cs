using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using APB_QR_server.JSONClasses;

namespace APB_QR_server
{
    internal class ApbQRWorker
    {
        public string DoCommand(string commandJson)
        {
            ResponceJson responceJson = new ResponceJson() { Code = -1, Message = "Общая ошибка выполнения операции" };
            RequestJson requestJson = new RequestJson();
            try
            {
                Logger.Log.Info("Десериализую полученное сообщение из формата JSON");
                requestJson = JsonConvert.DeserializeObject<RequestJson>(commandJson);
            }
            catch (Exception e)
            {
                responceJson = new ResponceJson()
                {
                    Code = -1,
                    Message = "Не удалось десериализовать Json." + Environment.NewLine +
                                         e.Message
                };
                Logger.Log.Info("Ошибка десериализации."
                +Environment.NewLine
                +e.Message
                +Environment.NewLine
                +"Возвращаю ответ с ошибкой");

                return JsonConvert.SerializeObject(responceJson);
            }

            if (requestJson == null)
            {
                responceJson = new ResponceJson() { Code = -1, Message = "Не удалось десериализовать Json" };
                Logger.Log.Info("Ошибка десериализации. Объект пустой."
                                + Environment.NewLine
                                + "Возвращаю ответ с ошибкой");
                return JsonConvert.SerializeObject(responceJson);
            }

            responceJson = new ResponceJson() { Code = -1, Message = "Не удалось десериализовать Json" };
            Logger.Log.Info("Выбираю метод для операции "+ requestJson.OpetaionType);
            switch (requestJson.OpetaionType)
            {
                case 1:
                    {
                        responceJson = CreatePayment(requestJson);
                        break;
                    }
                case 2:
                    {
                        responceJson = CreatePaymentID(requestJson);
                        break;
                    }
                case 3:
                    {
                        responceJson = ClosePayment(requestJson);
                        break;
                    }
                case 4:
                    {
                        responceJson = GetPaymentState(requestJson);
                        break;
                    }
                case 5:
                    {
                        responceJson = ReversePayment(requestJson);
                        break;
                    }
                case 6:
                    {
                        responceJson = GetPayments(requestJson);
                        break;
                    }
                case 7:
                    {
                        responceJson = CreatePaymentIDAndWaitStatus(requestJson);
                        break;
                    }
                case -1:
                    {
                        responceJson = CloseServer(requestJson);
                        break;
                    }

                default:
                    {
                        responceJson = new ResponceJson()
                        {
                            Code = -1,
                            Message = "Указан не верный код операции!"
                        };
                        break;
                    }
            }

            Logger.Log.Info("Сериализую ответ в JSON ");

            return JsonConvert.SerializeObject(responceJson);
        }



        private ResponceJson CreatePayment(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();

            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };
            if (requestJson.RequestData == null)
                return new ResponceJson() { Code = -3, Message = "Аргументы запроса были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request = apbQrOperations.TryCreatePayment(requestJson.RequestData.Amount, requestJson.RequestData.CurrencyCode);
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;
                responceJson.ResponceData = new ResponceData()
                {
                    Amount = requestJson.RequestData.Amount,
                    CurrencyCode = requestJson.RequestData.CurrencyCode
                };
            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка создания нового платежа." + Environment.NewLine + request.Item2;
            }

            return responceJson;
        }


        /// <summary>
        ////Создает новый платеж с ID и проверяет его статус в течении 3х минут пока не оплатят или не отменят платеж
        /// </summary>
        /// <param name="requestJson"></param>
        /// <returns></returns>
        private ResponceJson CreatePaymentIDAndWaitStatus(RequestJson requestJson)
        {
            bool isStopCheckStatus = false;
            bool isStopCheckStatusByUser = false;

            // создаю новый платёж
            Logger.Log.Info("Создаю новый платеж");
            ResponceJson responceJson = new ResponceJson();
            ResponceJson responceJson_CreatePaymentID = CreatePaymentID(requestJson);

            // если создать платеж не удалось
            if (responceJson_CreatePaymentID.Code != 0)
            {
                Logger.Log.Info("Создать новый платеж не удалось: "+ responceJson_CreatePaymentID.Message);
                return responceJson_CreatePaymentID;

            }

            //RequestJson для получения статуса платежа
            RequestJson requestJsonState = new RequestJson()
            {
                AuthenticationData = requestJson.AuthenticationData,
                OpetaionType = 4,
                RequestData = new RequestData()
                {
                    Id = responceJson_CreatePaymentID.ResponceData.Id
                }
            };

            //Форма для пользовательской отмены платежа
            FormClosePayment formClosePayment = new FormClosePayment(
                "Нажмите \"Отменить платёж\" для отмены ожидания платежа на сумму "
                + requestJson.RequestData.Amount/100+" руб."
                , "Отменить ожидание платежа?");

            EventHandler buttonClickEventHandler = null;


            // отсдельный поток для формы
            Task.Run(() =>
            {
                buttonClickEventHandler = delegate (object sender, EventArgs args)
                {
                    Logger.Log.Info("Был вызван метод нажатия кнопки отмены платежа");

                    isStopCheckStatusByUser = true;
                    formClosePayment.buttonCancelPayment.Click -= buttonClickEventHandler;
                };
                formClosePayment.buttonCancelPayment.Click += buttonClickEventHandler;
                Logger.Log.Info("Открываю форму для отмены платежа");

                formClosePayment.ShowDialog();
            });

            Logger.Log.Info("Запускаю цикл проверки статуса платежа");

            //цикл для проверки статуса платежа раз в N секунд
            ResponceJson responceJsonState = new ResponceJson();
            do
            {
                Logger.Log.Info("Получаю статус платежа");

                responceJsonState = GetPaymentState(requestJsonState);
                responceJsonState.ResponceDataList = null;
                responceJsonState.XmlList = null;

                //если не получилось получить статус платежа
                if (responceJsonState.Code != 0)
                {
                    //isStopCheckStatus = true;
                    formClosePayment.SafeInvoke(formClosePayment,
                        new Action(() =>
                        {
                            formClosePayment.labelStatusState.Text = "Получить статус не удалось";
                        }));

                    Logger.Log.Info("Получить статус не удалось");
                }
                else
                {
                    Logger.Log.Info("Статус:" + responceJsonState.ResponceData.StatusText);
                    formClosePayment.SafeInvoke(formClosePayment,
                        new Action(() =>
                        {
                            formClosePayment.labelStatusState.Text = responceJsonState.ResponceData.StatusText;
                        }));
                    //если статус изменился с "Введен"
                    if (responceJsonState.ResponceData.Status != 1)
                    {
                        Logger.Log.Info("Статус изменился. Закрываю форму отмены платежа");
                        isStopCheckStatus = true;

                        formClosePayment.SafeInvoke(formClosePayment.buttonCancelPayment,
                            new Action(() => { formClosePayment.buttonCancelPayment.PerformClick(); }));
                    }
                }
                Thread.Sleep(1500);

            } while (!(isStopCheckStatus || isStopCheckStatusByUser));

            if (isStopCheckStatusByUser)
            {
                Logger.Log.Info("Цикл проверки статуса платежа завершен по причине \"Оплата отменена кассиром, либо таймаут операции\"");
            }
            if (isStopCheckStatus)
            {
                Logger.Log.Info("Цикл проверки статуса платежа завершен по причине \"Статус платежа изменился\"");
            }

            Logger.Log.Info("Делаю проверочный запрос статуса платежа");
            responceJsonState = GetPaymentState(requestJsonState);
            responceJsonState.ResponceDataList = null;
            responceJsonState.XmlList = null;

            //если получить статус платежа не удалось за выделенное время
            if (responceJsonState.Code != 0)
            {
                Logger.Log.Info("Создать платеж получилось успешно, но провека статуса платежа была не удачной!");

                responceJson = responceJson_CreatePaymentID;
                responceJson.Code = -6;
                responceJson.Message =
                    "Создать платеж получилось успешно, но проверка статуса платежа была не удачной!"
                    + Environment.NewLine
                    + responceJsonState.Message;
            }
            else
            {
                //если оплата прошла успешно
                if (responceJsonState.ResponceData.Status == 2)
                {
                    Logger.Log.Info("Оплата прошла успешно!");
                    responceJson = responceJsonState;
                }

                //если статус платежа остался "Введен"
                if (responceJsonState.ResponceData.Status == 1)
                {
                    Logger.Log.Info("Статус платежа не имзенился со статуса Введен. Пробую сделать отмену платежа");
                    responceJson = responceJsonState;

                    RequestJson requestJsonClose = new RequestJson()
                    {
                        AuthenticationData = requestJson.AuthenticationData
                    };
                    ResponceJson responceJsonClose = ClosePayment(requestJsonClose);

                    if (responceJsonClose.Code != 0)
                    {
                        Logger.Log.Info("Отменить платеж не удалось");
                        responceJson.Code = -7;
                        responceJson.Message = "Оплата не была произведена. Отменить платеж не удалось!"
                                               + Environment.NewLine
                                               + responceJsonClose.Message;
                    }
                    else
                    {
                        Logger.Log.Info("Платеж отменен");

                        responceJson = GetPaymentState(requestJsonState);
                        responceJson.ResponceDataList = null;
                        responceJson.XmlList = null;

                        responceJson.Code = -8;
                        responceJson.Message = "Оплата не была произведена. Платеж отменен успешно!";
                    }

                }
            }

            


            

            return responceJson;
        }



        private ResponceJson CreatePaymentID(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();
            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };
            if (requestJson.RequestData == null)
                return new ResponceJson() { Code = -3, Message = "Аргументы запроса были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request = apbQrOperations.TryCreatePaymentId(requestJson.RequestData.Amount, requestJson.RequestData.CurrencyCode);
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;
                responceJson.ResponceData = new ResponceData()
                {
                    Id = request.PaymentId,
                    Amount = requestJson.RequestData.Amount,
                    CurrencyCode = requestJson.RequestData.CurrencyCode
                };
            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка создания нового платежа." + Environment.NewLine + request.Item2;
            }

            return responceJson;
        }


        private ResponceJson GetPaymentState(RequestJson requestJson)
        {
            requestJson.RequestData.DateTimeFrom = DateTime.Now;
            requestJson.RequestData.DateTimeTo = DateTime.Now;
            ResponceJson responceJson = GetPayments(requestJson);


            return responceJson;
        }

        private ResponceJson GetPayments(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();
            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };
            if (requestJson.RequestData == null)
                return new ResponceJson() { Code = -3, Message = "Аргументы запроса были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request = apbQrOperations.TryGetPayments(
                requestJson.RequestData.DateTimeFrom, requestJson.RequestData.DateTimeTo);
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;
                responceJson.ResponceDataList = new ObservableCollection<ResponceData>();
                responceJson.XmlList = request.PaymentsXml;
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(request.PaymentsXml);
                    XmlNodeList xmlNodePayments = xmlDocument.GetElementsByTagName("Payment");
                    if (xmlNodePayments.Count > 0)
                    {
                        foreach (XmlNode xmlNodePayment in xmlNodePayments)
                        {
                            ResponceData responceData = new ResponceData()
                            {
                                XmlResponce = xmlNodePayment.InnerXml,
                                RRN = xmlNodePayment["trxreferencenumber"].InnerText,
                                Id = Convert.ToInt32(xmlNodePayment["qrpaymentid"].InnerText),
                                Status = Convert.ToInt32(xmlNodePayment["state"].InnerText),
                                Amount = Convert.ToDecimal(xmlNodePayment["amount"].InnerText.Replace(',', '.')),
                                CurrencyCode = xmlNodePayment["currencycode"].InnerText,
                                DataCreate = xmlNodePayment["createdate"].InnerText
                            };

                            responceJson.ResponceDataList.Add(responceData);

                            if (responceData.Id == requestJson.RequestData.Id)
                            {
                                responceJson.ResponceData = new ResponceData()
                                {
                                    XmlResponce = xmlNodePayment.InnerXml,
                                    RRN = responceData.RRN,
                                    Id = responceData.Id,
                                    Status = responceData.Status,
                                    Amount = responceData.Amount,
                                    CurrencyCode = responceData.CurrencyCode,
                                    DataCreate = responceData.DataCreate
                                };
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    responceJson.Code = -5;
                    responceJson.Message = "Ошибка парсинга XML." + Environment.NewLine + e.Message;
                }

            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка получения информации о платеже/платежах." + Environment.NewLine + request.Message;
            }

            return responceJson;
        }

        private ResponceJson GetPaymentState_old(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();
            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };
            if (requestJson.RequestData == null)
                return new ResponceJson() { Code = -3, Message = "Аргументы запроса были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request = apbQrOperations.TryGetPaymentState(requestJson.RequestData.Id);
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;

                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(request.PaymentStateXml);
                    XmlNodeList xmlNodePayments = xmlDocument.GetElementsByTagName("root");

                    responceJson.ResponceData = new ResponceData()
                    {
                        XmlResponce = request.PaymentStateXml,
                        RRN = xmlNodePayments[0]["trxreferencenumber"].InnerText,
                        Id = Convert.ToInt32(xmlNodePayments[0]["qrpaymentid"].InnerText),
                        Status = Convert.ToInt32(xmlNodePayments[0]["state"].InnerText),
                        CurrencyCode = xmlNodePayments[0]["currencycode"].InnerText
                    };
                }
                catch (Exception e)
                {
                    responceJson.Code = -5;
                    responceJson.Message = "Ошибка парсинга XML." + Environment.NewLine + e.Message;
                }

            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка получения статуса платежа." + Environment.NewLine + request.Message;
            }

            return responceJson;
        }

        private ResponceJson ClosePayment(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();
            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request = apbQrOperations.TryClosePayment();
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;
            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка отмены платежа." + Environment.NewLine + request.Message;
            }

            return responceJson;
        }

        private ResponceJson ReversePayment(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson();
            if (requestJson.AuthenticationData == null)
                return new ResponceJson() { Code = -2, Message = "Данные авторизации были null" };
            if (requestJson.RequestData == null)
                return new ResponceJson() { Code = -3, Message = "Аргументы запроса были null" };

            AuthenticationData authenticationData = requestJson.AuthenticationData;

            ApbQROperations apbQrOperations =
                new ApbQROperations(
                    authenticationData.TerminalId,
                    authenticationData.Login,
                    authenticationData.Password,
                    authenticationData.TerminalId,
                    authenticationData.ExternalToken);

            var request =
                apbQrOperations.TryReversePayment(requestJson.RequestData.Id, requestJson.RequestData.RRN);
            if (request.IsSuccessOperation == true)
            {
                responceJson.Code = 0;
                responceJson.Message = request.Message;
            }
            else
            {
                responceJson.Code = -4;
                responceJson.Message = "Ошибка возврата денежных средств." + Environment.NewLine + request.Message;
            }

            return responceJson;
        }

        private ResponceJson CloseServer(RequestJson requestJson)
        {
            ResponceJson responceJson = new ResponceJson()
            {
                Message = "Выключаю сервис, через 2 секунды",
                Code = 0
            };
            CloseServerAsync();

            return responceJson;
        }


        private async Task CloseServerAsync()
        {
            await Task.Delay(2000).ContinueWith(t =>
            {
                System.Environment.Exit(1);
            });
        }
    }
}
