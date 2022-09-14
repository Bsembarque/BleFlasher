# BleFlasher

BleFlasher is a librairie and UI to update embedded devices over BLE, written in .NET 6.0 MAUI.
Tested on Android and Windows 10. 

## BLE GATT
* Service UID 42535331-0000-1000-8000-00805F9B34FB
* Characteristic UID 42534331-0000-1000-8000-00805F9B34FB

## FRAME FORMAT

### HEADER
16 bytes header

[b0...4->COMMAND | b4...7->STATUS][ADDR0][ADDR1][ADDR2][ADDR3][SIZE0][SIZE1][SIZE2][SIZE3][DUMMY0][DUMMY1][DUMMY2]

#### DESCRIPTION
* COMMAND : NO_COMMAND=0x0, COMMAND_ERASE=0x1, COMMAND_WRITE=0x2, COMMAND_READ=0x3, COMMAND_START=0x4, COMMMAND_RESET=0x5
* STATUS : COMMAND_REQUEST=0x0, COMMAND_ACK=0x01, COMMAND_ERR=0x02

### DATA
128 bytes raw data
            
# OTHERS
Offer an STM32WB Bootloader ELF example.
Use BleFlasher_UI to get example or embedded testing.
