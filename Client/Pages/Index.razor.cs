using AntDesign;
using IBApi;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using traderui.Client.Models;
using traderui.Shared;
using Action = traderui.Shared.Action;

namespace traderui.Client.Pages
{
    public partial class Index
    {
        // TODO: Split these properties into a domain object.

        public Random _randomNumberGenerator = new Random();
        public int AdrBarDataRequest { get; set; }
        public ADR AdrData { get; set; } = new ADR();
        public Input<double> priceInput;
        public Input<string> tickerInput;
        bool TransmitOrder { get; set; } = false;
        public bool IsBuyOrder { get; set; } = true;

        public bool IsSellOrder
        {
            get { return !IsBuyOrder; }
        }

        public DateTime DateTimeUpdated { get; set; }
        public string Symbol { get; set; }
        public string TempSymbol { get; set; }
        public bool SymbolLoaded { get; set; }
        public bool PriceLoaded { get; set; }
        public ContractDetails ContractDetails { get; set; }
        public double AccountSize { get; set; }
        public string AccountName { get; set; }
        public double BuyingPower { get; set; }
        public double AskPrice { get; set; }
        public double LowOfDay { get; set; }
        public double HighOfDay { get; set; }
        public bool UseLowOfDayAsStopLoss { get; set; }
        public double BidPrice { get; set; }
        public double ClosePrice { get; set; }
        public double MarketPrice { get; set; }
        public double OverridePrice { get; set; }
        public double OverrideStopLoss { get; set; }
        public bool SizeIsDirty { get; set; }
        public bool AllowPlaceOrder { get; set; }
        public OrderType OrderType { get; set; } = OrderType.MKT;
        public List<Position> Positions { get; set; } = new List<Position>();

        public double MaxLossInDollar { get; set; } = 0;

        public double DailyPnL { get; set; }
        public double UnrealizedPnL { get; set; }
        public double RealizedPnL { get; set; }

        public double RiskRatioTarget { get; set; } = 15;

        public double Price
        {
            get
            {
                if (OverridePrice > 0)
                {
                    PriceType = "Overridden";
                    return OverridePrice;
                }

                if (IsBuyOrder && AskPrice > 0)
                {
                    PriceType = "Ask";
                    return AskPrice;
                }

                if (IsSellOrder && BidPrice > 0)
                {
                    PriceType = "Bid";
                    return BidPrice;
                }

                if (MarketPrice > 0)
                {
                    PriceType = "Market";
                    return MarketPrice;
                }

                if (ClosePrice > 0)
                {
                    PriceType = "Close";
                    return ClosePrice;
                }

                return 0;
            }
        }

        public double StopLossAt { get; set; }
        public string PriceType { get; set; }
        public string StopLossType { get; set; }
        public double Size { get; set; }
        public double Risk { get; set; }
        public double PositionSize { get; set; }
        public double Cost { get; set; }
        public double MaxLoss { get; set; }

        public string LogMessages { get; set; }

        protected override async void OnInitialized()
        {
            AdrBarDataRequest = _randomNumberGenerator.Next();
            ContractDetails = new ContractDetails();
            Symbol = "";
            Risk = await localStorage.GetItemAsync<double>("risk"); // In percentage
            MaxLossInDollar = await localStorage.GetItemAsync<double>("maxLossInDollar");
            RiskRatioTarget = await localStorage.GetItemAsync<double>("riskRatioTarget");
            AccountSize = 5000.0; // In USD
            Cost = 0;
            MaxLoss = 0;
            BuyingPower = AccountSize;
            PositionSize = await localStorage.GetItemAsync<double>("positionSize"); // In percentage
            PriceLoaded = false;
            UseLowOfDayAsStopLoss = true;

            _message.Config(new MessageGlobalConfig {Top = 1, Duration = 1, MaxCount = 3,});

            TempSymbol = await localStorage.GetItemAsync<string>("symbol");
            if (!string.IsNullOrWhiteSpace(TempSymbol))
            {
                OnSymbolChange();
            }

            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(new Uri($"{NavigationManager.Uri}hub/broker"))
                .WithAutomaticReconnect()
                .Build();

            connection.Reconnected += async (connectionId) =>
            {
                await BrokerService.CancelSubscriptions(CancellationToken.None);
                await BrokerService.GetPositions(CancellationToken.None);
                await BrokerService.GetAccountSummary(false, CancellationToken.None);
            };

            connection.On("contractDetails", async (string contractDetails) =>
            {
                AdrData.Clear();
                ContractDetails = JsonSerializer.Deserialize<ContractDetails>(contractDetails);
                SymbolLoaded = true;
                await localStorage.SetItemAsync<string>("symbol", ContractDetails.Contract.Symbol);

                await BrokerService.GetTickerPrice(ContractDetails.Contract.Symbol, CancellationToken.None);
                await BrokerService.GetHistoricalBarData(ContractDetails.Contract.Symbol, AdrBarDataRequest, CancellationToken.None);
                StateHasChanged();
            });

            connection.On("errorCode", async (int id, int errorCode, string error) =>
            {
                switch (errorCode)
                {
                    case 200:
                        ResetForm(false);
                        await tickerInput.Focus(FocusBehavior.FocusAndClear);
                        break;
                }

                StateHasChanged();
            });

            connection.On("connectAck", (string error) => { AddLogMessage("Connection established"); });

            connection.On("position", async (string contract, double pos, double avgCost) =>
            {
                var positionContract = JsonSerializer.Deserialize<Contract>(contract);
                Position position = new Position {PositionId = positionContract.ConId, Contract = positionContract, Size = pos, AvgCost = avgCost};

                var ix = Positions.FindIndex(c => c.PositionId == position.PositionId);

                if (ix == -1)
                {
                    await BrokerService.GetTickerPnL(AccountName, position.PositionId, true, CancellationToken.None);
                    Positions.Add(position);
                }
                else if (ix > 0 && position.Size > 0)
                {
                    Positions[ix] = position;
                }
                else
                {
                    Positions.RemoveAt(ix);
                }
            });

            connection.On("historicalDataUpdate", (string bar) =>
            {
                var barData = JsonSerializer.Deserialize<Bar>(bar);
                HighOfDay = barData.High;
                LowOfDay = barData.Low;
            });

            connection.On("historicalData", (int requestId, string bar) =>
            {
                var barData = JsonSerializer.Deserialize<Bar>(bar);
                if (requestId.Equals(AdrBarDataRequest))
                {
                    AdrData.AddHistoricalData(barData);
                }
            });

            connection.On("historicalDataEnd", (int reqId, string start, string end) =>
            {
                if (reqId == AdrBarDataRequest)
                {
                    AdrData.CalculateDailyRange();
                }
            });

            connection.On("pnl", (double dailyPnL, double unrealizedPnL, double realizedPnL) =>
            {
                DailyPnL = dailyPnL;
                UnrealizedPnL = unrealizedPnL;
                RealizedPnL = realizedPnL;
                StateHasChanged();
            });

            connection.On("pnlSingle", (int reqId, decimal pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value) =>
            {
                var ix = Positions.FindIndex(c => c.PositionId == reqId);
                var position = Positions[ix];
                position.Daily = dailyPnL;
                position.Unrealized = unrealizedPnL;
                position.Realized = realizedPnL;
                position.Value = value;
                StateHasChanged();
            });

            connection.On("accountSummary", (string account, string tag, string value, string currency) =>
            {
                switch (tag)
                {
                    case "AvailableFunds":
                        BuyingPower = Convert.ToDouble(value);
                        break;

                    case "NetLiquidation":
                        AccountSize = Convert.ToDouble(value);
                        break;
                }

                AccountName = account;
                BrokerService.GetAccountSummary(true, CancellationToken.None);
                BrokerService.GetPnL(AccountName, CancellationToken.None);

                StateHasChanged();
            });

            connection.On("openOrder", (Contract contract, Order order, OrderState orderState) => { Console.WriteLine(orderState.Status); });

            connection.On("orderStatus", (string status, double filled, double remaining, double avgFillPrice) => { _message.Info($"{status} - {filled}/{remaining}"); });

            connection.On("tickPrice", (int field, double value) =>
            {
                // https://interactivebrokers.github.io/tws-api/tick_types.html
                if (value > 0)
                {
                    switch (field)
                    {
                        case 1: // Bid
                            BidPrice = value;
                            break;

                        case 2: // Ask
                            AskPrice = value;
                            break;

                        case 7: // Low
                            LowOfDay = value;
                            break;

                        case 9: // Close Price
                            ClosePrice = value;
                            break;

                        case 37: // Market Price
                            MarketPrice = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                            break;
                    }

                    DateTimeUpdated = DateTime.Now;
                    PriceLoaded = true;
                    RecalculateNumbers();
                }
            });

            connection.On("tickByTickBidAsk", (double bid, double ask) =>
            {
                AskPrice = ask;
                BidPrice = bid;
                RecalculateNumbers();
            });

            connection.On<string>("log", (obj) =>
            {
                AddLogMessage(obj);
                StateHasChanged();
            });
            await connection.StartAsync();

            await BrokerService.GetAccountSummary(false, CancellationToken.None);
            await BrokerService.GetPositions(CancellationToken.None);
        }

        private void AddLogMessage(string message)
        {
            LogMessages = $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {message}\n\r" + LogMessages;
        }

        public async void OnSymbolChange()
        {
            AdrData.Clear();
            Symbol = TempSymbol;
            await BrokerService.GetTicker(Symbol, CancellationToken.None);
            await BrokerService.GetHistoricalBarData(Symbol, AdrBarDataRequest, CancellationToken.None);
            StateHasChanged();
        }

        public void OnAccountSizeChange(double v)
        {
            AccountSize = v;
            Size = Price > 0 ? AccountSize / Price : 0;
        }

        public void RecalculateNumbers()
        {
            // RecalculatePositionSize();
            RecalculateCost();
            StateHasChanged();
        }

        private void RecalculateCost()
        {
            if (OverrideStopLoss <= 0 && UseLowOfDayAsStopLoss && LowOfDay > 0 && IsBuyOrder)
            {
                StopLossAt = LowOfDay;
                StopLossType = "LOD";
            }
            else if (OverrideStopLoss <= 0 && !IsBuyOrder && HighOfDay > 0)
            {
                StopLossAt = HighOfDay;
                StopLossType = "HID";
            }
            else if (OverrideStopLoss > 0)
            {
                StopLossAt = OverrideStopLoss;
                StopLossType = "Override";
            }
            else
            {
                StopLossAt = Math.Round(Price - (Price * (1 - (100 - Risk) / 100)), 2, MidpointRounding.AwayFromZero);
                StopLossType = "%";
            }

            RecalculatePositionSize();
            Cost = Math.Round((Price * Size), 2, MidpointRounding.AwayFromZero);
            MaxLoss = Math.Round((Price - StopLossAt) * Size, 2, MidpointRounding.AwayFromZero);
            CalculateAllowPlaceOrder();
        }

        private void RecalculatePositionSize()
        {
            if (!SizeIsDirty)
            {
                if (MaxLossInDollar > 0)
                {
                    var maxLossSize = MaxLossInDollar / (MarketPrice - StopLossAt);
                    Size = Math.Round(maxLossSize, MidpointRounding.ToZero);
                }
                else
                {
                    Size = Math.Round(AccountSize * (1 - (100 - PositionSize) / 100) / Price, 0, MidpointRounding.AwayFromZero);
                }
            }
        }

        public void OnSizeChange(double v)
        {
            Size = v;
            SizeIsDirty = true;
            RecalculateNumbers();
        }

        public void OnStopLossChange(double v)
        {
            OverrideStopLoss = v;
            RecalculateNumbers();
        }

        public async void OnRiskChange(double v)
        {
            Risk = v;
            await localStorage.SetItemAsync<double>("risk", Risk);
            RecalculateNumbers();
        }

        public async void OnPositionSizeChange(double v)
        {
            PositionSize = v;
            await localStorage.SetItemAsync<double>("positionSize", PositionSize);
            RecalculateNumbers();
        }

        public void OnReceivedFocus()
        {
            // tickerInput.Focus(FocusBehavior.FocusAndSelectAll);
        }

        public void OnResetPriceClick()
        {
            SizeIsDirty = false;
            PriceLoaded = false;
            PriceType = "Price";
            OverridePrice = 0;
            OverrideStopLoss = 0;
            BrokerService.GetTickerPrice(Symbol, CancellationToken.None);
        }

        public void PlaceBuyOrder()
        {
            WebOrder webOrder = new WebOrder
            {
                Contract = new Contract {Currency = "USD", Symbol = Symbol, SecType = "STK", Exchange = "SMART"},
                Action = Action.BUY,
                OrderType = OrderType,
                Price = Price,
                Qty = Size,
                Transmit = TransmitOrder,
                LmtPrice = Price,
                StopLoss = true,
                StopLossAt = StopLossAt,
            };

            BrokerService.BuyOrder(Symbol, webOrder, CancellationToken.None);
        }

        public void PlaceSellOrder()
        {
            WebOrder webOrder = new WebOrder
            {
                Contract = new Contract {Currency = "USD", Symbol = Symbol, SecType = "STK", Exchange = "SMART"},
                Action = Action.SELL,
                OrderType = OrderType,
                Price = Price,
                Qty = Size,
                Transmit = TransmitOrder,
                LmtPrice = Price,
                StopLoss = false,
                StopLossAt = StopLossAt,
            };

            BrokerService.BuyOrder(Symbol, webOrder, CancellationToken.None);
        }

        private void CalculateAllowPlaceOrder()
        {
            AllowPlaceOrder = (Size > 0
                               && Price > 0
                               && Cost < BuyingPower
                               && PriceLoaded
                               && !string.IsNullOrWhiteSpace(Symbol));
        }

        private void PlaceOrder()
        {
            if (AllowPlaceOrder)
            {
                if (IsBuyOrder)
                {
                    PlaceBuyOrder();
                }
                else
                {
                    PlaceSellOrder();
                }
            }
        }

        private async void ResetForm(bool reload = true)
        {
            await localStorage.RemoveItemAsync("symbol");
            SymbolLoaded = false;
            AllowPlaceOrder = false;
            Symbol = string.Empty;
            PriceType = string.Empty;
            ContractDetails = new ContractDetails();
            TempSymbol = "";
            PriceLoaded = false;
            SizeIsDirty = false;
            LowOfDay = 0;
            MarketPrice = 0;
            Size = 0;
            AskPrice = 0;
            BidPrice = 0;
            ClosePrice = 0;
            OverridePrice = 0;
            OverrideStopLoss = 0;

            if (reload)
            {
                NavigationManager.NavigateTo("/", true);
            }

            //OnPriceChange();
            RecalculatePositionSize();
            RecalculateNumbers();
            StateHasChanged();
        }

        private void OnOverridePriceChange(double value)
        {
            OverridePrice = value;
        }

        private async void OnMaxLossChange(double d)
        {
            MaxLossInDollar = d;
            await localStorage.SetItemAsync("maxLossInDollar", MaxLossInDollar); // In percentage
            RecalculateNumbers();
        }

        private async void OnRiskRatioChange(double d)
        {
            RiskRatioTarget = d;
            await localStorage.SetItemAsync("riskRatioTarget", RiskRatioTarget); // In percentage
            RecalculateNumbers();
        }
    }
}
