#include <Arduino.h>
#include <Wire.h> 
#include <LiquidCrystal_I2C.h>
LiquidCrystal_I2C lcd(0x27,20,4); 

#define pin_do 9
#define pin_xanh 10
#define pin_analog  A0
// r chan quat vs coi dau
int relay = 6; // quat
int speaker = 4; // coi
int dk_quat = 0;

String inString="";
boolean stringComplete=false;
String commandString="";

float value_ghd=1500;
float value_ghq=3000;
float value;
float value_test;
float sensorvalue;
void setup() {
  Serial.begin(9600);
  lcd.init();
  lcd.backlight();
  pinMode(pin_do,OUTPUT);
  pinMode(pin_xanh,OUTPUT);
  pinMode(relay,OUTPUT);
  pinMode(speaker,OUTPUT);
  lcd.setCursor(3,0);
  lcd.print("BAI TAP LON");
  lcd.setCursor(6,1);
  lcd.print("AM18");
  delay(1000); 
  lcd.clear(); 
  pinMode(pin_analog,INPUT);

}
void qled(){
 lcd.setCursor(3,0);
 lcd.print("ro ri gas");
 lcd.setCursor(3,0);
 lcd.print("canh bao");
  }
void tlcd(){
  if((value_ghq>value)&&(value_ghd>value)){
     lcd.setCursor(2,0);
     lcd.print("...An Toan...");
     lcd.setCursor(0,1);
     lcd.print("GIA TRI:");
     lcd.setCursor(9,1);
     lcd.print(value);     
    }
  }
void control_pin()
{
  if((value_ghd<value)&&(value_ghq>value)) 
  {
    lcd.clear();
    digitalWrite(pin_do,HIGH);
    digitalWrite(pin_xanh,LOW);
    digitalWrite(speaker,LOW);
    qled();    
  }
  if(value_ghq<value)
  {  lcd.clear();
    for(int i=0;i<7;i++){
    digitalWrite(relay,HIGH);
    digitalWrite(pin_do,HIGH); 
    digitalWrite(pin_xanh,LOW);
    digitalWrite(speaker,HIGH);
    qled();
    delay(1000);
      if(i>=7)
        break;     
      }  
  }  
  if((value_ghq>value)&&(value_ghd>value))
  { 
    digitalWrite(relay,LOW);
    digitalWrite(pin_do,LOW);
    digitalWrite(pin_xanh,HIGH);
    digitalWrite(speaker,LOW);
    lcd.clear();
    tlcd();
  }
}
float read_set()
{
  float i=0;
  if(inString.length()>0)
  {
    i=inString.substring(4,8).toFloat();
  }
  return i;
}
boolean get_pin_state()
{
  boolean state=false;
  if(inString.substring(4,6)=="ON")
  {
    state=true;
  }
  else if(inString.substring(4,6)=="OF")
  {
    state=false;
  }
  return state;
}
  void loop() {
     float sensor_volt;
     float RS_air; // Get value of RS in a GAS
     float ratio; // Get ratio RS_GAS/RS_air
     float R0;
     float ra;
     value_test = analogRead(A0);
    for(int x = 0 ; x < 100 ; x++)
    {
     sensorvalue = sensorvalue + value_test;
     delay(1);
    }
    sensorvalue = sensorvalue/100.0; // tính trung bình
    /* d là giá trị analog;
     sai số đo thực tế là 0.01V; 
     0.01V =(4.62*X%)/100% => X=0.002%; X là phần trăm của 0.01V 
     P=d/9.48, giá trị max của analog trên cảm biến là 948:P là phần trăm của tỷ số analog và 9.48
     => gia trị sai số d=9.48*0.002% => d(sai số) = 0.02 
     */
     value  = map(sensorvalue, 0, 1023, 300, 10000);
     sensor_volt=(sensorvalue/1024)*5.0;
     RS_air = (5.0-sensor_volt)/sensor_volt; // omit *RL

     
    Serial.println(value);
    delay(400);
    control_pin();

    while (Serial.available()>0) 
  {
    char val= (char)Serial.read();
    inString+=val;
    if(val=='\n')
    {
      stringComplete=true;
    } 
  }
  
 if(stringComplete)
  {
    stringComplete=false;
    //c=inString.substring(4,8).toFloat();
    if(inString.substring(1,4)=="SED")
    {
      value_ghd=read_set();
      control_pin();
    }
    else if(inString.substring(1,4)=="SEQ")
    {
      value_ghq=read_set();
      control_pin();
    }
    else if(inString.substring(1,4)=="FAN")
    {

      boolean state_fan = get_pin_state();
      if(state_fan == true)
      {
        digitalWrite(6, HIGH);
      }
      else
      {
        digitalWrite(6, LOW);
      }
    }
    else if(inString.substring(1,4)=="COI")
    {
       boolean state_coi = get_pin_state();
       if(state_coi == true)
       {
        digitalWrite(4, HIGH);
      }
      else
      {
        digitalWrite(4, LOW);
      }
    }
    inString=""; 
   }
  }
   
 
