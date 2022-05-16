[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) 

![MBN logo](site_logo.png)

# **Available drivers**

## TinyCLR
The list of all drivers and their status is [here](TinyCLRDriversStatus.md).

## nanoFramework
The list of all drivers and their status is [here](nanoFrameworkDriversStatus.md).

# **Please note**

More drivers will be added as soon as they are verified as working on TinyCLR 2.0 and/or nanoFramework. Drivers tagged with the clock symbol :clock130: are yet to be verified.

## **How to use the drivers**
### TinyCLR
It's as simple as :

* Create a TinyCLR application project in Visual Studio
* Add [**_MBNCore.cs_**](MBNCore) to your project
* Add [**_MBNHardware.cs_**](MBNCore/MBNHardware) to your project
* Add the required driver source as well, from the [Drivers folder](Drivers)
* Use an example for the required driver from the [Examples folder](Examples) as a start for your program
* That's all !

### nanoFramework
It's as simple as :

* Create a nanoFramework application project in Visual Studio
* Add [**_MBNCore.cs_**](MBNCore) to your project
* Add **_nfHardware<device>.cs_** to your project
* Add the required driver source as well, from the [Drivers folder](Drivers)
* Use an example for the required driver from the [Examples folder](Examples) as a start for your program
* That's all !

## **How it works**

The main concept in MBNHardware is `Sockets`.
Sockets designate the physical Mikrobus sockets available on different boards.

Each socket has a number and is accessible in code using the following syntax :

```csharp
_transmitter = new T4_20mAClick(Hardware.SocketOne);  // Socket #1 on Ram board
_receiver = new T4_20mAClick(Hardware.SC20260_1);  // Socket #1 on SC20260D board
```

So far, the current boards are supported :

### TinyCLR
* MBN Ram (6 sockets)
* GHI SC20100 dev board (2 sockets)
* GHI SC20260 dev board (2 sockets)
* GHI FEZ Stick (2 sockets)

### nanoFramework
* STM32F769I-Discovery + Arduino Uno Click Shield

## **Virtual sockets**

If you don't use a Click module or if you connect it on the exposed board headers, then you may ask _'which socket should I use ?!'_

The answer is really simple : **create your own virtual socket !**

Imagine that the driver you want to use needs pins AN, RST, CS and INT. You would then create a virtual socket with the required pins :

```csharp
var virtualSocket = new Hardware.Socket
{
    AnPin = SC20260.GpioPin.PF10,
    Rst = SC20260.GpioPin.PI8,
    Cs = SC20260.GpioPin.PG12,
    Int = SC20260.GpioPin.PG6,
};
```

Then pass that socket to the driver :

```csharp
var _lcd = new DevantechLcd03(virtualSocket, 0xC8 >> 1);
var _lcdOnOff = GpioController.GetDefault().OpenPin(virtualSocket.Rst);
```
