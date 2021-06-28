# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Updated metadata to keep capitalization of ShipStation consistent

## [0.0.43] - 2021-04-12

### Changed

- Updated metadata for submission to app store

## [0.0.42] - 2021-03-25

### Fixed

- Fix for shipping tax

## [0.0.41] - 2021-03-22

### Fixed

- Fix for null Sla

## [0.0.40] - 2021-03-19

### Fixed

- Fix for null Sla

## [0.0.39] - 2021-03-16

### Fixed

- order domain filter

## [0.0.38] - 2021-03-16

### Fixed

- null fix

## [0.0.37] - 2021-03-15

## [0.0.36] - 2021-03-15

### Fixed

- null fix

## [0.0.35] - 2021-03-15

### Changed

- Changed Marketplace setting to dropdown

### Fixed

- Do not add Shipping tax to items

## [0.0.34] - 2021-03-11

### Fixed

- Bill to information

## [0.0.33] - 2021-03-10

### Fixed

- Handle erros if warehouse data is missing

## [0.0.32] - 2021-02-01

### Fixed

- For split shipments get tax from orderform

## [0.0.31] - 2021-01-19

### Fixed

- Typos

### Added

- Add UPC to item options

## [0.0.30] - 2021-01-14

### Changed

- Use dock name for warehouse name in item options

## [0.0.29] - 2021-01-12

### Added

- Option to show Payment Method in Custom 1

## [0.0.28] - 2021-01-12

- Check for missed order on handling and start-handling notification

## [0.0.27] - 2021-01-08

### Changed

- Send order to ShipStation on handling and start-handling

## [0.0.26] - 2021-01-08

### Added

- Debug log message

## [0.0.25] - 2021-01-06

### Added

- Option to add Dock name to Item Options
- Create Warehouses in ShipStation from Vtex Docks

## [0.0.24] - 2020-12-29

### Fixed

- Fixed JSON error

## [0.0.23] - 2020-12-29

### Added

- Added option to use RefId for Sku
- Added option to send Sku Specifications

## [0.0.22] - 2020-12-29

### Changed

- Changed field for shipping method

### Added

- Check for order that was plit and partially cancelled in ShipStation that is not cancelled in Vtex

## [0.0.21] - 2020-12-10

### Added

- Check for order cancelled in ShipStation that are not cancelled in Vtex

## [0.0.20] - 2020-12-07

### Changed

- Skip orders with a blank status

## [0.0.19] - 2020-12-04

### Changed

- Changed vendor to vtex
- Updated billing options
- Added "cancelled" state

## [0.0.18] - 2020-11-13

### Changed

- Changed vendor to vtexus

## [0.0.17] - 2020-11-10

### Changed

- Tax rounding fix
- Input Values formatting

## [0.0.16] - 2020-11-06

### Changed

- SetOrderStatus to http

## [0.0.15] - 2020-11-06

### Changed

- SetOrderStatus outbound access

## [0.0.14] - 2020-11-06

### Changed

- Added error handling around setting order status to start-handling

## [0.0.13] - 2020-11-06

### Changed

- If all items have shipped, force invoice total to remaining

## [0.0.12] - 2020-11-06

### Changed

- debugging

## [0.0.11] - 2020-11-06

### Changed

- Force order status update for debug

## [0.0.10] - 2020-11-06

### Changed

- Added more servers to outbound access

## [0.0.9] - 2020-11-06

### Changed

- Changed vendor to vtex

## [0.0.8] - 2020-11-05

### Changed

- Handle ship notifications for orders that are not found

## [0.0.7] - 2020-11-03

### Changed

- Changed vendor to vtexus

## [0.0.6] - 2020-11-03

### Changed

- Admin page

## [0.0.5] - 2020-10-30

### Added

- Admin page

## [0.0.4] - 2020-10-19

### Changed

- Added Complement field to shipping address
- Removed Brand Id and Category Id

## [0.0.3] - 2020-10-16

### Changed

- Hook to Broadcast

## [0.0.2] - 2020-10-15

### Changed

- Test version

## [0.0.1] - 2020-09-22

### Added

- Initial version
