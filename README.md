# Web interface for Interactive Brokers TWS

This is a simple UI for submitting orders with attached Stop-Loss and correct Position Size based on predefined variables.

## Disclaimer

This software is provided as is and should be used at your own risk. For one reason or another you will most likely lose money while using this software :)

This software could for whatever reason and at any point be archived or discontinued.

## Start
Start the application by running the `traderui.exe` file from the build, or downloaded release. From your favorite browser open `http://localhost:5000`

## TODOs

Since this project started out as a proof of concept for testing and getting to know the IBKR API there is a lot shortcuts and not-that-good approaches that should be fixed
before this source code stable and maintainable.

Functional changes:

- [ ] UI should allow for multiple ticker tabs

Architecture changes

- [ ] All API requests should have a requestId when initialized from front-end.
- [ ] Split up into components and SSOT (Flux/Redux pattern)

## Known bugs

- [ ] PnL for positions does not update outside of RTH

## Prerequisites

- IBKR TWS must be started and configured for allowing ActiveX and Socket Clients.

![APIConfiguration](https://user-images.githubusercontent.com/27571840/159119439-269f6998-8aa4-4a04-9033-272d11cbbdf3.png)

## Building from scratch.
Due to license restriction the API Source code must be downloaded from https://interactivebrokers.github.io/ prior to compilation.

Download and install Interactive Brokers TWS API.

### Manually

1. Download and install the TWS API from https://interactivebrokers.github.io/downloads/TWS%20API%20Install%20976.01.msi
1. Copy the files from `source\CSharpClient\client` to `.\ibkr-api` in the repository folder.

### Powershell

Run these commands in the repository folder

```powershell
wget https://interactivebrokers.github.io/downloads/TWS%20API%20Install%20976.01.msi -OutFile $env:TEMP"\TWS_API.msi"
Start-Process msiexec.exe -Wait -ArgumentList "/i $env:TEMP\TWS_API.msi /quiet /qn TARGETDIR=$env:TEMP EULA=1"
Copy-Item -Recurse -Path $env:TEMP"\TWS API\source\CSharpClient\client\*" .\ibkr-api\
Start-Process msiexec.exe -Wait -ArgumentList "/x $env:TEMP\TWS_API.msi /quiet /qn"
Remove-Item -Path $env:TEMP"\TWS_API.msi"
```

## Functional overview of the trading screen

![Overview](https://user-images.githubusercontent.com/27571840/159007766-4f35b72a-c471-4c17-96ca-4260589144b3.png)
