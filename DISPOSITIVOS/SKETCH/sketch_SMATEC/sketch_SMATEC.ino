#include <SPI.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <Ethernet.h>

#include <avr/wdt.h>//para resetear el arduino

//ntp
#include <EthernetUdp.h>
unsigned int localUdpPort = 80;      //  Puerto local para escuchar UDP
IPAddress timeServer(xx, xx, xx, xx);  // hora.roa.es NTP server
const int NTP_PACKET_SIZE = 48;        // La hora son los primeros 48 bytes del mensaje
byte packetBuffer[ NTP_PACKET_SIZE];      // buffer para los paquetes

EthernetUDP Udp;                // Una instancia de UDP para enviar y recibir mensajes
String CurrentTime = "";//Time UTC
int CurrentHour = 0;//Hora UTC
int CurrentMinute = 0;//Minuto UTC
int LastHour = 0;//la hora que tiene los estadísticos
//fin ntp

#define puertoEth 10
#include <SD.h>
#define puertoSD 4

#include <CS_MQ7.h>
#define MQ7TOG 3
#define MQ7DATA A0
CS_MQ7 MQ7(MQ7TOG, 13);  // 12 = digital Pin connected to "tog" from sensor board
// 13 = digital Pin connected to LED Power Indicator

#include <DHT.h>
#define DHTTYPE DHT11
#define DHTPIN 2     // Pin del Arduino al cual esta conectado el pin 2 del sensor
DHT dht(DHTPIN, DHTTYPE);

#include <Emon.h>
#define CONSUMO1 A15
#define CONSUMO2 A14
#define CONSUMO3 A13
#define CONSUMO4 A12
#define CONSUMO5 A11

EnergyMonitor emon1; // Create an instance
EnergyMonitor emon2;
EnergyMonitor emon3;
EnergyMonitor emon4;
EnergyMonitor emon5;

// cambiarlo para que se ajuste a la red
/*byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };
IPAddress ip( xx, xx, xx, xx );
IPAddress ipdns( xx, xx, xx, xx );
IPAddress gateway( xx, xx, xx, xx );*/

IPAddress subnet( 255, 255, 255, 0 );


// change server to your email server ip or domain
//IPAddress server( 1, 2, 3, 4 );
const char server[] = "xx.xx.xx.xx";// "xx.xx.xx.xx";
const char From[] = "soporteavalon@xxx.es";
const char emailTo[] = "soporteavalon@xxx.es";//"jorge.belenguer@xxx.es";
EthernetClient mail_client;

//configuración para loguear contra el servicio web
const char RecopiladorDatosIP[] = "xx.xx.xx.xx";
const int RecopiladorDatosPuerto = 81;
EthernetClient RecopiladorDatos_client;

bool SDavailable;

EthernetServer http_server(80);

int CanvasX = 630;
int CanvasY = 290;

int HoraX = 315;


// Array valores
//               Actual  Diario  Semanal  Mensual  Anual
//  Temperatura    x        x       x        x       x
//  Humedad        x        x       x        x       x
//  Consumo        x        x       x        x       x
//  CO             x        x       x        x       x

double valores[20] = { 0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    200.0/1024.0, 0, 0, 0, 0
                  };
//{0:00,  1:00,  2:00,  3:00,  4:00, 5:00,
// 6:00,  7:00,  8:00,  9:00, 10:00, 11:00,
//12:00, 13:00, 14:00, 15:00, 16:00, 17:00,
//18:00, 19:00, 20:00, 21:00, 22:00, 23:00,
// 0:00}

double valoresTablaTemp[25] = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                0.0
                              };
double valoresTablaHumedad[25] = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0
                                 };
double valoresTablaConsumo[25] = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                                   0.0
                                 };
double valoresTablaCO[25] = { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                              0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                              0.0
                            };


//LCD
//creamos el objeto display LCD I2C
LiquidCrystal_I2C lcd(0x27, 2, 16); //lcd(0x27, 2, 1, 0, 4, 5, 6, 7, 3, NEGATIVE);
//Addr, En, Rw, Rs, d4, d5, d6, d7, backlighpin, polarity

//Alertas por incidencia
enum TipoAlerta
{
  AlertLow,
  WarningLow,
  Ok,
  WarningHigh,
  AlertHigh
} ;

TipoAlerta AlertaAnteriorTemp;
TipoAlerta AlertaAnteriorHum;
TipoAlerta AlertaAnteriorCO;

void printLCDSoft(String T11, int delayCharMs, int columna, int fila, bool clearAll, int delayMs)
{
  if (clearAll)
  {
    lcd.clear();
  }
  lcd.setCursor(columna, fila);        //Nos posiscionamos en la columna 0 fila 0

  for (int iLCD = 0; iLCD < T11.length(); iLCD++)
  {
    lcd.print(T11.charAt(iLCD));
    delay(delayCharMs);
  }
  delay(delayMs);
}

void setup()
{
  wdt_disable();//deshabilita el reset

  lcd.init();
  lcd.begin(16, 2, LCD_5x8DOTS);   //inicializamos el display como de 16 columnas y 2 filas
  lcd.backlight();


  printLCDSoft("Config. pinMode" , 100 , 0 , 0 , true , 0 );

  pinMode(53, OUTPUT);

  Serial.begin(9600);
  pinMode(puertoEth, OUTPUT);
  pinMode(puertoSD, OUTPUT);
  pinMode(CONSUMO1, INPUT);
  pinMode(CONSUMO2, INPUT);
  pinMode(CONSUMO3, INPUT);
  pinMode(CONSUMO4, INPUT);
  pinMode(CONSUMO5, INPUT);

  printLCDSoft("Habilita SD:" , 50 , 0 , 0 , true , 0 );
  //Serial.print("Starting SD...");
  enableSD();

  if (!SD.begin(puertoSD))
  {
    //Serial.println("failed");
    SDavailable = false;
    printLCDSoft("SD :-(" , 50 , 0 , 1 , false , 300 );
  }
  else
  {
    SDavailable = true;
    //Serial.println("ok");
    printLCDSoft("SD ;-)" , 50 , 0 , 1 , false , 300 );
  }

  printLCDSoft("Lectura de .cfg" , 50 , 0 , 0 , true , 0 );

  if (leeConfiguracion("configuracion.cfg"))
  {
    printLCDSoft(".cfg ;-)" , 50 , 0 , 1 , false , 300 );
  }
  else
  {
    printLCDSoft(".cfg :-(" , 50 , 0 , 1 , false , 300 );
  }


  printLCDSoft("Habilita Eth." , 50 , 0 , 0 , true , 0 );

  enableEther();

  Ethernet.begin(mac, ip, ipdns, gateway, subnet);

  printLCDSoft(String(Ethernet.localIP()[0]) + "." +
               String(Ethernet.localIP()[1]) + "." +
               String(Ethernet.localIP()[2]) + "." +
               String(Ethernet.localIP()[3]) ,
               100 , 0 , 1 , false , 2000 );

  printLCDSoft("Hora NTP UTC:" , 50 , 0 , 0 , true , 0 );

  //escucha NTP
  Udp.begin(localUdpPort);

  //Pongo en hora el Arduino
  GetHoraNTP();
  LastHour = CurrentHour;
  printLCDSoft(CurrentTime , 50 , 0 , 1 , false , 1000 );

  printLCDSoft("Monit. de consumo:" , 50 , 0 , 0 , true , 0 );
  emon1.current(CONSUMO1, 1800.0 / 62.0); // Current: input pin, calibration. Cur Const= Ratio/BurdenR. 1800/62 = 29.
  emon2.current(CONSUMO2, 1800.0 / 62.0);
  emon3.current(CONSUMO3, 1800.0 / 62.0);
  emon4.current(CONSUMO4, 1800.0 / 62.0);
  emon5.current(CONSUMO5, 1800.0 / 62.0);
  printLCDSoft("EMon: :-)" , 50 , 0 , 1 , false , 300 );

  printLCDSoft("Temp. y Humedad:" , 50 , 0 , 0 , true , 0 );
  dht.begin();
  printLCDSoft("DHT11: :-)" , 50 , 0 , 1 , false , 300 );

  delay(1000);

  enableEther();

  printLCDSoft("Servidor HTTP:80" , 50 , 0 , 0 , true , 0 );
  http_server.begin();
  printLCDSoft("HTTP: :-)" , 50 , 0 , 1 , false , 300 );
  /*Serial.print(("server is at "));
    Serial.println(Ethernet.localIP());*/

  printLCDSoft("GRUPO AVALON" , 100 , 0 , 0 , true , 0 );

  printLCDSoft("v2.0", 100, 0, 1, false, 500);

  Serial.println("Ready. Press 'e' to send.");
  Serial.println("Press 'r' to delete 'LOGDIA.CSV'");

  //Alertas
  AlertaAnteriorTemp = Ok;
  AlertaAnteriorHum = Ok;
  AlertaAnteriorCO = Ok;
}

void printLCD(String T11 = "", String T12 = "", String T13 = "",
              String T21 = "", String T22 = "", String T23 = "",
              int delayMs = 2000)
{
  lcd.clear();

  lcd.setCursor(0, 0);
  for (int iLCD = 0; iLCD < T11.length(); iLCD++)
  {
    lcd.print(T11.charAt(iLCD));
  }
  for (int iLCD = 0; iLCD < T12.length(); iLCD++)
  {
    lcd.print(T12.charAt(iLCD));
  }
  for (int iLCD = 0; iLCD < T13.length(); iLCD++)
  {
    lcd.print(T13.charAt(iLCD));
  }
  /*lcd.print(T11);
    lcd.print(T12);
    lcd.print(T13);*/

  lcd.setCursor(0, 1);
  for (int iLCD = 0; iLCD < T21.length(); iLCD++)
  {
    lcd.print(T21.charAt(iLCD));
  }
  for (int iLCD = 0; iLCD < T22.length(); iLCD++)
  {
    lcd.print(T22.charAt(iLCD));
  }
  for (int iLCD = 0; iLCD < T23.length(); iLCD++)
  {
    lcd.print(T23.charAt(iLCD));
  }
  /*lcd.print(T21);
    lcd.print(T22);
    lcd.print(T23);*/

  delay(delayMs);
}

void printLCD(String T11 = "", float T12 = (float)NAN, String T13 = "",
              String T21 = "", float T22 = (float)NAN, String T23 = "",
              int delayMs = 2000)
{
  String temp12 = "";
  String temp22 = "";
  /*Serial.print(T11);
    Serial.print(T12);
    Serial.println(T13);
    Serial.print(T21);
    Serial.print(T22);
    Serial.println(T23);*/


  if (!isnan(T12))
  {
    temp12 = (String)T12;
  }

  if (!isnan(T22))
  {
    temp22 = (String)T22;
  }


  printLCD(T11, temp12, T13, T21, temp22, T23, delayMs);
}

bool leeConfiguracion(String FicheroConfig)
{
  bool res = true;
  enableSD();
  //lee el fichero de configuraciÃ³n y modifica los valores
  return res;
}

void loop()
{
  byte inChar;

  inChar = Serial.read();

  if (inChar == 'e')
  {
    if (sendEmail(emailTo, "Test lectura y envio de Log", "A continuacion el log en formato CSV:", "LOGDIA.CSV") == 1)
    {
      Serial.println("Email sent");
    }
    else
    {
      Serial.println("Email failed");
    }
    /*}
      else
      {
      if (sendEmail(To, "Test lectura y envio de Log", "No existe el fichero LOGDIA.CSV")) Serial.println("Email sent");
      else Serial.println("Email failed");
      }*/
  }

  if (inChar == 'r')
  {
    enableSD();
    //elimina el anterior
    SD.remove("LOGDIA.CSV");
  }

  GetHoraNTP();

  readSensors();

  EvaluaAlertas();

  LogValues();
}

void enableEther()
{
  digitalWrite(puertoEth, LOW); //ethernet on
  digitalWrite(puertoSD, HIGH); //SD card off
}

void enableSD()
{
  digitalWrite(puertoEth, HIGH); //ethernet off
  digitalWrite(puertoSD, LOW); //SD card on
}



byte sendEmail(String To, String Subject, String MSG, String Fichero)
{
  enableEther();
  Serial.println(MSG);
  byte thisByte = 0;
  byte respCode;

  if (mail_client.connect(server, 587) == 1) {
    Serial.println("connected");
  } else {
    Serial.println("connection failed");
    return 0;
  }

  if (!eRcv()) return 0;
  //Serial.println(("Sending helo"));

  // change to your public ip
  mail_client.println(("helo 10.28.1.244"));

  if (!eRcv()) return 0;
  Serial.print("Sending From: ");
  Serial.println(From);
  // change to your email address (sender)
  mail_client.print("MAIL From: <" );
  mail_client.print(From);
  mail_client.println(">");

  if (!eRcv()) return 0;

  // change to recipient address
  Serial.print("Sending To: ");
  Serial.println(To);
  mail_client.print("RCPT To: <");
  mail_client.print(To);
  mail_client.println(">");

  if (!eRcv()) return 0;

  Serial.println("Sending DATA");
  mail_client.println("DATA");

  if (!eRcv()) return 0;

  //Serial.println(("Sending email"));

  // change to recipient address
  mail_client.print("To: Tester <" );
  mail_client.print( To );
  mail_client.println( ">");

  // change to your address
  mail_client.print("From: Arduino <" );
  mail_client.print( From );
  mail_client.println(">");

  mail_client.print("Subject: ");
  mail_client.print(Subject);
  mail_client.println("\r\n");

  //Serial.print("Sending Message: ");
  mail_client.print(MSG);
  mail_client.println();

  String tempMSG = "";
  enableSD();
  File ArchivoTest;

  if (SD.exists(Fichero))
  {
    ArchivoTest = SD.open(Fichero);

    tempMSG = "Sin Abrir el fichero ";
    tempMSG.concat(Fichero);

    if (ArchivoTest)
    {
      tempMSG = "";
      byte loops = 0;
      while (ArchivoTest.available())
      {
        tempMSG.concat((char)ArchivoTest.read());

        loops++;
        if (loops >= 250)
        {
          int str_len = tempMSG.length() + 1;
          char MSG_char_array[str_len];
          tempMSG.toCharArray(MSG_char_array, str_len);
          tempMSG = "";
          loops = 0;

          enableEther();
          //Serial.println(MSG_char_array);
          mail_client.print(MSG_char_array);
          enableSD();
        }
      }

      if (tempMSG.length() > 0)
      {
        int str_len = tempMSG.length() + 1;
        char MSG_char_array[str_len];
        tempMSG.toCharArray(MSG_char_array, str_len);
        enableEther();
        //Serial.println(MSG_char_array);
        mail_client.print(MSG_char_array);
      }

      enableSD();
      ArchivoTest.close();
    }
  }

  enableEther();

  mail_client.println();
  mail_client.println();
  mail_client.println(".");

  if (!eRcv()) return 0;

  Serial.println("Sending QUIT");
  mail_client.println("QUIT");

  if (!eRcv()) return 0;

  mail_client.stop();

  Serial.println(("disconnected"));

  return 1;
}

byte eRcv()
{
  byte respCode;
  byte thisByte;
  int loopCount = 0;

  while (!mail_client.available()) {
    delay(1);
    loopCount++;

    // if nothing received for 10 seconds, timeout
    if (loopCount > 10000) {
      mail_client.stop();
      Serial.println(("\r\nTimeout"));
      return 0;
    }
  }

  respCode = mail_client.peek();

  while (mail_client.available())
  {
    thisByte = mail_client.read();
    Serial.write(thisByte);
  }

  if (respCode >= '4')
  {
    efail();
    return 0;
  }

  return 1;
}


void efail()
{
  byte thisByte = 0;
  int loopCount = 0;

  mail_client.println("QUIT");

  while (!mail_client.available()) {
    delay(1);
    loopCount++;

    // if nothing received for 10 seconds, timeout
    if (loopCount > 10000) {
      mail_client.stop();
      Serial.println(("\r\nTimeout"));
      return;
    }
  }

  while (mail_client.available())
  {
    thisByte = mail_client.read();
    Serial.write(thisByte);
  }

  mail_client.stop();

  Serial.println("disconnected");
}

void http()
{
  int IndxVal = 0;
  enableEther();

  // listen for incoming clients
  EthernetClient http_client = http_server.available();
  if (http_client)
  {
    //Serial.println(("new http_client"));
    // an http request ends with a blank line
    bool currentLineIsBlank = true;
    while (http_client.connected())
    {
      if (http_client.available())
      {
        char c = http_client.read();
        Serial.print(c);
        // if you've gotten to the end of the line (received a newline
        // character) and the line is blank, the http request has ended,
        // so you can send a reply
        if (c == '\n' && currentLineIsBlank)
        {
          http_client.println("HTTP/1.1 200 OK");
          http_client.println("Content-Type: text/html");
          http_client.println("Connection: close");  // the connection will be closed after completion of the response
          http_client.println("Refresh: 30");  // refresh the page automatically every 5 sec
          http_client.println();

          if (SDavailable)
          {
            enableSD();
            File Archivo1;
            Archivo1 = SD.open("index.htm");

            if (Archivo1)
            {
              String Resultado = "";
              byte Bin;
              int CountChar = 0;
              String sRes = "";

              while (Archivo1.available())
              {
                Bin = Archivo1.read();

                //Serial.print(char(Bin));
                //Serial.println(Bin);

                if (Bin == 183)//·
                {
                  if (IndxVal < 25) //Temperatura
                  {
                    /*Serial.print("T ");
                      Serial.print(IndxVal);*/

                    //sRes = (String)valoresTablaTemp[IndxVal];
                    //map(value, fromLow, fromHigh, toLow, toHigh)
                    sRes = (String)map(valoresTablaTemp[IndxVal],
                                       5, 45,
                                       CanvasY, 0);
                    /*Serial.print(" ");
                      Serial.println(sRes);*/
                    CountChar += sRes.length();
                  }
                  else if (25 <= IndxVal && IndxVal < 50)//Humedad
                  {
                    /*Serial.print("H ");
                      Serial.println(IndxVal);*/

                    //sRes = (String)valoresTablaHumedad[IndxVal - 25];
                    //map(value, fromLow, fromHigh, toLow, toHigh)
                    sRes = (String)map(valoresTablaHumedad[IndxVal - 25],
                                       10, 90,
                                       CanvasY, 0);
                    /*Serial.print(" ");
                      Serial.println(sRes);*/
                    CountChar += sRes.length();
                  }
                  else if (50 <= IndxVal && IndxVal < 75)//Consumo
                  {
                    /*Serial.print("W ");
                      Serial.println(IndxVal);*/

                    //sRes = (String)valoresTablaConsumo[IndxVal - 50];
                    //map(value, fromLow, fromHigh, toLow, toHigh)
                    sRes = (String)map(valoresTablaConsumo[IndxVal - 50],
                                       1, 5000,
                                       CanvasY, 0);
                    /*Serial.print(" ");
                      Serial.println(sRes);*/
                    CountChar += sRes.length();
                  }
                  else if (75 <= IndxVal && IndxVal < 100)//CO
                  {
                    /*Serial.print("C ");
                      Serial.println(IndxVal);*/

                    //sRes = (String)valoresTablaCO[IndxVal - 75];
                    //map(value, fromLow, fromHigh, toLow, toHigh)
                    sRes = (String)map(valoresTablaCO[IndxVal - 75],
                                       400, 3100,
                                       CanvasY, 0);
                    /*Serial.print(" ");
                      Serial.println(sRes);*/
                    CountChar += sRes.length();
                  }
                  else if (100 <= IndxVal && IndxVal < 120) //tabla resumen
                  {
                    /*Serial.print("R ");
                      Serial.println(IndxVal);*/
                    sRes = (String)valores[IndxVal - 100];
                    /*Serial.print(" ");
                      Serial.println(sRes);*/
                    CountChar += sRes.length();
                  }
                  else
                  {
                    //sin acciones
                    Serial.print("SA ");
                    Serial.println(IndxVal);
                  }
                  IndxVal++;
                }
                else if (Bin == 124)//|
                {
                  //Barra indicadora de la hora UTC
                  sRes = (String)(map(CurrentHour,
                                      0, 24,
                                      0, CanvasX) +
                                  map(CurrentMinute,
                                      0, 60,
                                      0, CanvasX / 24));
                  CountChar += sRes.length();
                }
                else
                {
                  sRes = "";
                  sRes += char(Bin);
                  CountChar++;
                }

                Resultado += sRes;

                if (CountChar > 250)
                {
                  CountChar = 0;
                  enableEther();

                  http_client.print(Resultado);

                  enableSD();

                  Resultado = "";
                }
              }

              enableSD();
              Archivo1.close();

              enableEther();
              http_client.print(Resultado);
            }
            else
            {
              Serial.println("Error al leer el archivo inicial");
            }
          }
          else
          {
            http_client.println("No se encuentra la SD.");
          }
          break;
        }
        if (c == '\n') {
          // you're starting a new line
          currentLineIsBlank = true;
        } else if (c != '\r') {
          // you've gotten a character on the current line
          currentLineIsBlank = false;
        }
      }
    }
    // give the web browser time to receive the data
    delay(1);
    // close the connection:
    http_client.stop();
    Serial.println("http_client disconnected");
    Ethernet.maintain();
  }
}



//Valores para estadísticos
//Horarios
double SumaTemp = 0.0;
int ConteoTemp = 0;
double SumaHum = 0.0;
int ConteoHum = 0;
double SumaCO = 0.0;
int ConteoCO = 0;
double SumaConsumo = 0.0;
int ConteoConsumo = 0;

int passLog = 0;
void LogValues()
{
  LogSD();
  LogBD();
}

void LogSD()
{
 if (SDavailable)
  {
    passLog++;

    if (passLog >= 5)
    {
      passLog = 0;

      enableSD();

      bool TempExist = SD.exists("LOGDIA.CSV");

      File Archivo3 = SD.open("LOGDIA.CSV", FILE_WRITE);

      if (!TempExist)
      {
        Serial.println("No Existe LOGDIA.CSV");
        //escribe las cabeceras inicialescabeceras
        Archivo3.println("Hora(UTC);Temperatura(ºC);Humedad(%);Consumo(w);CO(ppm)");
      }

      Archivo3.print(CurrentTime);
      Archivo3.print(';');

      //Hoy, última lectura
      Archivo3.print(valores[0]);//Temperatura
      Archivo3.print(";");
      Archivo3.print(valores[5]);//Humedad
      Archivo3.print(";");
      Archivo3.print(valores[10]);//Consumo
      Archivo3.print(";");
      Archivo3.println(valores[15]);//CO

      //Diarios
      //Mensuales
      //Anuales
      
      Archivo3.close();
    }
  } 
}

void LogBD()
{
  //Envía HTTP a WS
  enableEther();

  if (RecopiladorDatos_client.connect(RecopiladorDatosIP, RecopiladorDatosPuerto) == 1) {
    Serial.println("WS connected");
  } else {
    Serial.println("WS connection failed");
    return;
  }

  RecopiladorDatos_client.print("GET /Service1.svc/rest/SetData?CadenaLectura="); // Enviamos los datos por GET
    Serial.print("GET /Service1.svc/rest/SetData?CadenaLectura=");
  RecopiladorDatos_client.print("");//hora del servidor
  Serial.print("");
  RecopiladorDatos_client.print('|');
  Serial.print("|");
  RecopiladorDatos_client.print(ip[0]);
  Serial.print(ip[0]);
  RecopiladorDatos_client.print('.');
  Serial.print('.');
  RecopiladorDatos_client.print(ip[1]);
  Serial.print(ip[1]);
  RecopiladorDatos_client.print('.');
  Serial.print('.');
  RecopiladorDatos_client.print(ip[2]);
  Serial.print(ip[2]);
  RecopiladorDatos_client.print('.');
  Serial.print('.');
  RecopiladorDatos_client.print(ip[3]);
  Serial.print(ip[3]);
  RecopiladorDatos_client.print('|');
  Serial.print('|');
  RecopiladorDatos_client.print("T@");
  Serial.print("T@");
  RecopiladorDatos_client.print(valores[0]);
  Serial.print(valores[0]);
  RecopiladorDatos_client.print('|');
  Serial.print('|');
  RecopiladorDatos_client.print("H@");
  Serial.print("H@");
  RecopiladorDatos_client.print(valores[5]);
  Serial.print(valores[5]);
  
  RecopiladorDatos_client.print('|');
  Serial.print('|');
  RecopiladorDatos_client.print("W@");
  Serial.print("W@");
  RecopiladorDatos_client.print(valores[10]);
  Serial.print(valores[10]);
  
  RecopiladorDatos_client.print('|');
  Serial.print('|');
  
  RecopiladorDatos_client.print("C@");
  Serial.print("C@");
  RecopiladorDatos_client.println(valores[15]);
  Serial.println(valores[15]);

  RecopiladorDatos_client.println(" HTTP/1.0");
  Serial.println(" HTTP/1.0");
  RecopiladorDatos_client.println("User-Agent: Arduino 1.0");
  Serial.println("User-Agent: Arduino 1.0");
  RecopiladorDatos_client.println();
   Serial.println();

if (!RecopiladorDatos_client.connected()) {
    Serial.println("WS Disconnected!");
  }
  
  RecopiladorDatos_client.stop();
  RecopiladorDatos_client.flush();
}

void readSensors()
{
  //Lee Los sensores y prepara para almacenar el historico
  float tt = dht.readTemperature(false, true);
  valores[0] = tt;
  /*Serial.print("Temp: ");
    Serial.print(tt);
    Serial.println("ÂºC");*/
  float hh = dht.readHumidity();
  valores[5] = hh;
  /*Serial.print("H rel: ");
    Serial.print(hh);
    Serial.println("%");*/

  //EvalÃºa hay que hacer un resumen de los datos
  if (isnan(hh) || isnan(tt))
  {
    printLCD("Failed to read", "", "", "from DHT sensor!");
  }
  else
  {
    printLCD("T: ", tt, "C", "Hr: ", hh, "%");

    valoresTablaTemp[LastHour] = tt;
    valoresTablaHumedad[LastHour] = hh;

    SumaTemp += tt;
    ConteoTemp++;
    SumaHum += hh;
    ConteoHum++;

  }

  http();

  //Obtiene los valores de CO2
  MQ7.CoPwrCycler();
  if (MQ7.CurrentState() == LOW)
  {
    //we are at 1.4v, read sensor data!
    int CoData = analogRead(MQ7DATA);
    valores[15] = (double)CoData/1024.0;

    valoresTablaCO[LastHour] = valores[15];
    SumaCO += valores[15];
    ConteoCO++;

    //Serial.println(CoData);
    printLCD("CO: ", valores[15], "ppm", "", (float)NAN, "");
  }
  else
  {
    //sensor is at 5v, heating time
    //Serial.println("CO2 sensor heating!");
    printLCD("CO   sensor", (float)NAN, "", "heating! ", (float)valores[15]);
  }

  http();

  //Obtiene los valores de consumo instantaneo
  double Irms = emon1.calcIrms(1480);//numero de muestras
  //double Vrms = emon1.calcVrms(1480);

  printLCD("I1: ", Irms, "A",
           "W1: ", (230.0 * Irms), "w");

  valoresTablaConsumo[LastHour] = 230.0 * Irms;
  SumaConsumo += (230.0 * Irms);
  ConteoConsumo++;

  valores[10] = (230.0 * Irms);

  http();

  Irms = emon2.calcIrms(1480);
  //Vrms = emon2.calcVrms(1480);

  printLCD("I2: ", Irms, "A",
           "W2: ", (230.0 * Irms), "w");

  http();

  Irms = emon3.calcIrms(1480);
  //Vrms = emon3.calcVrms(1480);

  printLCD("I3: ", Irms, "A",
           "W3: ", (230.0 * Irms), "w");

  http();

  Irms = emon4.calcIrms(1480);
  //Vrms = emon4.calcVrms(1480);

  printLCD("I4: ", Irms, "A",
           "W4: ", (230.0 * Irms), "w");

  http();

  Irms = emon5.calcIrms(1480);
  //Vrms = emon5.calcVrms(1480);

  printLCD("I5: ", Irms, "A",
           "W5: ", (230.0 * Irms), "w");

  http();

  if (CurrentHour != LastHour)
  {
    //Calcula e inicializa
    valoresTablaTemp[LastHour] = ConteoTemp == 0 ? 0.0 : SumaTemp / (double)ConteoTemp;
    valoresTablaHumedad[LastHour] = ConteoHum == 0 ? 0.0 : SumaHum / (double)ConteoHum;
    valoresTablaConsumo[LastHour] = ConteoConsumo == 0 ? 0.0 : SumaConsumo / (double)ConteoConsumo;
    valoresTablaCO[LastHour] = ConteoCO == 0 ? 0.0 : SumaCO / (double)ConteoCO;

    if (LastHour == 0)
    {
      valoresTablaTemp[24] = valoresTablaTemp[LastHour];
      valoresTablaHumedad[24] = valoresTablaHumedad[LastHour];
      valoresTablaConsumo[24] = valoresTablaConsumo[LastHour];
      valoresTablaCO[24] = valoresTablaCO[LastHour];
    }

    //empleo las variables como temporales
    SumaTemp = 0.0;
    SumaHum = 0.0;
    SumaCO = 0.0;
    SumaConsumo = 0.0;

    //Evalua si hay que calcular el resumen diario
    if (LastHour == 23 && CurrentHour == 0)
    {
      for (int i = 0; i < 24; i++)
      {
        SumaTemp += valoresTablaTemp[i];
        ConteoHum += valoresTablaHumedad[i] ;
        SumaConsumo += valoresTablaConsumo[i];
        SumaCO += valoresTablaCO[i] = ConteoCO ;
      }
      valores[1] = SumaTemp / 23.0; //Temperatura Diaria
      valores[6] = ConteoHum / 23.0; //Humedad Diaria
      valores[11] = SumaCO / 23.0; //CO Diaria
      valores[16] = SumaConsumo / 23.0; //Consumo Diaria*/
    }

    //resetea las variables
    SumaTemp = 0.0;
    ConteoTemp = 0;
    SumaHum = 0.0;
    ConteoHum = 0;
    SumaCO = 0.0;
    ConteoCO = 0;
    SumaConsumo = 0.0;
    ConteoConsumo = 0;
  }
}

//Obtiene la hora NTP
void GetHoraNTP()
{
  enableEther();
  sendNTPpacket(timeServer);   // Enviar unaa peticion al servidor NTP

  delay(1000);                  // Damos tiempo a la respuesta
  if ( Udp.parsePacket() )
  {
    Serial.println("Hemos recibido un paquete");
    Udp.read(packetBuffer, NTP_PACKET_SIZE); // Leemos el paquete

    // La hora empieza en el byte 40 de la respuesta y es de 4 bytes o 2 words
    // Hay que extraer ambas word:

    unsigned long highWord = word(packetBuffer[40], packetBuffer[41]);
    unsigned long lowWord = word(packetBuffer[42], packetBuffer[43]);
    // combine the four bytes (two words) into a long integer
    // this is NTP time (seconds since Jan 1 1900):
    unsigned long secsSince1900 = highWord << 16 | lowWord;
    Serial.print("Segundos desde 1 Enero de 1900 = " );
    Serial.println(secsSince1900);

    // Ahora a convertir el tiempo NTP en tiempo Uxix:
    Serial.print("Unix time = ");
    // Unix time starts on Jan 1 1970. In seconds, that's 2208988800:
    const unsigned long seventyYears = 2208988800UL;
    // subtract seventy years:
    unsigned long epoch = secsSince1900 - seventyYears;
    // print Unix time:
    Serial.println(epoch);

    Serial.print("The UTC time is ");   // UTC es el tiempo universal o GMT
    //Serial.print((epoch  % 86400L) / 3600);   // Horas (86400 segundos por dia)
    int tempHoras = (epoch  % 86400L) / 3600;
    Serial.print(tempHoras);   // Horas (86400 segundos por dia)
    LastHour = CurrentHour;
    CurrentHour = tempHoras;
    CurrentTime = (String)tempHoras;
    Serial.print(':');
    CurrentTime += ':';
    if (((epoch % 3600) / 60) < 10 )
    {
      // Añadir un 0 en los primeros 9 minutos
      Serial.print('0');
      CurrentTime += '0';
    }
    //Serial.print((epoch  % 3600) / 60); // Minutos:
    tempHoras = (epoch  % 3600) / 60;
    CurrentMinute = tempHoras;
    Serial.print(tempHoras); // Minutos:
    CurrentTime += tempHoras;
    Serial.print(':');
    CurrentTime += ':';
    tempHoras = epoch % 60;
    if ( tempHoras < 10 )
    {
      // Añadir un 0 en los primeros 9 minutos
      Serial.print('0');
      CurrentTime += '0';
    }
    Serial.println(tempHoras);              // Segundos
    CurrentTime += (String)tempHoras;
    /*Serial.print("CurrentHour ");
      Serial.println(CurrentHour);
      Serial.print("LastHour ");
      Serial.println(LastHour);*/

    //Envío de Log por tiempo
    if (SDavailable && SD.exists("LOGDIA.CSV") && (CurrentHour < LastHour) == 1)
      //if (CurrentHour == 0 && LastHour == 23)//paso de media noche
    {
      Serial.println();
      Serial.println();
      Serial.println();
      Serial.println("Cambio de hora:");
      Serial.print("Current: ");
      Serial.println(CurrentHour);
      Serial.print("Last: ");
      Serial.println(LastHour);

      String AsuntoLog = "Reporte Diario Arduino ";
      AsuntoLog.concat(ip[0]);
      AsuntoLog.concat(".");
      AsuntoLog.concat(ip[1]);
      AsuntoLog.concat(".");
      AsuntoLog.concat(ip[2]);
      AsuntoLog.concat(".");
      AsuntoLog.concat(ip[3]);

      if (sendEmail(emailTo, "Reporte Diario Arduino " + String(ip[0]) +"."+ String(ip[1]) +"."+ String(ip[2]) +"."+ String(ip[3]), "Log Diario:", "LOGDIA.CSV") == 1)
      {
        enableSD();
        //elimina el anterior
        SD.remove("LOGDIA.CSV");
      }

      //reinicia el sistema
      Serial.println("RESET");
      wdt_enable(WDTO_1S);//Habilita el reset en 1 Segundo
    }
  }
}

//Solicitar hora NTP
unsigned long sendNTPpacket(IPAddress & address)
{
  // set all bytes in the buffer to 0
  memset(packetBuffer, 0, NTP_PACKET_SIZE);
  // Initialize values needed to form NTP request
  // (see URL above for details on the packets)
  packetBuffer[0] = 0b11100011;   // LI, Version, Mode
  packetBuffer[1] = 0;     // Stratum, or type of clock
  packetBuffer[2] = 6;     // Polling Interval
  packetBuffer[3] = 0xEC;  // Peer Clock Precision
  // 8 bytes of zero for Root Delay & Root Dispersion
  packetBuffer[12]  = 49;
  packetBuffer[13]  = 0x4E;
  packetBuffer[14]  = 49;
  packetBuffer[15]  = 52;

  // all NTP fields have been given values, now
  // you can send a packet requesting a timestamp:

  Udp.beginPacket(address, 123); //NTP requests are to port 123
  Udp.write(packetBuffer, NTP_PACKET_SIZE);
  Udp.endPacket();
}

void EvaluaAlertas()
{
  EvaluaAlertaTemp();

  EvaluaAlertaHum();

  EvaluaAlertaCO();
}

void EvaluaAlertaTemp()
{
  //AlertaAnteriorTemp
  TipoAlerta Estado = Ok;
  bool Informa = false;

  String AsuntoAlerta = "Temperatura ";
  String InfoAlerta = "Anomalía en la Temperatura del CPD:";
  InfoAlerta.concat("\n");
  InfoAlerta.concat("El margen de Operación está comprendido entre 15ºC y 28ºC siendo el valor actual de ");
  InfoAlerta.concat(valores[0]);
  InfoAlerta.concat("ºC.");

  /*valores[0]//temp
    valores[5]//humedad
    valores[15]//co*/

  //Temperatura
  if ((valores[0] < 12.0) == 1) //alert high
  {
    Estado = AlertLow;
    AsuntoAlerta.concat("Crítica");
  }
  else if ((valores[0] < 15.0) == 1) //warning low
  {
    Estado = WarningLow;
    AsuntoAlerta.concat("Baja");
  }
  else if ((valores[0] > 32) == 1) //alert high
  {
    Estado = AlertHigh;
    AsuntoAlerta.concat("Crítica");
  }
  else if ((valores[0] > 28.0) == 1) //warning high
  {
    Estado = WarningHigh;
    AsuntoAlerta.concat("Alta");
  }
  else//Ok
  {
    Estado = Ok;
    AsuntoAlerta.concat("Normal");
    InfoAlerta.concat("\n");
    InfoAlerta.concat("El CPD vuelve a la normalidad. ;-)");
  }

  AsuntoAlerta.concat(" en " +
                      String(ip[0]) + "." +
                      String(ip[1]) + "." +
                      String(ip[2]) + "." +
                      String(ip[3]));

  if (AlertaAnteriorTemp != Estado)
  {
    if (InformaAlertas(AsuntoAlerta, InfoAlerta))
    {
      AlertaAnteriorTemp = Estado;
    }
  }
}

void EvaluaAlertaHum()
{
  //AlertaAnteriorHum
  TipoAlerta Estado = Ok;
  bool Informa = false;

  String AsuntoAlerta = "Humedad ";
  String InfoAlerta = "Anomalía en la Humedad del CPD:";
  InfoAlerta.concat("\n");
  InfoAlerta.concat("El margen de Operación está comprendido entre 15% y 70% siendo el valor actual de ");
  InfoAlerta.concat(valores[0]);
  InfoAlerta.concat("%.");
  /*valores[0]//temp
    valores[5]//humedad
    valores[15]//co*/

  //Humedad
  if (valores[5] < 12.0)//alert high
  {
    Estado = AlertLow;
    AsuntoAlerta.concat("Crítica");
  }
  else if (valores[5] < 15.0)//warning low
  {
    Estado = WarningLow;
    AsuntoAlerta.concat("Baja");
  }
  else if (valores[5] > 82.0)//alert high
  {
    Estado = AlertHigh;
    AsuntoAlerta.concat("Crítica");
  }
  else if (valores[5] > 70.0)//warning high
  {
    Estado = WarningHigh;
    AsuntoAlerta.concat("Alta");
  }
  else//Ok
  {
    Estado = Ok;
    AsuntoAlerta.concat("Normal");
    InfoAlerta.concat("\n");
    InfoAlerta.concat("El CPD vuelve a la normalidad. ;-)");
  }

  AsuntoAlerta.concat(" en " +
                      String(ip[0]) + "." +
                      String(ip[1]) + "." +
                      String(ip[2]) + "." +
                      String(ip[3]));

  if (AlertaAnteriorHum != Estado)
  {
    if (InformaAlertas(AsuntoAlerta, InfoAlerta))
    {
      AlertaAnteriorHum = Estado;
    }
  }
}

void EvaluaAlertaCO()
{
  //AlertaAnteriorCO
  TipoAlerta Estado = Ok;

  String AsuntoAlerta = "CO ";
  String InfoAlerta = "Anomalía en la concentración de CO del CPD:";
  InfoAlerta.concat("\n");
  InfoAlerta.concat("El margen de Operación está comprendido entre 100ppm y 1100ppm siendo el valor actual de ");
  InfoAlerta.concat(valores[15]);
  InfoAlerta.concat("ppm.");

  /*valores[0]//temp
    valores[5]//humedad
    valores[15]//co*/

  //CO
  if (valores[15] < 0.0)//alert low
  {
    Estado = AlertLow;
    AsuntoAlerta.concat("Crítico");
  }
  else if (valores[15] < 0.0)//warning low
  {
    Estado = WarningLow;
    AsuntoAlerta.concat("Bajo");
  }
  else if (valores[15] > 1.5)//alert high
  {
    Estado = AlertHigh;
    AsuntoAlerta.concat("Crítico");
  }
  else if (valores[15] > 1.2)//warning high
  {
    Estado = WarningHigh;
    AsuntoAlerta.concat("Alto");
  }
  else//Ok
  {
    Estado = Ok;
    AsuntoAlerta.concat("Normal");
    InfoAlerta.concat("\n");
    InfoAlerta.concat("El CPD vuelve a la normalidad. ;-)");
  }

  AsuntoAlerta.concat(" en " +
                      String(ip[0]) + "." +
                      String(ip[1]) + "." +
                      String(ip[2]) + "." +
                      String(ip[3]));
  if (AlertaAnteriorCO != Estado)
  {
    if (InformaAlertas(AsuntoAlerta, InfoAlerta))
    {
      AlertaAnteriorCO = Estado;
    }
  }
}

bool InformaAlertas(String Asunto, String Info)
{
  //Envía mail
  if (sendEmail(emailTo, Asunto, Info, "") == 1)
  {
    return true;
  }
  else
  {
    return false;
  }
}


