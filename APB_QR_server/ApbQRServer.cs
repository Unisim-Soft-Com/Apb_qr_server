using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace APB_QR_server
{
    internal class ApbQRServer
    {
        private bool isNeedToStopServer;
        private Socket listenSocket;

        public bool IsServerWork;
        public delegate void ServerStateHandler(bool state);
        public event ServerStateHandler ServerStateChange;

        public async void StartServerAsync ()
        {
            if (!IsServerWork)
                await Task.Run(StartServer);
        }


        public void StartServer()
        {

            try
            {
                isNeedToStopServer = false;
                IsServerWork = true;
                ServerStateChange?.Invoke(IsServerWork);

                ApbQRWorker apbQrWorker = new ApbQRWorker();

                int port = 8885;
                // получаем адреса для запуска сокета
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

                // создаем сокет
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                Logger.Log.Info("Сервер начинает прослушивать входящие сообщения");

                // начинаем прослушивание
                listenSocket.Listen(10);

                while (!isNeedToStopServer)
                {
                    Socket handler = listenSocket.Accept();
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    int bytes2 = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных
                    byte[] datal = new byte[sizeof(int)];

                    bool isFirst = true;
                    Logger.Log.Info("Ждем сообщение");

                    do
                    {
                        if (isFirst)
                            bytes2 = handler.Receive(datal);
                        isFirst = false;
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    } while (handler.Available > 0);


                    string req = builder.ToString();
                    Logger.Log.Info("Сообщение полученно: " 
                                    +Environment.NewLine
                                    + req 
                                    + Environment.NewLine
                                    +"Обрабатываю сообщение");

                    string responce;
                    try
                    {
                        Console.WriteLine("Полученно:" + Environment.NewLine + req + Environment.NewLine);
                        //req = req.Substring(req.IndexOf('{'));
                        responce = apbQrWorker.DoCommand(req);
                    }
                    catch (Exception e)
                    {
                        responce = "Ошибка!" + e.Message;
                        Logger.Log.Info("Ошибка обработки сообщения"
                                        +Environment.NewLine
                                        +e.Message);

                    }

                    // отправляем ответ

                    try
                    {
                        responce = Encoding.UTF8.GetBytes(responce).Length + "\n" + responce;
                        Console.WriteLine("Отправляю:" + Environment.NewLine + responce + Environment.NewLine);
                        Logger.Log.Info("Отправляю ответ"
                                        + Environment.NewLine
                                        + responce);
                        data = Encoding.UTF8.GetBytes(responce);
                        handler.Send(data);
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Info("Ошибка отправки ответа"
                                        + Environment.NewLine
                                        + e.Message);
                    }
                    finally
                    {
                        // закрываем сокет
                        //handler.Shutdown(SocketShutdown.Both);
                        //handler.Close();
                    }

                }

            }
            catch (Exception e)
            {
                Logger.Log.Info("Произошла ошибка во время прослушки/обработки/ответа"
                                + Environment.NewLine
                                + e.Message);
            }
            finally
            {
                IsServerWork = false;
                ServerStateChange?.Invoke(IsServerWork);
                Logger.Log.Info("Сервер закончил прослушивать входящие сообщения");
            }
        }

        public void StopServer()
        {
            Logger.Log.Info("Поступил запрос на остановку сервера");

            isNeedToStopServer = true;

            // закрываем сокет
            if (listenSocket != null)
            {
                try
                {
                    listenSocket.Close();
                    Logger.Log.Info("Сервер остановлен");

                }
                catch (Exception e)
                {
                    Logger.Log.Info("Ошибка во время остановки сервера"
                                    +Environment.NewLine
                                    +e.Message);

                }
            }
            
        }
    }
}
