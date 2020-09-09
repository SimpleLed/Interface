# SimpleLed

API Documentation available at our [Github Pages](https://simpleled.github.io/Interface/)

## Overview
the SimpleLed SDK (SLS) is a simple but powerful interface to develop "drivers" for RGB hardware, such as motherboards or USB fan controllers. It is designed as a simple and singular interface to sit on top of the myriad of protocols and transports that is the RGB landscape right now. It also allows the creation of "source drivers" - which do not connect to real hardware but act as a source, simulating a device.

## Inspiration

SimpleLed is highly inspired by [RGB.Net](https://github.com/DarthAffe/RGB.NET) which already sets out to acheive the same goal as SimpleLed. Where SimpleLed deviates is RGB.NET has many abstractions and layers that simply are not required for the vast majority of RGB applications and this complicated creating drivers. SimpleLed strives to offer a powerful platform that is as slim as possible, simplifying both creating new drivers and the consumption of them.

## RGB.NET porting
It is trivial to port a driver from RGB.NET to SimpleLed as they share a number of common patterns. Currently if you wish for 2D animations (mainly used on keyboards) etc, RGB.NET is probably the better choice, though this is something we are working towards. We are also working towards offering an RGB.NET driver for SimpleLed allowing you to load RGB.NET drivers into SimpleLed supporting apps and we may even offer the reverse, loading SimpleLed drivers into RGB.NET supported applications.

## Projects using SimpleLed

### JackNet RGBSync

The next version of [RGB Sync](https://github.com/rgbsync) has migrated from RGB.Net to SimpleLed and in doing so has had a huge overhaul because of the advanced functionality meaning a simplified user experience and leveraging SimpleLeds "source driver" concept.

