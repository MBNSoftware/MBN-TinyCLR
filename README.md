[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) 

![MBN logo](logonarrow.jpg)

# **Available drivers**

The list of all drivers and their status is [here](https://github.com/MikroBusNet/MBN-TinyCLR/blob/master/DriversStatus.md).

# **Please note**

More drivers will be added as soon as they are verified by TinyCLR 2.0. Those are the drivers tagged with the clock symbol :clock130: in the above list.

## **How to use our drivers**

It's as simple as that :

* Create a TinyCLR application project in Visual Studio
* Add [**_MBNCore.cs_**](https://github.com/MikroBusNet/MBN-TinyCLR/tree/master/MBNCore) to your project
* Add the needed driver source as well, from the [Drivers folder](https://github.com/MikroBusNet/MBN-TinyCLR/tree/master/Drivers)
* Use the example in the [Examples folder](https://github.com/MikroBusNet/MBN-TinyCLR/tree/master/Examples) as a start for your program
* That's all !

## **How it works**

The main concept in MBN Core is "Sockets".
Sockets designate the physical Mikrobus sockets available on different boards.

So far, the current boards are supported :

* MBN Ram (6 sockets)
* GHI SC20100 dev board (2 sockets)
* GHI SC20260 dev board (2 sockets)
* GHI FEZ Stick (2 sockets)
  
Each socket has a number and is accessible in code using the following syntax :

```csharp
_transmitter = new T4_20mAClick(Hardware.SocketOne);  // Socket #1 on Ram board
_receiver = new T4_20mAClick(Hardware.SC20260_1);  // Socket #1 on SC20260D board
```

## **Virtual sockets**

If you don't use a Click module or if you connect it on the exposed headers, then you may ask _'which socket should I use ?!'_

Answer is really simple : **create your own virtual socket !**

Imagine that the driver you want to use needs pins AN, RST, CS and INT. You would then create a virtual socket with those pins :

```csharp
var virtualSocket = new Hardware.Socket
{
    AnPin = SC20260.GpioPin.PF10,
    Rst = SC20260.GpioPin.PI8,
    Cs = SC20260.GpioPin.PG12,
    Int = SC20260.GpioPin.PG6,
};
```

Then you pass that socket to the driver :

```csharp
var _lcd = new DevantechLcd03(virtualSocket, 0xC8 >> 1);
var _lcdOnOff = GpioController.GetDefault().OpenPin(virtualSocket.Rst);
```
