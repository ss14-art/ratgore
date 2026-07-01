ent-EconomicConsole = economic monitor
    .desc = A terminal for monitoring market prices and stock trends.
ent-StockMarketCartridge = stock market cartridge
    .desc = A cartridge for trading stocks and monitoring market trends.
cmd-viewprices-desc = View current dynamic prices for all tracked goods
cmd-viewprices-help = Usage: {$command} [goodId] - Shows all prices or specific good
cmd-viewprices-good-not-found = Good '{$goodId}' not found in dynamic pricing system.
cmd-viewprices-available-goods = Available goods:
cmd-viewprices-header === Dynamic Pricing Overview ===
cmd-viewprices-column-good-id = Good ID
cmd-viewprices-column-multiplier = Multiplier
cmd-viewprices-column-change = Change
cmd-viewprices-total = Total tracked goods: {$count}
cmd-forceupdateprices-desc = Force update all dynamic prices immediately
cmd-forceupdateprices-help = Usage: {$command} - Forces immediate price recalculation
cmd-forceupdateprices-success = All prices have been force updated.
cmd-resetprices-desc = Reset all dynamic prices to baseline (1.0 multiplier)
cmd-resetprices-help = Usage: {$command} - Resets all prices
cmd-resetprices-success = All prices have been reset to baseline.
cmd-viewshuttleprices-desc = View current shuttle prices
cmd-viewshuttleprices-help = Usage: {$command} - Shows all shuttle prices
cmd-viewshuttleprices-header === Shuttle Pricing Overview ===
cmd-viewshuttleprices-column-entity = Entity ID
cmd-viewshuttleprices-column-base = Base Price
cmd-viewshuttleprices-column-current = Current Price
cmd-viewshuttleprices-column-change = Change
cmd-viewshuttleprices-total = Total shuttles tracked: {$count}
cmd-viewshuttleprices-none = No shuttles with dynamic pricing found.
rat-economy-log-price-change = {$goodId} price changed by {$deviation ->
    [one] {$deviation}%
    *[other] {$deviation}%
} (multiplier: {$multiplier})
rat-economy-log-shuttle-price-change = Shuttle {$shuttleUid} price changed by {$percentChange ->
    [one] {$percentChange}%
    *[other] {$percentChange}%
} to {$currentPrice} credits
price-monitor-window-title = Economic Monitor
price-monitor-tab-prices = Trade Goods
price-monitor-tab-shuttles = Shuttles
price-monitor-refresh = Refresh
price-monitor-auto-update = Auto Update
price-monitor-good-id = Good ID
price-monitor-base-price = Base Price
price-monitor-current-price = Current Price
price-monitor-multiplier = Multiplier
price-monitor-change = Change
price-monitor-shuttle-name = Shuttle
cmd-setmarkettrend-desc = Set market trend direction for specific goods
cmd-setmarkettrend-help = Usage: {$command} <goodId> <bullish|bearish|volatile|stable> [strength] [duration]
cmd-setmarkettrend-invalid-direction = Invalid trend direction. Valid: bullish, bearish, volatile, stable
cmd-setmarkettrend-valid-directions = Valid directions: bullish (prices up), bearish (prices down), volatile (high fluctuation), stable (low fluctuation)
cmd-setmarkettrend-invalid-strength = Invalid strength value. Must be a number (0.1-10.0)
cmd-setmarkettrend-invalid-duration = Invalid duration value. Must be seconds (60-3600)
cmd-setmarkettrend-success = Set market trend for {$goodId}: {$direction} (strength: {$strength}, duration: {$duration}s)
cmd-crashmarket-desc = Crash the market (force all prices down)
cmd-crashmarket-executed = Market crashed! All prices decreased significantly.
cmd-volatilemarket-desc = Make market highly volatile
cmd-volatilemarket-executed = Market is now highly volatile!
cmd-stabilizemarket-desc = Stabilize the market (reset all prices)
cmd-stabilizemarket-executed = Market stabilized. All prices reset to baseline.
stock-market-buy = Buy
stock-market-sell = Sell
stock-market-price = Price
stock-market-shares = Shares
stock-market-value = Value
stock-market-trend = Trend
stock-market-buy-success = Bought {$amount} shares of {$company} for {$cost} credits
stock-market-buy-fail = Insufficient funds to buy shares
stock-market-sell-success = Sold {$amount} shares of {$company} for {$profit} credits
stock-market-sell-fail = You don't have enough shares to sell
economic-console-title = Economic Monitor Console
economic-console-loading = Loading market data...
economic-console-no-data = No market data available
economic-console-status = Tracking {$goods} goods and {$stocks} shuttles
economic-console-status-count = Tracking: {$count}
economic-console-search = Search
economic-console-search-placeholder = Filter...
economic-console-no-results = No items match your search
economic-console-more-items = ... and {$count} more items
economic-console-prices = Market Prices
economic-console-items = Items
economic-console-shuttles = Shuttles
economic-console-stocks = Stock Market
economic-console-admin = Admin Controls
economic-console-refresh = Refresh
economic-console-auto-update = Auto-Update
economic-console-trend-up = UP
economic-console-trend-down = DOWN
economic-console-trend-stable = STABLE
economic-console-trend-volatile = VOLATILE
stock-market-cartridge-name = Stock Market
stock-market-cartridge-desc = Trade stocks and monitor market trends
stock-market-app-title = Stock Market
stock-market-portfolio = Portfolio
stock-market-available = Stocks
stock-market-owned = Owned
stock-market-amount = Amount
stock-market-total = Total
stock-market-buy-btn = Buy
stock-market-sell-btn = Sell
stock-market-refresh = Refresh
stock-market-no-portfolio = You don't own any stocks yet
stock-market-price-up = [color=lime]UP[/color]
stock-market-price-down = [color=red]DOWN[/color]
stock-company-tpsh = TPSH
stock-company-kvst = KVST
stock-company-bms = BMS
stock-company-lrnp = LRNP
