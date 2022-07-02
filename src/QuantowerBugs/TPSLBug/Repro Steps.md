# How to reproduce the TPSL Bug
## QT Versions Tested
 1. 1.124.15
 2. 1.124.4

## Backtesting Environment
 - Account: Any AMP/CQG account
 - Symbol: MESU22
 - Start Date/Time: 2002-06-15 14:00 -00:00
 - End Date/Time: 2002-06-18 00:00 -00:00
 - Build from: Tick
 - Executing Type: Last

## Problem
The position is not closed when the stop loss price is triggered.  With an order quantity of five, only 3 of the 5 contracts are sold when the stop loss is triggered.  The other contracts are sold a seemingly random point in the many hours/days in the future.  There are thousands of opportunities for either the stop loss or take profit orders to be filled before they are.