# Web interface for Interactive Brokers TWS

This is a simple UI for submitting orders with attached Stop-Loss and correct Position Size based on predefined variables.

### Disclaimer

This software is provided as is and should be used at your own risk. For one reason or another you will most likely lose money while using this software :)

This software could for whatever reason and at any point be archived or discontinued.

## Prerequisites

- IBKR TWS must be started and configured for allowing ActiveX and Socket Clients.

![APIConfiguration](https://user-images.githubusercontent.com/27571840/159119439-269f6998-8aa4-4a04-9033-272d11cbbdf3.png)

# Start
To get going using TraderUI you have three options. 
1. Download the latest release from [Release](https://github.com/aklepaker/traderui/releases)
2. Use Docker image from [Packages](https://github.com/aklepaker/traderui/pkgs/container/traderui)
3. Build the application from scratch on your own machine

## 1. Download the latest release 
After downloading the latest release unzip the files in a proper place and folder. 
Start IBKR TWS, and when logged in start the application by running the `traderui.exe`. From your favorite browser open `http://localhost:5000`

## 2. Docker
You may also run this with Docker, but be sure to set correct Server address (the IP of the machine running TWS) and ensure to update the tag of the image to correct version.

Start TWS and log in with your username and password. 
Then run
`docker run -it --rm --name tradeui -p 5000:5000 -e "ServerOptions:Server=192.168.1.2" ghcr.io/aklepaker/traderui:0.2.1.22-5816236`

Remember to update the TWS settings allowing for non-localhost addresses and add the IP of the docker host.

![image](https://user-images.githubusercontent.com/27571840/165388593-da5d3c5d-2a8d-4cd1-b441-67e3ce16db41.png)

## 3. Build on your own machine.
When doing this you should be somewhat familiar with git and Microsoft development enviornments i.e .Net 6. and be familiar with `dotnet` commands for compiling. 

Due to license restriction the API Source code must be downloaded from https://interactivebrokers.github.io/ prior to compilation.

### Clone this repository
You need to clone this repository to your local machine. 

```
git clone https://github.com/aklepaker/traderui.git
```

### Download IBKR TWS API Manually

1. Download and install the TWS API from https://interactivebrokers.github.io/downloads/TWS%20API%20Install%20976.01.msi
1. Copy the files from `source\CSharpClient\client` to `.\ibkr-api` in the repository folder.

### Download and install IBKR TWS API with Powershell

Run these commands in the repository folder

```powershell
wget https://interactivebrokers.github.io/downloads/TWS%20API%20Install%20976.01.msi -OutFile $env:TEMP"\TWS_API.msi"
Start-Process msiexec.exe -Wait -ArgumentList "/i $env:TEMP\TWS_API.msi /quiet /qn TARGETDIR=$env:TEMP EULA=1"
Copy-Item -Recurse -Path $env:TEMP"\TWS API\source\CSharpClient\client\*" .\ibkr-api\
Start-Process msiexec.exe -Wait -ArgumentList "/x $env:TEMP\TWS_API.msi /quiet /qn"
Remove-Item -Path $env:TEMP"\TWS_API.msi"
```

When the TWS API is installed and copied to the correct folder you should be able to run `traderui` with 
```
dotnet run --project .\Server\traderui.Server.csproj
```

You should now be able to open http://localhost:5124 in your browser. 

## Functional overview of the trading screen

![Overview](https://user-images.githubusercontent.com/27571840/159007766-4f35b72a-c471-4c17-96ca-4260589144b3.png)
